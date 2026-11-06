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
    
    // CONSTANTS
    public int MaxHealth = 300;
    public float MeleeCooldown = 1f;
    public float RangedCooldown = 0.25f;
    public float MovementSpeed = 2f;
    public float Friction = 0.2f;
    public float Gravity = 2f;
    public float MaxThrust = 10f;
    public float Radius = 20f;
    
    // METHODS
    public void Setup()
    {
        Position = new Vector2(Window.Width * 0.5f, Window.Height - 2 * Radius);
    }
    public void Damage(int damage, int iFrames)
    {
        CurrentHealth -= damage;
        IFrames = iFrames;
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
        if (Position.X >= Window.Width - Radius) // Right
        {
            Position.X = Window.Width - Radius;
            if (Velocity.X < 0) { Velocity.X = 0; }
        }
        if (Position.X <= Radius) // Left
        {
            Position.X = Radius;
            if (Velocity.X > 0) { Velocity.X = 0; }
        }

        // Rendering
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
        
        Draw.Circle(Position, Radius);
        Draw.FillColor = new(0);
        Draw.Circle(Position + new Vector2(MathF.CopySign(Radius * 0.4f, Direction.X), Radius * -0.1f), Radius * -1 / 6f);
        Draw.Circle(Position + new Vector2(MathF.CopySign(Radius * 0.8f, Direction.X), Radius * -0.1f), Radius * -1 / 6f);
    }
}
