using System.Numerics;
using TheAdventure;

namespace TheAdventure.Objects;

public sealed class Player : IDamageable
{
    // Dimensions used for rendering and collision
    public const int Width = 48;
    public const int Height = 48;

    public Vector2 Position { get; set; }
    public int Health { get; private set; }
    public int MaxHealth { get; set; }
    public bool IsAlive => Health > 0;

    public float MoveSpeed { get; set; }
    /// Shots per second.
    public float FireRate { get; set; }
    public int BulletDamage { get; set; }
    public int PiercingShots { get; set; }
    public float BulletSpeed { get; set; }

    // Timestamp (seconds) of the last shot fired.
    public double LastShotTime { get; set; }

    public Player(Vector2 startPosition)
    {
        Position = startPosition;
        MaxHealth = 100;
        Health = MaxHealth;
        MoveSpeed = 180f;
        FireRate = 5f;
        BulletDamage = 10;
        PiercingShots = 0;
        BulletSpeed = 480f;
        LastShotTime = double.MinValue;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Clamp(Health - amount, 0, MaxHealth);
    }

    public void Heal(int amount)
    {
        Health = Math.Min(Health + amount, MaxHealth);
    }

    /// Clamp position inside the room rectangle.
    public void ClampToRoom(int roomX, int roomY, int roomW, int roomH)
    {
        float x = Math.Clamp(Position.X, roomX + Width / 2f, roomX + roomW - Width / 2f);
        float y = Math.Clamp(Position.Y, roomY + Height / 2f, roomY + roomH - Height / 2f);
        Position = new Vector2(x, y);
    }
}
