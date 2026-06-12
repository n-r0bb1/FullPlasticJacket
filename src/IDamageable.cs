namespace TheAdventure;

/// Implemented by any entity that can receive damage.
public interface IDamageable
{
    int Health { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }

    void TakeDamage(int amount);
}
