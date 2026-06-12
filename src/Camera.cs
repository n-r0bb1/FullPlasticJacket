using System.Numerics;

namespace TheAdventure;

/// Keeps the player centred on screen and converts world ↔ screen coordinates.
public sealed class Camera
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    // Top-left corner of the visible viewport in world space.
    public Vector2 Position { get; private set; }

    public Camera(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    /// Move the camera so that `target` stays centred.
    public void Follow(Vector2 target)
    {
        Position = new Vector2(
            target.X - _screenWidth / 2f,
            target.Y - _screenHeight / 2f);
    }

    public Vector2 WorldToScreen(Vector2 world) => world - Position;

    public Vector2 ScreenToWorld(Vector2 screen) => screen + Position;
}
