using System.Numerics;
using TheAdventure;

namespace TheAdventure.Objects;

public sealed class Enemy : IDamageable
{
    public const int Width = 48;
    public const int Height = 48;

    public Vector2 Position { get; set; }
    public int Health { get; private set; }
    public int MaxHealth { get; }
    public bool IsAlive => Health > 0;
    public bool Active { get; set; } = true;

    public float Speed { get; init; }
    public int ContactDamage { get; init; }

    public Enemy(Vector2 position, int health, float speed, int contactDamage)
    {
        Position = position;
        MaxHealth = health;
        Health = health;
        Speed = speed;
        ContactDamage = contactDamage;
    }

    public void TakeDamage(int amount)
    {
        Health = Math.Max(0, Health - amount);
        if (Health == 0)
            Active = false;
    }

    public void Update(Vector2 playerPosition, float dt)
    {
        var delta = playerPosition - Position;
        if (delta == Vector2.Zero) return;
        Position += Vector2.Normalize(delta) * Speed * dt;
    }
}
