using System;
using System.Numerics;

namespace MohawkGame2D;

public class Player
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public float MeleeCooldown = 1f;
    public float RangedCooldown = 0.25f;
    public int MaxHealth = 300;
    public int CurrentHealth = 300;
    public void Setup()
    {

    }
    public void Update()
    {

    }
}
