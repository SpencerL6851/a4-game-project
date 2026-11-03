using MohawkGame2D;
using System;
using System.Numerics;

namespace MohawkGame2D;



public class Boss
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public Vector2 TargetPosition;
    public int CurrentPhase = 1;
    public string? CurrentAttack;
    public float IdleTimer;
    public bool UsePhysics = true;
    public int MaxHealth = 25000;
    public int CurrentHealth = 25000;
    public void Spawn()
    {
        Position = new Vector2(Window.Width * 0.5f, -Window.Height * 0.5f);
    }
    public void Update()
    {

    }
}