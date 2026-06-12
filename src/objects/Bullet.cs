using System.Numerics;

namespace TheAdventure.Objects;

public sealed class Bullet
{
    public const int Width = 8;
    public const int Height = 8;

    public Vector2 Position { get; set; }
    public Vector2 Direction { get; init; }
    public float Speed { get; set; }
    public int Damage { get; init; }
    /// Remaining number of additional enemies this bullet can pass through (0 = no piercing).
    public int RemainingPierces { get; set; }
    public bool Active { get; set; } = true;

    /*Start of AI generated code*/
    public void Update(float dt)
    {
        Position += Direction * Speed * dt;
    }

    /// Returns true if the bullet centre is outside the given room rectangle.
    public bool IsOutsideRoom(int roomX, int roomY, int roomW, int roomH) =>
        Position.X < roomX || Position.X > roomX + roomW ||
        Position.Y < roomY || Position.Y > roomY + roomH;
    /*End of AI generated code*/
}
