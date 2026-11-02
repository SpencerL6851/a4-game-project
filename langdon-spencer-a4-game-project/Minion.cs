using System;
using System.Numerics;

namespace MohawkGame2D;

public class Minion
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public int MaxHealth = 100;
    public int CurrentHealth = 100;
}
