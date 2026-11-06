using System;
using System.Numerics;

namespace MohawkGame2D;

public abstract class Particle(Vector2 Position, ColorF[] ColorPalette, float Lifetime)
{
    // PROPERTIES
    public Vector2 Position = Position;
    public ColorF[] ColorPalette = ColorPalette;
    public float Lifetime = Lifetime;
    public float Timer = Lifetime;
    public bool Expired = false;
    
    // CONSTANTS
    public readonly float SpawnTime = Time.SecondsElapsed;

    // METHODS
    
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
            newAngle = (angle * MathF.Tau / 360);
        }
        return new Vector2(vector.X * MathF.Cos(newAngle) - vector.Y * MathF.Sin(newAngle), vector.X * MathF.Sin(newAngle) + vector.Y * MathF.Cos(newAngle));
    }
    public abstract void Update();

}
public abstract class Attack(Vector2 Position, ColorF[] ColorPalette, float Lifetime, int Damage, int IFrames) : Particle(Position, ColorPalette, Lifetime)
{
    // CONSTANTS
    public int Damage = Damage;
    public int IFrames = IFrames;
    public abstract bool IsHittingTarget(dynamic Target, float Tolerance);
}

internal class Beam(Vector2 Position, ColorF[] ColorPalette, float Lifetime, Vector2 Direction, float Width, int Damage, int IFrames) : Attack(Position, ColorPalette, Lifetime, Damage, IFrames)
{
    // CONSTANTS
    public Vector2 Direction = Direction;
    public float Width = Width;

    // METHODS
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        throw new NotImplementedException();
    }

    public override void Update()
    {
        Timer -= Time.DeltaTime;
        float animationPercent = Math.Clamp(1 - (Timer / Lifetime), 0, 1);
        Draw.LineColor = ColorPalette[0];
        Draw.FillColor = ColorPalette[1];
        if (animationPercent > 0f & animationPercent < 0.1f)
        {
            Draw.Capsule(Position, Position + (Direction * 1500f), (Width * animationPercent * 10));
        }
        else if (animationPercent >= 0.1f & animationPercent < 0.9f)
        {
            Draw.Capsule(Position, Position + (Direction * 1500f), Width * (100 / Random.Integer(95, 106)));
        }
        else if (animationPercent >= 0.9f & animationPercent < 1f)
        {
            Draw.Capsule(Position, Position + (Direction * 1500f), (Width * (1 - animationPercent) * 10));
        }
        else
        {
            Expired = true;
        }
    }
}

internal class Shockwave(Vector2 Position, ColorF[] ColorPalette, float Lifetime, float Thickness, float StartRadius, float EndRadius) : Particle(Position, ColorPalette, Lifetime)
{
    // CONSTANTS
    public float Thickness = Thickness;
    public float StartRadius = StartRadius;
    public float EndRadius = EndRadius;
    
    // METHODS
    public override void Update()
    {
        Timer -= Time.DeltaTime;
        float animationPercent = Math.Clamp(1 - (Timer / Lifetime), 0, 1);
        Draw.LineColor = new ColorF(ColorPalette[0].R, ColorPalette[0].G, ColorPalette[0].B, (1 - animationPercent));
        Draw.LineSize = Thickness;
        Draw.FillColor = new ColorF(0, 0, 0, 0);
        if (animationPercent == 1)
        {
            Expired = true;
        }
        else
        {
            Draw.Circle(Position, float.Lerp(StartRadius, EndRadius, animationPercent));
        }
    }
}

internal class Bullet(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, ColorF[] ColorPalette, float Radius, float Lifetime, int Damage, int IFrames) : Attack(Position, ColorPalette, Lifetime, Damage, IFrames)
{
    public float Radius = Radius;
    public Vector2 Velocity = Velocity;
    public Vector2 Acceleration = Acceleration;
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        return Vector2.Distance(Target.Position, Position) <= (Radius + Target.Radius) * Tolerance;
    }
    public override void Update()
    {
        if (Acceleration != Vector2.Zero)
        {
            Acceleration -= Velocity;
            Velocity += Acceleration;
        }
        Position += Velocity;
        Timer -= Time.DeltaTime;
        if (Timer <= 0 | Position.X < -Radius | Position.X > Window.Width + Radius | Position.Y < -Radius | Position.Y > Window.Height + Radius)
        {
            Expired = true;
        }
        else
        {
            Draw.LineColor = ColorPalette[0];
            Draw.FillColor = ColorPalette[1];
            Draw.LineSize = Radius * 0.15f;
            Draw.Circle(Position, Radius * 0.85f);
        }
    }
}
internal class HomingBullet(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, ColorF[] ColorPalette, float Radius, float Lifetime, int Damage, int IFrames, dynamic Target, float Inertia, float TrackingRadius) : Bullet(Position, Velocity, Acceleration, ColorPalette, Radius, Lifetime, Damage, IFrames)
{
    public dynamic Target = Target;
    public float Inertia = Inertia;
    public float TrackingRadius = TrackingRadius;

    public override void Update()
    {
        if (Acceleration != Vector2.Zero)
        {
            Acceleration -= Velocity;
            Velocity += Acceleration;
        }
        if (Vector2.Distance(Position, Target.Position) < TrackingRadius | TrackingRadius == -1)
        {
            Velocity = Vector2.Normalize(((Inertia - 1) * Velocity + (Velocity.Length() * Vector2.Normalize(Target.Position - Position))) / Inertia) * Velocity.Length();
        }
        Vector2 direction = Vector2.Normalize(Velocity);
        Position += Velocity;
        Timer -= Time.DeltaTime;
        if (Timer <= 0 | Position.X < -Radius | Position.X > Window.Width + Radius | Position.Y < -Radius | Position.Y > Window.Height + Radius)
        {
            Expired = true;
        }
        else
        {
            Draw.LineColor = ColorPalette[0];
            Draw.FillColor = ColorPalette[1];
            Draw.LineSize = Radius * 0.15f;
            Draw.Circle(Position, Radius * 0.85f);
            Draw.FillColor = new ColorF(0.85f * ColorPalette[0].R, 0.85f * ColorPalette[0].G, 0.85f * ColorPalette[0].B);
            Draw.LineSize = 0;
            Draw.Quad(Position + direction * (Radius - 1), Position + RotateVector(direction, 135, true) * (Radius - 1), Position, Position + RotateVector(direction, -135, true) * (Radius - 1));
        }
    }
}