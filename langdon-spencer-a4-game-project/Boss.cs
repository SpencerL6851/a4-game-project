using System;
using System.Numerics;

namespace MohawkGame2D;

public class Boss
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public bool UsePhysics = true;
    public int MaxHealth = 25000;
    public int CurrentHealth = 25000;
}
