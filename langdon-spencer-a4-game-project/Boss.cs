using System;
using System.Numerics;

namespace MohawkGame2D;

public class Boss
{
    // PROPERTIES
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 TargetPosition;
    public Vector2 ZoomStartPosition;
    public bool Invincible = true;
    public int CurrentPhase;
    public string CurrentAction = "Spawn";
    public string NextAction = "Shoot Bullets";
    public int ActionCounter;
    public float ActionDuration;
    public float ActionTimer;
    public float AnimationPercent;
    public int CurrentHealth = 7500;
    public int IFrames = 0;

    // CONSTANTS
    public int MaxHealth = 7500;
    public int ContactDamage = 50;
    public float Radius = 45;

    // METHODS

    public float InOutSineEasing(float percent)
    {
        return (-(MathF.Cos(MathF.PI * percent) - 1)) / 2f;
    }

    /// <summary>
    /// Rotates a vector by a given angle.
    /// </summary>
    /// <param name="degrees"> If true, specified angle is converted to radians before calculation</param>
    /// <returns>The rotated vector.</returns>
    public Vector2 RotateVector(Vector2 vector, float angle, bool degrees)
    {
        float newAngle = angle;
        if (degrees)
        {
            newAngle = (angle * MathF.PI / 180);
        }
        return new Vector2(vector.X * MathF.Cos(newAngle) - vector.Y * MathF.Sin(newAngle), vector.X * MathF.Sin(newAngle) + vector.Y * MathF.Cos(newAngle));
    }
    public Vector2[] GetBulletFanVectors(Vector2 direction, float spreadDegrees, int amount)
    {
        Vector2[] vectorArray = new Vector2[amount];
        for (int i = 0; i < amount; i++)
        {
            float percent = (float)i / (amount - 1);
            float angle = MathF.Atan2(direction.Y, direction.X);
            vectorArray[i] = RotateVector(direction, angle - (spreadDegrees / 2) + (spreadDegrees * percent), true);
        }
        return vectorArray;
    }
    public void Damage(int Damage, int IFrames)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - (int)MathF.Round(Damage * Random.Float(0.85f, 1.15f)));
        this.IFrames = IFrames;
    }
    public void Setup()
    {
        CurrentPhase = 1;
        ActionDuration = ActionTimer = 2;
        ActionCounter = -1;
        Position = new Vector2(Window.Width * 0.5f, Window.Height * -0.5f);
    }
    public void Update()
    {
        IFrames = Math.Max(0, IFrames - 1);
        ActionTimer = Math.Max(0, ActionTimer - Time.DeltaTime);
        AnimationPercent = Math.Clamp(1 - (ActionTimer / ActionDuration), 0, 1);

        Position += Velocity;

        // Collision Detection
        if (Position.Y >= Window.Height - Radius) // Bottom
        {
            Position.Y = Window.Height - Radius;
            if (Velocity.Y > 0) { Velocity.Y = 0; }
        }
        if (Position.Y <= Radius & CurrentAction != "Spawn") // Top
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

        // Rendering Body
        Draw.FillColor = new("993333");
        Draw.LineColor = new("4d1919");
        Draw.LineSize = 2;
        Draw.Circle(Position, Radius);

        // Rendering Shield
        if (Invincible)
        {
            Draw.FillColor = new(0, 255, 255, 50);
            Draw.LineColor = new(0, 255, 255);
            Draw.Circle(Position, Radius * 1.25f);
        }
    }
}