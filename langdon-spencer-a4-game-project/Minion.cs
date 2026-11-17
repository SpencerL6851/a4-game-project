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
    public int IFrames;
    public int MaxHealth;
    public ColorF[] ColorPallete = new ColorF[2];
    public int ContactDamage;
    public int AttackDamage;
    public float Radius;
    public string EnemyType;
    public float Friction = 0.2f;
    public float Gravity = 2f;
    public float AttackRange;
    public int EnemyID; 

    // CONSTRUCTOR
    public Minion(Vector2 pos, Vector2 vel, Vector2 acc, int id, string type)
    {
        Position = pos;
        Velocity = vel;
        Acceleration = acc;
        EnemyID = id;
        EnemyType = type;
        switch (type)
        {
            case "Basic":
                {
                    CurrentHealth = MaxHealth = 50;
                    ContactDamage = 15;
                    AttackDamage = 15;
                    Radius = 20;
                    AttackRange = 150;
                    ColorPallete = [new(153f / 255, 51f / 255, 51f / 255), new(73f / 255, 25f / 255, 25f / 255)];
                    break;
                }
            case "Flying":
                {
                    CurrentHealth = MaxHealth = 30;
                    ContactDamage = 5;
                    AttackDamage = 20;
                    Radius = 18;
                    AttackRange = 400;
                    ColorPallete = [new(153f / 255, 85f / 255, 51f / 255), new(77f / 255, 42f / 255, 25f / 255)];
                    break;
                }
            case "Orbiting":
                {
                    AttackCooldown = 1.25f;
                    CurrentHealth = MaxHealth = 135;
                    ContactDamage = 0;
                    AttackDamage = 35;
                    Radius = 15;
                    AttackRange = -1;
                    ColorPallete = [new(82f / 255, 18f / 255, 15f / 255), new(41f / 255, 9f / 255, 7f / 255)];
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
    // METHODS

    /// <summary>
    ///     Damages this minion and gives them invincibility frames.
    /// </summary>
    /// <param name="Damage">   The base damage of the attack. The actual damage dealt will be within 15% of the base damage.</param>
    /// <param name="IFrames">  The amount of invincibility frames the attack will provide.</param>
    public void Damage(int Damage, int IFrames)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - (int)MathF.Round(Damage * Random.Float(0.85f, 1.15f)));
        this.IFrames = IFrames;
    }


    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public void Update()
    {
        AttackCooldown = Math.Max(0, AttackCooldown - Time.DeltaTime);
        IFrames = Math.Max(0, IFrames - 1);
        
        // Physics
        if (EnemyType == "Orbiting")
        {

        }
        else
        {
            Acceleration -= Velocity * Friction;
            Velocity += Acceleration;
            Position += Velocity;
            if (EnemyType == "Flying")
            {
                Acceleration = Vector2.Zero;
            }
            else
            {
                Acceleration = new Vector2(Acceleration.X, Gravity);
            }
            if (Velocity.Length() < 0.001f)
            {
                Velocity = Vector2.Zero;
            }
        }
        

        // Collision Detection
        if (Position.Y >= Window.Height - Radius) // Bottom
        {
            Position.Y = Window.Height - Radius;
            if (Velocity.Y > 0) { Velocity.Y = 0; }
        }
        if (Position.Y <= Radius) // Top
        {
            Position.Y = Radius;
            if (Velocity.Y < 0) { Velocity.Y = 0; }
        }
        if (Position.X >= Window.Width - 150 - Radius) // Right
        {
            Position.X = Window.Width - 150 - Radius;
            if (Velocity.X < 0) { Velocity.X = 0; }
        }
        if (Position.X <= 150 + Radius) // Left
        {
            Position.X = 150 + Radius;
            if (Velocity.X > 0) { Velocity.X = 0; }
        }

        // Rendering
        Draw.FillColor = ColorPallete[0];
        Draw.LineColor = ColorPallete[1];
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
