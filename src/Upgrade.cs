using TheAdventure.Objects;

namespace TheAdventure;

public sealed class Upgrade
{
    public string Name { get; }
    public string Description { get; }
    private readonly Action<Player> _apply;

    public Upgrade(string name, string description, Action<Player> apply)
    {
        Name = name;
        Description = description;
        _apply = apply;
    }

    public void Apply(Player player) => _apply(player);
}
