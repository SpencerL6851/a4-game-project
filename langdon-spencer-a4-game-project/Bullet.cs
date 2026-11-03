using System;
using System.Numerics;

namespace MohawkGame2D;

public class Bullet(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, string AttackName)
{
    public Vector2 Position = Position;
    public Vector2 Velocity = Velocity;
    public Vector2 Acceleration = Acceleration;
    public string AttackName = AttackName;
    public void Update()
    {

    }
}
