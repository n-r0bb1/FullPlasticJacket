using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Objects;

namespace TheAdventure;

/// <summary>
/// Central game class.  Owns all systems, drives the main loop, and implements IDisposable
/// to guarantee SDL resource cleanup.
/// </summary>
public sealed class Game : IDisposable
{
    // ── Constants ────────────────────────────────────────────────────────────
    private const int ScreenWidth    = 600;
    private const int ScreenHeight   = 600;

    private const int RoomX          = 0;
    private const int RoomY          = 0;
    private const int RoomWidth      = 600;
    private const int RoomHeight     = 600;
    private const int WallThickness  = 24;

    private const int UpgradeCardW   = 140;
    private const int UpgradeCardH   = 80;
    private const int UpgradeCardGap = 10;

    // ── SDL handles ──────────────────────────────────────────────────────────
    private readonly Sdl    _sdl;
    private readonly IntPtr _window;
    private readonly IntPtr _renderer;

    // ── Textures ─────────────────────────────────────────────────────────────
    private unsafe Texture* _texPlayer;
    private unsafe Texture* _texEnemy;
    private unsafe Texture* _texArena;

    // ── Input state ──────────────────────────────────────────────────────────
    // ReadOnlySpan<byte> cannot be a field; we store a raw pointer and span per-call.
    private unsafe byte* _keyboardStatePtr;
    private readonly byte[] _mouseButtons     = new byte[(int)MouseButton.Count];
    private readonly byte[] _prevMouseButtons = new byte[(int)MouseButton.Count];
    private int _mouseX;
    private int _mouseY;

    // ── Game systems ─────────────────────────────────────────────────────────
    private readonly Camera      _camera;
    private readonly SaveManager _saveManager;
    private Player _player;

    private readonly List<Enemy>  _enemies = new();
    private readonly List<Bullet> _bullets = new();
    private readonly Random _rng = new();

    // ── Upgrade selection ────────────────────────────────────────────────────
    private readonly List<Upgrade> _upgradePool;
    private List<Upgrade> _offeredUpgrades = new();

    // ── State ────────────────────────────────────────────────────────────────
    private GameState _state        = GameState.Playing;
    private int  _currentRound      = 1;
    private int  _score;
    private int  _highScore;
    private bool _running           = true;

    // Total elapsed time in seconds — used for fire-rate timing.
    private double _totalTime;

    // ── Constructor ──────────────────────────────────────────────────────────
    public unsafe Game(Sdl sdl, IntPtr window, IntPtr renderer)
    {
        _sdl      = sdl;
        _window   = window;
        _renderer = renderer;

        _keyboardStatePtr = sdl.GetKeyboardState(null);

        _camera      = new Camera(ScreenWidth, ScreenHeight);
        _camera.Follow(new Vector2(RoomX + RoomWidth / 2f, RoomY + RoomHeight / 2f));
        _saveManager = new SaveManager();
        _highScore   = _saveManager.LoadHighScore();

        _upgradePool = UpgradePool.Build();

        unsafe
        {
            var r = (Renderer*)renderer;
            _texPlayer = TextureLoader.Load(sdl, r, Path.Combine("assets", "player.png"));
            _texEnemy  = TextureLoader.Load(sdl, r, Path.Combine("assets", "enemy.png"));
            _texArena  = TextureLoader.Load(sdl, r, Path.Combine("assets", "arena.png"));
        }

        _player = new Player(new Vector2(RoomX + RoomWidth / 2f, RoomY + RoomHeight / 2f));
        _camera.Follow(_player.Position);

        StartRound(_currentRound);
    }

    // ── Public API ───────────────────────────────────────────────────────────

    public bool HandleEvent(ref Event ev)
    {
        switch (ev.Type)
        {
            case (uint)EventType.Quit:
                _running = false;
                return false;

            case (uint)EventType.Mousemotion:
                _mouseX = ev.Motion.X;
                _mouseY = ev.Motion.Y;
                break;

            case (uint)EventType.Mousebuttondown:
                if (ev.Button.Button < _mouseButtons.Length)
                    _mouseButtons[ev.Button.Button] = 1;
                break;

            case (uint)EventType.Mousebuttonup:
                if (ev.Button.Button < _mouseButtons.Length)
                    _mouseButtons[ev.Button.Button] = 0;
                break;

            case (uint)EventType.Fingerdown:
                _mouseButtons[(byte)MouseButton.Primary] = 1;
                break;

            case (uint)EventType.Fingerup:
                _mouseButtons[(byte)MouseButton.Primary] = 0;
                break;

            case (uint)EventType.Windowevent:
                if (ev.Window.Event == (byte)WindowEventID.TakeFocus)
                    unsafe { _sdl.SetWindowInputFocus(_sdl.GetWindowFromID(ev.Window.WindowID)); }
                break;

            case (uint)EventType.Keydown:
                if ((int)ev.Key.Keysym.Scancode == (int)KeyCode.R && _state == GameState.GameOver)
                    Restart();
                break;
        }

        return _running;
    }

    public void Update(double deltaSeconds)
    {
        _totalTime += deltaSeconds;
        float dt = (float)deltaSeconds;

        // Pattern matching on game state.
        switch (_state)
        {
            case GameState.Playing:
                UpdatePlaying(dt);
                break;

            case GameState.BetweenRounds:
                if (_mouseButtons[(byte)MouseButton.Primary] == 1 &&
                    _prevMouseButtons[(byte)MouseButton.Primary] == 0)
                {
                    int totalW = _offeredUpgrades.Count * UpgradeCardW +
                                 (_offeredUpgrades.Count - 1) * UpgradeCardGap;
                    int startX = (ScreenWidth  - totalW) / 2;
                    int startY = (ScreenHeight - UpgradeCardH) / 2;
                    for (int i = 0; i < _offeredUpgrades.Count; i++)
                    {
                        int cx = startX + i * (UpgradeCardW + UpgradeCardGap);
                        if (_mouseX >= cx && _mouseX <= cx + UpgradeCardW &&
                            _mouseY >= startY && _mouseY <= startY + UpgradeCardH)
                        {
                            ApplyUpgrade(_offeredUpgrades[i]);
                            break;
                        }
                    }
                }
                break;

            case GameState.GameOver:
                break;
        }

        Array.Copy(_mouseButtons, _prevMouseButtons, _mouseButtons.Length);
    }

    /*Start of AI generated code*/
    public void Render()
    {
        unsafe
        {
            var r = (Renderer*)_renderer;
            _sdl.SetRenderDrawColor(r, 20, 20, 20, 255);
            _sdl.RenderClear(r);

            switch (_state)
            {
                case GameState.Playing:
                    RenderWorld(r);
                    RenderHud(r);
                    break;

                case GameState.BetweenRounds:
                    RenderWorld(r);
                    RenderHud(r);
                    RenderUpgradeOverlay(r);
                    break;

                case GameState.GameOver:
                    RenderWorld(r);
                    RenderHud(r);
                    RenderGameOver(r);
                    break;
            }

            _sdl.RenderPresent(r);
        }
    }
    /*End of AI generated code*/

    // ── IDisposable ──────────────────────────────────────────────────────────
    public void Dispose()
    {
        if (_score > _highScore)
        {
            _highScore = _score;
            _ = _saveManager.SaveHighScoreAsync(_highScore);
        }

        // NOTE: We only destroy the renderer here; window was created in Program.cs
        // but Game.Dispose handles it per the ownership contract passed in the ctor.
        unsafe
        {
            _sdl.DestroyTexture(_texPlayer);
            _sdl.DestroyTexture(_texEnemy);
            _sdl.DestroyTexture(_texArena);
            _sdl.DestroyRenderer((Renderer*)_renderer);
            _sdl.DestroyWindow((Window*)_window);
        }
    }

    // ── Private: state management ────────────────────────────────────────────

    private void StartRound(int round)
    {
        _enemies.Clear();
        _bullets.Clear();
        SpawnWave(round);
        _state = GameState.Playing;
    }

    private void Restart()
    {
        _player = new Player(new Vector2(RoomX + RoomWidth / 2f, RoomY + RoomHeight / 2f));
        _enemies.Clear();
        _bullets.Clear();
        _score = 0;
        _currentRound = 1;
        StartRound(_currentRound);
    }

    private void EnterBetweenRounds()
    {
        _state = GameState.BetweenRounds;
        // LINQ: shuffle via OrderBy random key, then take N.
        _offeredUpgrades = _upgradePool.OrderBy(_ => _rng.Next()).Take(3).ToList();
    }

    private void ApplyUpgrade(Upgrade upgrade)
    {
        upgrade.Apply(_player);
        _currentRound++;
        StartRound(_currentRound);
    }

    // ── Private: wave spawning ────────────────────────────────────────────────

    private void SpawnWave(int round)
    {
        int count    = 5 + round * 2;
        int health   = 20 + round * 5;
        float speed  = 80f + round * 4f;
        int damage   = 5 + round;

        for (int i = 0; i < count; i++)
            _enemies.Add(new Enemy(RandomEdgePosition(), health, speed, damage));
    }

    private Vector2 RandomEdgePosition()
    {
        float margin = WallThickness + 20f;
        return _rng.Next(4) switch
        {
            0 => new Vector2(_rng.NextSingle() * (RoomWidth  - margin * 2) + RoomX + margin,
                             RoomY + margin),
            1 => new Vector2(_rng.NextSingle() * (RoomWidth  - margin * 2) + RoomX + margin,
                             RoomY + RoomHeight - margin),
            2 => new Vector2(RoomX + margin,
                             _rng.NextSingle() * (RoomHeight - margin * 2) + RoomY + margin),
            _ => new Vector2(RoomX + RoomWidth - margin,
                             _rng.NextSingle() * (RoomHeight - margin * 2) + RoomY + margin),
        };
    }

    // ── Private: update ───────────────────────────────────────────────────────

    private void UpdatePlaying(float dt)
    {
        MovePlayer(dt);
        TryShoot();
        UpdateBullets(dt);
        UpdateEnemies(dt);
        CheckBulletEnemyCollisions();
        CheckPlayerEnemyCollisions();

        if (!_player.IsAlive)
        {
            if (_score > _highScore)
            {
                _highScore = _score;
                _ = _saveManager.SaveHighScoreAsync(_highScore);
            }
            _state = GameState.GameOver;
            return;
        }

        // LINQ: check whether any enemies are still alive.
        if (!_enemies.Any(e => e.Active))
            EnterBetweenRounds();
    }

    private unsafe void MovePlayer(float dt)
    {
        var ks  = new ReadOnlySpan<byte>(_keyboardStatePtr, (int)KeyCode.Count);
        var dir = Vector2.Zero;

        if (ks[(byte)KeyCode.W] > 0 || ks[(byte)KeyCode.Up]    > 0) dir.Y -= 1;
        if (ks[(byte)KeyCode.S] > 0 || ks[(byte)KeyCode.Down]  > 0) dir.Y += 1;
        if (ks[(byte)KeyCode.A] > 0 || ks[(byte)KeyCode.Left]  > 0) dir.X -= 1;
        if (ks[(byte)KeyCode.D] > 0 || ks[(byte)KeyCode.Right] > 0) dir.X += 1;

        if (dir != Vector2.Zero)
            _player.Position += Vector2.Normalize(dir) * _player.MoveSpeed * dt;

        _player.ClampToRoom(RoomX + WallThickness, RoomY + WallThickness,
                            RoomWidth - WallThickness * 2, RoomHeight - WallThickness * 2);
    }

    /*Start of AI generated code*/
    private void TryShoot()
    {
        if (_mouseButtons[(byte)MouseButton.Primary] == 0) return;

        double fireInterval = 1.0 / _player.FireRate;
        if (_totalTime - _player.LastShotTime < fireInterval) return;

        _player.LastShotTime = _totalTime;

        Vector2 worldMouse = _camera.ScreenToWorld(new Vector2(_mouseX, _mouseY));
        var dir = worldMouse - _player.Position;
        if (dir == Vector2.Zero) return;

        _bullets.Add(new Bullet
        {
            Position         = _player.Position,
            Direction        = Vector2.Normalize(dir),
            Speed            = _player.BulletSpeed,
            Damage           = _player.BulletDamage,
            RemainingPierces = _player.PiercingShots,
            Active           = true,
        });
    }

    private void UpdateBullets(float dt)
    {
        foreach (var b in _bullets)
        {
            if (!b.Active) continue;
            b.Update(dt);
            if (b.IsOutsideRoom(RoomX, RoomY, RoomWidth, RoomHeight))
                b.Active = false;
        }

        // Trim dead bullets periodically to keep memory bounded.
        if (_bullets.Count(b => !b.Active) > 100)
            _bullets.RemoveAll(b => !b.Active);
    }
    /*End of AI generated code*/

    private void UpdateEnemies(float dt)
    {
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            e.Update(_player.Position, dt);
        }
    }

    /*Start of AI generated code*/
    private void CheckBulletEnemyCollisions()
    {
        // LINQ: only test against active enemies.
        var active = _enemies.Where(e => e.Active).ToList();

        foreach (var b in _bullets)
        {
            if (!b.Active) continue;
            foreach (var e in active)
            {
                if (!e.Active) continue;
                if (!Aabb(b.Position, Bullet.Width, Bullet.Height,
                          e.Position, Enemy.Width,  Enemy.Height)) continue;

                e.TakeDamage(b.Damage);
                if (!e.IsAlive) _score += 10;

                if (b.RemainingPierces > 0)
                    b.RemainingPierces--;
                else
                    b.Active = false;

                if (!b.Active) break;
            }
        }
    }

    private void CheckPlayerEnemyCollisions()
    {
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            if (!Aabb(e.Position, Enemy.Width, Enemy.Height,
                      _player.Position, Player.Width, Player.Height)) continue;

            // Push enemy away so it doesn't spam damage every frame.
            var push = e.Position - _player.Position;
            if (push == Vector2.Zero) push = Vector2.UnitX;
            e.Position += Vector2.Normalize(push) * (Enemy.Width * 0.55f);

            _player.TakeDamage(e.ContactDamage);
        }
    }
    /*End of AI generated code*/

// ── Private: rendering ────────────────────────────────────────────────────

    /*Start of AI generated code*/
    private unsafe void RenderWorld(Renderer* r)
    {
        // Arena background texture stretched to fill the full room
        var arenaRect = SR(RoomX, RoomY, RoomWidth, RoomHeight);
        _sdl.RenderCopy(r, _texArena, null, ref arenaRect);

        // Bullets (yellow rectangles — small, texture not needed)
        _sdl.SetRenderDrawColor(r, 255, 230, 0, 255);
        foreach (var b in _bullets)
        {
            if (!b.Active) continue;
            var rect = SR((int)(b.Position.X - Bullet.Width  / 2f),
                          (int)(b.Position.Y - Bullet.Height / 2f),
                          Bullet.Width, Bullet.Height);
            _sdl.RenderFillRect(r, ref rect);
        }

        // Enemies — texture
        foreach (var e in _enemies)
        {
            if (!e.Active) continue;
            var rect = SR((int)(e.Position.X - Enemy.Width  / 2f),
                          (int)(e.Position.Y - Enemy.Height / 2f),
                          Enemy.Width, Enemy.Height);
            _sdl.RenderCopy(r, _texEnemy, null, ref rect);
            DrawHealthBar(r, e, rect.Origin.X, rect.Origin.Y - 6, Enemy.Width);
        }

        // Player — texture
        var prect = SR((int)(_player.Position.X - Player.Width  / 2f),
                       (int)(_player.Position.Y - Player.Height / 2f),
                       Player.Width, Player.Height);
        _sdl.RenderCopy(r, _texPlayer, null, ref prect);
    }

private unsafe void DrawHealthBar(Renderer* r, Enemy e, int x, int y, int width)
    {
        var bg = MakeRect(x, y, width, 4);
        _sdl.SetRenderDrawColor(r, 60, 0, 0, 255);
        _sdl.RenderFillRect(r, ref bg);

        int fillW = (int)(width * ((float)e.Health / e.MaxHealth));
        if (fillW <= 0) return;
        var fill = MakeRect(x, y, fillW, 4);
        _sdl.SetRenderDrawColor(r, 220, 30, 30, 255);
        _sdl.RenderFillRect(r, ref fill);
    }

    private unsafe void RenderHud(Renderer* r)
    {
        const int scale  = 2;
        const int lineH  = 8 * scale + 4;
        const int pad    = 8;
        const int panelW = 80;
        const int panelH = lineH * 2 + pad * 2;
        int panelX = ScreenWidth  - panelW - 60;
        int panelY = ScreenHeight - panelH - 8;

        // Background panel
        _sdl.SetRenderDrawBlendMode(r, BlendMode.Blend);
        _sdl.SetRenderDrawColor(r, 0, 0, 0, 160);
        var panel = MakeRect(panelX, panelY, panelW, panelH);
        _sdl.RenderFillRect(r, ref panel);
        _sdl.SetRenderDrawBlendMode(r, BlendMode.None);

        // HP label + value
        string hpLabel  = "HP";
        string hpVal    = $"{_player.Health}/{_player.MaxHealth}";
        int    labelY   = panelY + pad;
        DrawString(r, hpLabel, panelX + pad, labelY, 200, 80, 80, scale);
        DrawString(r, hpVal,   panelX + pad + StringWidth(hpLabel, scale) + scale * 2, labelY, 255, 200, 200, scale);

        // WAVE label + value
        string waveLabel = "WAVE";
        string waveVal   = $"{_currentRound}";
        int    waveY     = labelY + lineH;
        DrawString(r, waveLabel, panelX + pad, waveY, 80, 160, 200, scale);
        DrawString(r, waveVal,   panelX + pad + StringWidth(waveLabel, scale) + scale * 2, waveY, 180, 230, 255, scale);
    }

    private unsafe void RenderUpgradeOverlay(Renderer* r)
    {
        _sdl.SetRenderDrawBlendMode(r, BlendMode.Blend);
        _sdl.SetRenderDrawColor(r, 0, 0, 0, 180);
        var overlay = MakeRect(0, 0, ScreenWidth, ScreenHeight);
        _sdl.RenderFillRect(r, ref overlay);
        _sdl.SetRenderDrawBlendMode(r, BlendMode.None);

        int totalW = _offeredUpgrades.Count * UpgradeCardW +
                     (_offeredUpgrades.Count - 1) * UpgradeCardGap;
        int startX = (ScreenWidth  - totalW) / 2;
        int startY = (ScreenHeight - UpgradeCardH) / 2;

        // Header
        string header = "PICK AN UPGRADE";
        DrawString(r, header, (ScreenWidth - StringWidth(header, 2)) / 2, startY - 22, 255, 220, 80, 2);

        for (int i = 0; i < _offeredUpgrades.Count; i++)
        {
            int cx      = startX + i * (UpgradeCardW + UpgradeCardGap);
            bool hovered = _mouseX >= cx && _mouseX <= cx + UpgradeCardW &&
                           _mouseY >= startY && _mouseY <= startY + UpgradeCardH;

            // Card background
            byte shade = hovered ? (byte)75 : (byte)45;
            var card = MakeRect(cx, startY, UpgradeCardW, UpgradeCardH);
            _sdl.SetRenderDrawColor(r, shade, shade, (byte)(shade + 20), 255);
            _sdl.RenderFillRect(r, ref card);

            // Border
            byte bord = hovered ? (byte)220 : (byte)120;
            _sdl.SetRenderDrawColor(r, bord, bord, (byte)(bord + 35), 255);
            _sdl.RenderDrawRect(r, ref card);

            // Name — scale 1, centred, 6px from top
            string name = _offeredUpgrades[i].Name;
            int nameX = cx + (UpgradeCardW - StringWidth(name, 1)) / 2;
            DrawString(r, name, nameX, startY + 6, 255, 255, 255, 1);

            // Divider
            int divY = startY + 6 + 8 * 1 + 4;
            _sdl.SetRenderDrawColor(r, 80, 100, 150, 255);
            var div = MakeRect(cx + 6, divY, UpgradeCardW - 12, 1);
            _sdl.RenderFillRect(r, ref div);

            // Description — scale 1, word-wrapped at card width minus padding
            int maxCharsPerLine = (UpgradeCardW - 8) / 6; // 6px per char at scale 1
            var lines = WordWrap(_offeredUpgrades[i].Description, maxCharsPerLine);
            int descY = divY + 4;
            foreach (var line in lines)
            {
                DrawString(r, line, cx + 4, descY, 190, 190, 190, 1);
                descY += 10;
            }
        }
    }

    private static List<string> WordWrap(string text, int maxChars)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        string current = "";
        foreach (var word in words)
        {
            if (current.Length == 0)
                current = word;
            else if (current.Length + 1 + word.Length <= maxChars)
                current += " " + word;
            else { lines.Add(current); current = word; }
        }
        if (current.Length > 0) lines.Add(current);
        return lines;
    }

    private unsafe void RenderGameOver(Renderer* r)
    {
        _sdl.SetRenderDrawBlendMode(r, BlendMode.Blend);
        _sdl.SetRenderDrawColor(r, 0, 0, 0, 200);
        var overlay = MakeRect(0, 0, ScreenWidth, ScreenHeight);
        _sdl.RenderFillRect(r, ref overlay);
        _sdl.SetRenderDrawBlendMode(r, BlendMode.None);

        DrawString(r, "GAME OVER",
                   ScreenWidth / 2 - 27, ScreenHeight / 2 - 30, 255, 80, 80);
        DrawString(r, $"ROUND {_currentRound}   SCORE {_score}   BEST {_highScore}",
                   ScreenWidth / 2 - 90, ScreenHeight / 2, 200, 200, 200);
        DrawString(r, "PRESS R TO RESTART",
                   ScreenWidth / 2 - 55, ScreenHeight / 2 + 30, 150, 210, 255);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// World-space rect → screen-space Rectangle<int> via the camera.
    private Rectangle<int> SR(int wx, int wy, int w, int h)
    {
        var s = _camera.WorldToScreen(new Vector2(wx, wy));
        return new Rectangle<int>((int)s.X, (int)s.Y, w, h);
    }

    /// Screen-space rectangle (no camera transform).
    private static Rectangle<int> MakeRect(int x, int y, int w, int h) =>
        new(x, y, w, h);

    /// AABB overlap (positions are centre-points).
    private static bool Aabb(Vector2 aPos, int aW, int aH,
                              Vector2 bPos, int bW, int bH) =>
        MathF.Abs(aPos.X - bPos.X) < (aW + bW) / 2f &&
        MathF.Abs(aPos.Y - bPos.Y) < (aH + bH) / 2f;

    // ── Tiny pixel-font renderer ──────────────────────────────────────────────
    // Each glyph is 5×8 pixels encoded as 8 row-bytes (bit 7 = leftmost pixel).

    private static readonly Dictionary<char, byte[]> _glyphs = BuildGlyphs();

    private static Dictionary<char, byte[]> BuildGlyphs()
    {
        var g = new Dictionary<char, byte[]>
        {
            ['A'] = [0x70, 0x88, 0x88, 0xF8, 0x88, 0x88, 0x88, 0x00],
            ['B'] = [0xF0, 0x88, 0x88, 0xF0, 0x88, 0x88, 0xF0, 0x00],
            ['C'] = [0x78, 0x80, 0x80, 0x80, 0x80, 0x80, 0x78, 0x00],
            ['D'] = [0xF0, 0x88, 0x88, 0x88, 0x88, 0x88, 0xF0, 0x00],
            ['E'] = [0xF8, 0x80, 0x80, 0xF0, 0x80, 0x80, 0xF8, 0x00],
            ['F'] = [0xF8, 0x80, 0x80, 0xF0, 0x80, 0x80, 0x80, 0x00],
            ['G'] = [0x78, 0x80, 0x80, 0x98, 0x88, 0x88, 0x78, 0x00],
            ['H'] = [0x88, 0x88, 0x88, 0xF8, 0x88, 0x88, 0x88, 0x00],
            ['I'] = [0xF8, 0x20, 0x20, 0x20, 0x20, 0x20, 0xF8, 0x00],
            ['J'] = [0x38, 0x10, 0x10, 0x10, 0x10, 0x90, 0x60, 0x00],
            ['K'] = [0x88, 0x90, 0xA0, 0xC0, 0xA0, 0x90, 0x88, 0x00],
            ['L'] = [0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0xF8, 0x00],
            ['M'] = [0x88, 0xD8, 0xA8, 0x88, 0x88, 0x88, 0x88, 0x00],
            ['N'] = [0x88, 0xC8, 0xA8, 0x98, 0x88, 0x88, 0x88, 0x00],
            ['O'] = [0x70, 0x88, 0x88, 0x88, 0x88, 0x88, 0x70, 0x00],
            ['P'] = [0xF0, 0x88, 0x88, 0xF0, 0x80, 0x80, 0x80, 0x00],
            ['Q'] = [0x70, 0x88, 0x88, 0x88, 0xA8, 0x90, 0x68, 0x00],
            ['R'] = [0xF0, 0x88, 0x88, 0xF0, 0xA0, 0x90, 0x88, 0x00],
            ['S'] = [0x78, 0x80, 0x80, 0x70, 0x08, 0x08, 0xF0, 0x00],
            ['T'] = [0xF8, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x00],
            ['U'] = [0x88, 0x88, 0x88, 0x88, 0x88, 0x88, 0x70, 0x00],
            ['V'] = [0x88, 0x88, 0x88, 0x88, 0x88, 0x50, 0x20, 0x00],
            ['W'] = [0x88, 0x88, 0x88, 0xA8, 0xA8, 0xD8, 0x88, 0x00],
            ['X'] = [0x88, 0x88, 0x50, 0x20, 0x50, 0x88, 0x88, 0x00],
            ['Y'] = [0x88, 0x88, 0x50, 0x20, 0x20, 0x20, 0x20, 0x00],
            ['Z'] = [0xF8, 0x08, 0x10, 0x20, 0x40, 0x80, 0xF8, 0x00],
            ['0'] = [0x70, 0x88, 0x98, 0xA8, 0xC8, 0x88, 0x70, 0x00],
            ['1'] = [0x20, 0x60, 0x20, 0x20, 0x20, 0x20, 0x70, 0x00],
            ['2'] = [0x70, 0x88, 0x08, 0x30, 0x40, 0x80, 0xF8, 0x00],
            ['3'] = [0xF0, 0x08, 0x08, 0x70, 0x08, 0x08, 0xF0, 0x00],
            ['4'] = [0x88, 0x88, 0x88, 0xF8, 0x08, 0x08, 0x08, 0x00],
            ['5'] = [0xF8, 0x80, 0x80, 0xF0, 0x08, 0x08, 0xF0, 0x00],
            ['6'] = [0x38, 0x40, 0x80, 0xF0, 0x88, 0x88, 0x70, 0x00],
            ['7'] = [0xF8, 0x08, 0x10, 0x20, 0x20, 0x20, 0x20, 0x00],
            ['8'] = [0x70, 0x88, 0x88, 0x70, 0x88, 0x88, 0x70, 0x00],
            ['9'] = [0x70, 0x88, 0x88, 0x78, 0x08, 0x10, 0xE0, 0x00],
            [' '] = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00],
            ['-'] = [0x00, 0x00, 0x00, 0xF8, 0x00, 0x00, 0x00, 0x00],
            ['+'] = [0x00, 0x20, 0x20, 0xF8, 0x20, 0x20, 0x00, 0x00],
            ['%'] = [0xC0, 0xC8, 0x10, 0x20, 0x40, 0x98, 0x18, 0x00],
            ['['] = [0x70, 0x40, 0x40, 0x40, 0x40, 0x40, 0x70, 0x00],
            [']'] = [0x70, 0x10, 0x10, 0x10, 0x10, 0x10, 0x70, 0x00],
            ['.'] = [0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x60, 0x00],
            [','] = [0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x40, 0x00],
            ['!'] = [0x20, 0x20, 0x20, 0x20, 0x20, 0x00, 0x20, 0x00],
            ['/'] = [0x08, 0x10, 0x10, 0x20, 0x40, 0x40, 0x80, 0x00],
        };

        // Map lower-case to upper-case glyphs.
        for (char c = 'a'; c <= 'z'; c++)
            if (g.TryGetValue((char)(c - 32), out var upper))
                g[c] = upper;

        return g;
    }

    private unsafe void DrawString(Renderer* r, string text, int x, int y,
                                   byte red, byte green, byte blue, int scale = 1)
    {
        int glyphW = (5 + 1) * scale;
        _sdl.SetRenderDrawColor(r, red, green, blue, 255);
        int cx = x;
        foreach (char ch in text)
        {
            if (_glyphs.TryGetValue(ch, out var rows))
            {
                for (int row = 0; row < 8; row++)
                {
                    byte mask = rows[row];
                    for (int col = 0; col < 5; col++)
                    {
                        if ((mask & (0x80 >> col)) != 0)
                        {
                            var px = MakeRect(cx + col * scale, y + row * scale, scale, scale);
                            _sdl.RenderFillRect(r, ref px);
                        }
                    }
                }
            }
            cx += glyphW;
        }
    }

    // Returns pixel width of a string at given scale.
    private static int StringWidth(string text, int scale = 1) => text.Length * 6 * scale;
    /*End of AI generated code*/
}
