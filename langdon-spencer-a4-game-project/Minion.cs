using System;
using System.Numerics;

namespace MohawkGame2D;

public class Minion
{
    // PROPERTIES
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public float AttackCooldown;
    public int CurrentHealth;
    
    // CONSTANTS
    public int MaxHealth;
    public float Radius;
    public string EnemyType;
    public float Friction = 0.2f;
    public float Gravity = 2f;
    public float AttackRange;
    public Minion(Vector2 pos, Vector2 vel, Vector2 acc, int hp, float size, string type)
    {
        Position = pos;
        Velocity = vel;
        Acceleration = acc;
        CurrentHealth = MaxHealth = hp;
        Radius = size;
        EnemyType = type;
    }

    // METHODS
    public void Update()
    {
        // Physics
        Acceleration -= Velocity * Friction;
        Velocity += Acceleration;
        Position += Velocity;

        // Rendering
        Draw.FillColor = new("993333");
        Draw.LineColor = new("4d1919");
        Draw.LineSize = 2;
        Draw.Circle(Position, Radius);

        // Health Bar
        if (CurrentHealth < MaxHealth)
        {
            float healthPercent = (float)CurrentHealth / MaxHealth;
            Draw.FillColor = new(0);
            Draw.LineColor = new(64);
            Draw.LineSize = 2;
            Draw.Rectangle(Position - new Vector2(Radius * 1.25f, Radius * 1.2f), new(Radius * 2.5f, Radius * 0.2f));
            Draw.FillColor = new ColorF(1 - healthPercent, healthPercent, 0);
            Draw.LineSize = 0;
            Draw.Rectangle(Position - new Vector2(Radius * 1.25f, Radius * 1.2f), new(Radius * 2.5f * healthPercent, Radius * 0.2f));
        }
    }
}
