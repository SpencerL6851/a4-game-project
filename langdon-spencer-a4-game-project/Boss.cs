using System;
using System.Numerics;

namespace MohawkGame2D;

public class Boss
{
    // PROPERTIES
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Direction = -Vector2.UnitY;
    public Vector2 TargetPosition;
    public Vector2 ZoomStartPosition;
    public bool Invincible = true;
    public int CurrentPhase;
    public string CurrentAction = "Spawn";
    public string NextAction = "Shoot Bullets";
    public int ActionCounter;
    public float ActionDuration;
    public float ActionTimer;
    public float ActionStartTime;
    public float AttackCooldown;
    public float AnimationPercent;
    public int CurrentHealth = 7500;
    public int IFrames = 0;
    public bool LockToScreen;
    public float Alpha = 0f;

    // CONSTANTS
    public int MaxHealth = 7500;
    public int ContactDamage = 50;
    public float Radius = 45;

    // METHODS

    /// <summary>
    ///     Simple easing function for 'Zooming' state.
    /// </summary>
    /// <param name="percent">  The animation percent of the current action.</param>
    /// <returns>   The eased animation percent of the animation.</returns>
    public float InOutSineEasing(float percent)
    {
        return (-(MathF.Cos(MathF.PI * percent) - 1)) / 2f;
    }

    /// <summary>
    ///     Rotates a vector by a given angle.
    /// </summary>
    /// <param name="degrees">   If true, specified angle is converted to radians before calculation</param>
    /// <returns>   The rotated vector.</returns>
    public Vector2 RotateVector(Vector2 vector, float angle, bool degrees)
    {
        float newAngle = angle;
        if (degrees)
        {
            newAngle = (angle * MathF.PI / 180);
        }
        return new Vector2(vector.X * MathF.Cos(newAngle) - vector.Y * MathF.Sin(newAngle), vector.X * MathF.Sin(newAngle) + vector.Y * MathF.Cos(newAngle));
    }
    /// <summary>
    ///     Calculates the vectors for a fan of vectors, usually directions for bullets to travel.
    /// </summary>
    /// <param name="direction">    The direction vector for the fan.</param>
    /// <param name="spreadDegrees">    The amount of degrees wide the fan will be.</param>
    /// <param name="amount">   The amount of vectors to return. More vectors means less space between them.</param>
    /// <returns>   An array of vectors spaced evenly apart from one another with the middle vector being the given direction.</returns>
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
    /// <summary>
    ///     Damages the boss and gives them invincibility frames.
    /// </summary>
    /// <param name="Damage">   The base damage of the attack. The actual damage dealt will be within 15% of the base damage.</param>
    /// <param name="IFrames">  The amount of invincibility frames the attack will provide.</param>
    public void Damage(int Damage, int IFrames)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - (int)MathF.Round(Damage * Random.Float(0.85f, 1.15f)));
        this.IFrames = IFrames;
    }
    /// <summary>
    /// Initializes the values for the boss.
    /// </summary>
    public void Setup()
    {
        CurrentPhase = 1;
        ActionDuration = ActionTimer = 2;
        ActionCounter = -1;
        Position = new Vector2(Window.Width * 0.5f, Window.Height * -0.5f);
        LockToScreen = false;
    }
    /// <summary>
    /// This should be called once per frame.
    /// </summary>
    public void Update()
    {
        IFrames = Math.Max(0, IFrames - 1);
        ActionTimer = Math.Max(0, ActionTimer - Time.DeltaTime);
        AnimationPercent = Math.Clamp(1 - (ActionTimer / ActionDuration), 0, 1);
        AttackCooldown = MathF.Max(0, AttackCooldown - Time.DeltaTime);

        Position += Velocity;

        // Collision Detection
        if (LockToScreen)
        {
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
        }

        // Rendering Body
        Draw.FillColor = new ColorF(0.6f, 0.2f, 0.2f, Alpha);
        Draw.LineColor = new ColorF(0.4f, 0.13f, 0.13f, Alpha);
        Draw.LineSize = 2;
        float angle = MathF.Atan2(Direction.Y, Direction.X) * 180f / MathF.PI;
        Draw.Quad(Position + RotateVector(new Vector2(0, -1.15f * Radius), angle + 90, true), Position + RotateVector(new Vector2(Radius * 1.15f, -Radius * 0.85f), angle + 90, true), Position + RotateVector(new Vector2(Radius * 1.05f, Radius), angle + 90, true), Position);
        Draw.Quad(Position + RotateVector(new Vector2(0, -1.15f * Radius), angle + 90, true), Position + RotateVector(new Vector2(-Radius * 1.15f, -Radius * 0.85f), angle + 90, true), Position + RotateVector(new Vector2(-Radius * 1.05f, Radius), angle + 90, true), Position);
        Draw.Circle(Position, Radius);

        // Rendering Shield
        if (Invincible & CurrentAction != "Defeat")
        {
            Draw.FillColor = new ColorF(0, 1, 1, 0.3f * Alpha);
            Draw.LineColor = new ColorF(0, 1, 1, 0.8f * Alpha);
            Draw.Circle(Position, Radius * 1.45f);
        }
    }
}