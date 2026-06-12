using TheAdventure;

namespace TheAdventure.Objects;

/// The full upgrade pool. Call Build() to get a fresh shuffleable list.
public static class UpgradePool
{
    public static List<Upgrade> Build() =>
    [
        new Upgrade("+20 Max HP",        "Increase max health by 20.",        p => { p.MaxHealth += 20; p.Heal(20); }),
        new Upgrade("+15% Move Speed",   "Move 15% faster.",                  p => p.MoveSpeed  *= 1.15f),
        new Upgrade("+20% Fire Rate",    "Shoot 20% faster.",                 p => p.FireRate   *= 1.20f),
        new Upgrade("+5 Bullet Damage",  "Each bullet deals 5 more damage.",  p => p.BulletDamage += 5),
        new Upgrade("Piercing Shot",     "Bullets pierce one extra enemy.",   p => p.PiercingShots += 1),
        new Upgrade("+15% Bullet Speed", "Bullets travel 15% faster.",        p => p.BulletSpeed *= 1.15f),
        new Upgrade("+30 Max HP",        "Increase max health by 30.",        p => { p.MaxHealth += 30; p.Heal(30); }),
        new Upgrade("+10% Move Speed",   "Move 10% faster.",                  p => p.MoveSpeed  *= 1.10f),
        new Upgrade("+25% Fire Rate",    "Shoot 25% faster.",                 p => p.FireRate   *= 1.25f),
        new Upgrade("+10 Bullet Damage", "Each bullet deals 10 more damage.", p => p.BulletDamage += 10),
    ];
}
