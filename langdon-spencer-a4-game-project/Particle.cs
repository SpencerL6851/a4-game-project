using System;
using System.Numerics;

namespace MohawkGame2D;

public struct Particle(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, Color Color, Vector2 Size)
{
    public Vector2 Position = Position;
    public Vector2 Velocity = Velocity;
    public Vector2 Acceleration = Acceleration;
    public Color Color = Color;
    public Vector2 Size = Size;
    public void Update()
    {

    }
}
