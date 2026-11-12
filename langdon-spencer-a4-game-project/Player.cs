using System;
using System.Numerics;

namespace MohawkGame2D;

public class Player
{
    // PROPERTIES
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public Vector2 Direction;
    public int IFrames;
    public int CurrentHealth = 300;
    public bool Grounded;
    public float HealCooldown;
    public int HealsRemaining;
    public float DamageMultiplier;

    // CONSTANTS
    char Difficulty;
    public const int MaxHealth = 300;
    public float RangedCooldown;
    public float MeleeCooldown;
    public float MovementSpeed = 2f;
    public float Friction = 0.2f;
    public float Gravity = 2f;
    public float MaxThrust = 12f;
    public float Radius = 20f;
    // METHODS
    public void Setup(char Difficulty)
    {
        this.Difficulty = Difficulty;
        HealsRemaining = Difficulty switch
        {
            'E' => -1,
            'N' => 5,
            'H' => 1,
            _ => 0
        };
        DamageMultiplier = Difficulty switch
        {
            'E' => 0.8f,
            'N' => 1,
            'H' => 1.2f,
            _ => 0
        };
        Position = new Vector2(Window.Width * 0.5f, Window.Height - 2 * Radius);
    }
    public void Damage(int Damage, int IFrames)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - (int)Math.Round(Damage * Random.Float(0.85f, 1.15f) * DamageMultiplier));
        this.IFrames = IFrames;
        if (CurrentHealth == 0)
        {
            throw new Exception("You Died!");
        }
    }
    public void Update()
    {
        // Input Handling
        Acceleration = new(0, Gravity);
        if (Input.IsKeyboardKeyDown(KeyboardKey.S)) 
        { 
            Acceleration.Y = Gravity * 1.5f; 
        }
        if (Input.IsKeyboardKeyDown(KeyboardKey.A)) 
        { 
            Acceleration.X -= MovementSpeed; 
        }
        if (Input.IsKeyboardKeyDown(KeyboardKey.D)) 
        { 
            Acceleration.X += MovementSpeed; 
        }
        if (Input.IsKeyboardKeyDown(KeyboardKey.Space)) 
        { 
            Acceleration.Y = -0.8f * Gravity;
            Grounded = false; 
        }
        if (Input.IsKeyboardKeyPressed(KeyboardKey.H) & HealCooldown == 0 & CurrentHealth < MaxHealth & HealsRemaining > 0)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + 100);
            HealsRemaining -= 1;
            if (Difficulty == 'E') { HealCooldown = 45; }
            else { HealCooldown = 60; }
        }
        // Cooldowns
        RangedCooldown = MathF.Max(0, RangedCooldown - Time.DeltaTime);
        MeleeCooldown = MathF.Max(0, MeleeCooldown - Time.DeltaTime);
        HealCooldown = MathF.Max(0, HealCooldown - Time.DeltaTime);


        // Physics
        Acceleration.X -= Velocity.X * Friction;
        Velocity += Acceleration;
        Velocity.Y = Math.Clamp(Velocity.Y, -MaxThrust, (8 * Gravity / 1.225f));
        Position += Velocity;
        Direction = Vector2.Normalize(Input.GetMousePosition() - Position);

        // Collision Detection
        if (Position.Y >= Window.Height - Radius) // Bottom
        {
            Grounded = true;
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
        
        // Rendering Jetpack
        Draw.LineSize = 1;
        Draw.LineColor = new(16);
        Draw.FillColor = new(64);
        if (Direction.X > 0)
        {
            Draw.Rectangle(Position - new Vector2(MathF.CopySign(Radius * 1.15f, Direction.X), Radius), new(Radius * 0.5f, Radius * 1.75f));
        }
        else
        {
            Draw.Rectangle(Position - new Vector2(MathF.CopySign(Radius * 0.65f, Direction.X), Radius), new(Radius * 0.5f, Radius * 1.75f));
        }
        // Rendering Body
        Draw.FillColor = new Color("44aaff");
        Draw.LineColor = new Color("3078b3");
        Draw.LineSize = 2;
        if (IFrames > 0)
        {
            IFrames -= 1;
            if (Time.SecondsElapsed / 2 % 2 == 0)
            {
                Draw.FillColor = new Color("80c6ff");
                Draw.LineColor = new Color("598ab3");
            }
        }
        // Rendering Eyes
        Draw.Circle(Position, Radius);
        Draw.FillColor = new(0);
        Draw.Circle(Position + new Vector2(MathF.CopySign(Radius * 0.4f, Direction.X), Radius * -0.1f), Radius * -1 / 6f);
        Draw.Circle(Position + new Vector2(MathF.CopySign(Radius * 0.8f, Direction.X), Radius * -0.1f), Radius * -1 / 6f);
    }
}
