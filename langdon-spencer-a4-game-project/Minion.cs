using System;
using System.Numerics;

namespace MohawkGame2D;

public class Minion(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, int Health)
{
    public Vector2 Position = Position;
    public Vector2 Velocity = Velocity;
    public Vector2 Acceleration = Acceleration;
    public int MaxHealth = Health;
    public int CurrentHealth = Health;
    public void Setup()
    {

    }
    public void Update()
    {

    }
}
