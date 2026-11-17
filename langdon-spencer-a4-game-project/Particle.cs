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
    ///     Rotates a vector by a given angle.
    /// </summary>
    /// <param name="degrees">  If true, specified angle is converted to radians before calculation</param>
    /// <returns>   The rotated vector.</returns>
    public Vector2 RotateVector(Vector2 vector, float angle, bool degrees)
    {
        float newAngle = angle;
        if (degrees)
        {
            newAngle = (angle * MathF.Tau / 360);
        }
        return new Vector2(vector.X * MathF.Cos(newAngle) - vector.Y * MathF.Sin(newAngle), vector.X * MathF.Sin(newAngle) + vector.Y * MathF.Cos(newAngle));
    }
    /// <summary>
    ///     Checks whether or not two circles are overlapping.
    /// </summary>
    /// <param name="Circle1Position">  The position of the first circle.</param>
    /// <param name="Circle1Radius">    The radius of the first circle.</param>
    /// <param name="Circle2Position">  The position of the second circle.</param>
    /// <param name="Circle2Radius">    The radius of the second circle.</param>
    /// <param name="Tolerance">    The amount of leeway for collision detection.</param>
    /// <returns>   'true' if the circles are overlapping, otherwise 'false'.</returns>
    public bool CircleCircleCollision(Vector2 Circle1Position, float Circle1Radius, Vector2 Circle2Position, float Circle2Radius, float Tolerance)
    {
        return Vector2.Distance(Circle1Position, Circle2Position) <= Tolerance * (Circle1Radius + Circle2Radius);
    }
    /// <summary>
    ///     Checks if a ray collides with a circle.
    /// </summary>
    /// <param name="RayOrigin">    The starting point of the ray.</param>
    /// <param name="RayDirection"> The direction vector for the ray.</param>
    /// <param name="RayThickness"> The thickness of the ray.</param>
    /// <param name="CirclePosition">   The position of the circle.</param>
    /// <param name="CircleRadius"> The radius of the circle.</param>
    /// <param name="Tolerance">    The amount of leeway for collision detection.</param>
    /// <returns>   'true' if the ray is touching the circle, otherwise 'false'.</returns>
    public bool RayCircleDetection(Vector2 RayOrigin, Vector2 RayDirection, float RayThickness, Vector2 CirclePosition, float CircleRadius, float Tolerance)
    {
        Vector2 vectorToTarget = CirclePosition - RayOrigin;
        float scalarProjection = Vector2.Dot(vectorToTarget, RayDirection) / RayDirection.LengthSquared();
        if (scalarProjection < 0)
        {
            return Vector2.Distance(RayOrigin, CirclePosition) <= Tolerance * (CircleRadius + RayThickness);
        }
        else
        {
            Vector2 closestPointToTarget = RayOrigin + RayDirection * scalarProjection;
            return Vector2.Distance(closestPointToTarget, CirclePosition) <= Tolerance * (CircleRadius + RayThickness);
        }
    }
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public abstract void Update();

}
internal class Shockwave(Vector2 Position, ColorF[] ColorPalette, float Lifetime, float Thickness, float StartRadius, float EndRadius, int BaseDamage, int IFrames, bool IsTangible, bool IsFriendly) : Projectile(Position, ColorPalette, Lifetime, BaseDamage, IFrames, IsFriendly)
{
    // PROPERTIES
    public bool IsTangible = IsTangible;
    public float Thickness = Thickness;
    public float StartRadius = StartRadius;
    public float EndRadius = EndRadius;

    // METHODS
    /// <summary>
    ///     Checks if the attack is hitting the target.
    /// </summary>
    /// <param name="Target">   The target to check for.</param>
    /// <param name="Tolerance">    The amount of leeway for the collsion detection.</param>
    /// <returns>   'true' if the target is being hit by the attack, otherwise 'false'.</returns>
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        return CircleCircleCollision(Position, float.Lerp(StartRadius, EndRadius, 1 - (Timer / Lifetime)), Target.Position, Target.Radius, Tolerance);
    }
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public override void Update()
    {
        Timer = MathF.Max(0, Timer - Time.DeltaTime);
        float animationPercent = 1 - (Timer / Lifetime);
        Draw.LineColor = new ColorF(ColorPalette[0].R, ColorPalette[0].G, ColorPalette[0].B, (1 - animationPercent));
        Draw.LineSize = Thickness;
        Draw.FillColor = Color.Clear;
        if (animationPercent == 1)
        {
            Expired = true;
        }
        else
        {
            Draw.Circle(Position, StartRadius * (1 - animationPercent) + EndRadius * animationPercent);
        }
    }
}
internal class Dust(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, ColorF[] ColorPalette, float Lifetime, float Radius) : Particle(Position, ColorPalette, Lifetime)
{
    // PROPERTIES
    public Vector2 Velocity = Velocity / Time.DeltaTime;
    public Vector2 Acceleration = Acceleration / Time.DeltaTime;
    public float Radius = Radius;

    // METHODS
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public override void Update()
    {
        Timer = MathF.Max(0, Timer - Time.DeltaTime);
        float animationPercent = 1 - (Timer / Lifetime);
        Position += (Velocity * Time.DeltaTime) + (Acceleration * 0.5f * MathF.Pow(Time.DeltaTime, 2));
        Velocity += Acceleration * Time.DeltaTime;
        if (animationPercent == 0)
        {
            Expired = true;
        }
        else
        {
            Draw.LineSize = 0;
            Draw.FillColor = new ColorF(ColorPalette[0].R, ColorPalette[0].G, ColorPalette[0].B, (1 - animationPercent));
            Draw.Circle(Position, Radius * (1 - animationPercent));
        }
    }
}
public abstract class Projectile(Vector2 Position, ColorF[] ColorPalette, float Lifetime, int BaseDamage, int IFrames, bool IsFriendly) : Particle(Position, ColorPalette, Lifetime)
{
    // PROPERTIES
    public int Damage = BaseDamage;
    public int IFrames = IFrames;
    public bool IsFriendly = IsFriendly;

    // METHODS
    /// <summary>
    ///     Checks if the attack is hitting the target.
    /// </summary>
    /// <param name="Target">   The target to check for.</param>
    /// <param name="Tolerance">    The amount of leeway for the collsion detection.</param>
    /// <returns>   'true' if the target is being hit by the attack, otherwise 'false'.</returns>
    public abstract bool IsHittingTarget(dynamic Target, float Tolerance);
}
internal class Beam : Projectile
{
    // PROPERTIES
    public dynamic? Source { get; set; }
    public bool LockToSource { get; set; }
    public Vector2 Direction { get; set; }
    public float Width { get; set; }
    public float ActualWidth { get; set; }

    // CONSTRUCTORS
    public Beam(Boss Source, Vector2 Direction, bool LockToSource, ColorF[] ColorPalette, float Lifetime, float Width, int BaseDamage, int IFrames, bool IsFriendly) : base(Source.Position + Source.Radius * Vector2.Normalize(Source.TargetPosition - Source.Position), ColorPalette, Lifetime, BaseDamage, IFrames, IsFriendly)
    {
        this.Source = Source;
        this.LockToSource = LockToSource;
        this.Direction = Direction;
        this.Width = Width;
        ActualWidth = Width;
    }
    public Beam(Vector2 Position, Vector2 Direction, ColorF[] ColorPalette, float Lifetime, float Width, int BaseDamage, int IFrames, bool IsFriendly) : base(Position, ColorPalette, Lifetime, BaseDamage, IFrames, IsFriendly)
    {
        this.Direction = Direction;
        this.Width = Width;
        ActualWidth = Width;
    }


    // METHODS
    /// <summary>
    ///     Checks if the attack is hitting the target.
    /// </summary>
    /// <param name="Target">   The target to check for.</param>
    /// <param name="Tolerance">    The amount of leeway for the collsion detection.</param>
    /// <returns>   'true' if the target is being hit by the attack, otherwise 'false'.</returns>
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        return RayCircleDetection(Position, Direction, ActualWidth, Target.Position, Target.Radius, Tolerance);
    }
    /// <summary>
    ///     Gets a random point along the border of the box where the beam intersects with it.
    /// </summary>
    public Vector2 GetPointAlongBorder()
    {
        Vector2 beamNormal = new(-Direction.Y, Direction.X);
        Vector2 beamStartPosition = Position + beamNormal * Random.Float(-ActualWidth, ActualWidth) * 0.9f;

        float tValue = float.PositiveInfinity;
        if (Direction.X != 0)
        {
            if ((150 - beamStartPosition.X) / Direction.X > 0)
            {
                tValue = (150 - beamStartPosition.X) / Direction.X;
            }
            else if ((1350 - beamStartPosition.X) / Direction.X > 0)
            {
                tValue = (1350 - beamStartPosition.X) / Direction.X;
            }
        }
        if (Direction.Y != 0)
        {
            if ((900 - beamStartPosition.Y) / Direction.Y > 0)
            {
                tValue = (900 - beamStartPosition.Y) / Direction.Y;
            }
            else if ((-beamStartPosition.Y) / Direction.Y > 0)
            {
                tValue = (-beamStartPosition.Y) / Direction.Y;
            }
        }
        return beamStartPosition + Direction * tValue;
    }
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public override void Update()
    {
        if (Source != null)
        {
            if (LockToSource) { Position = Source.Position + (Source.Radius + Width + 5) * Direction; }
        }
        Timer = MathF.Max(0, Timer - Time.DeltaTime);
        float animationPercent = Math.Clamp(1 - (Timer / Lifetime), 0, 1);
        Draw.LineColor = ColorPalette[0];
        Draw.LineSize = 5;
        Draw.FillColor = ColorPalette[1];
        if (animationPercent != 1)
        {
            if (animationPercent > 0f & animationPercent < 0.05f)
            {
                ActualWidth = (Width * animationPercent * 10);
            }
            else if (animationPercent >= 0.05f & animationPercent < 0.9f)
            {
                ActualWidth = Width * Random.Float(0.95f, 1.05f);
            }
            else if (animationPercent >= 0.9f & animationPercent < 1f)
            {
                ActualWidth = (Width * (1 - animationPercent) * 10);
            }
            Draw.Capsule(Position, Position + (Direction * Window.Width * 2), ActualWidth);
        }
        else
        {
            Expired = true;
        }
    }
}
internal class Bullet(Vector2 Position, Vector2 Velocity, Vector2 Acceleration, ColorF[] ColorPalette, float Radius, float Lifetime, int BaseDamage, int IFrames, bool Homing, bool IsFriendly) : Projectile(Position, ColorPalette, Lifetime, BaseDamage, IFrames, IsFriendly)
{
    // PROPERTIES
    public float Radius = Radius;
    public Vector2 Velocity = Velocity / Time.DeltaTime;
    public float Inertia = Velocity.Length() * 8f;
    public Vector2 Direction = Vector2.Normalize(Velocity);
    public Vector2 Acceleration = Acceleration / Time.DeltaTime;
    public Vector2 TrailEndPosition = Position;
    public bool Homing = Homing;

    // METHODS
    /// <summary>
    ///     Checks if the attack is hitting the target.
    /// </summary>
    /// <param name="Target">   The target to check for.</param>
    /// <param name="Tolerance">    The amount of leeway for the collsion detection.</param>
    /// <returns>   'true' if the target is being hit by the attack, otherwise 'false'.</returns>
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        return CircleCircleCollision(Position, Radius, Target.Position, Target.Radius, Tolerance);
    }
    /// <summary>
    ///     Curves the velocity of the bullet towards the target.
    /// </summary>
    /// <param name="Target">   The target to curve towards.</param>
    public void CurveToTarget(dynamic Target)
    {
        Velocity = Vector2.Normalize(((Inertia - 1) * Velocity + (Velocity.Length() * Vector2.Normalize(Target.Position - Position))) / Inertia) * Velocity.Length();
    }
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public override void Update()
    {
        Vector2 trailEndPosition = Position - 0.25f * Radius * Velocity * Time.DeltaTime;
        Position += (Velocity * Time.DeltaTime) + (0.5f * Acceleration * MathF.Pow(Time.DeltaTime, 2));
        Velocity += Acceleration * Time.DeltaTime;
        Timer = MathF.Max(0, Timer - Time.DeltaTime);
        if (Timer == 0 | Position.X < 150 - Radius | Position.X > Window.Width - 150 + Radius | Position.Y < -Radius | Position.Y > Window.Height + Radius)
        {
            Expired = true;
        }
        else
        {
            Draw.LineColor = Draw.FillColor = ColorPalette[0];
            Draw.LineSize = Radius * 0.05f;
            Draw.Triangle(Position + RotateVector(Direction, 90, true) * Radius, trailEndPosition, Position + RotateVector(Direction, -90, true) * Radius);
            Draw.FillColor = ColorPalette[1];
            Draw.Circle(Position, Radius * 0.95f);
        }
    }
}
internal class SwordSwing(Player Source, float Width, ColorF[] ColorPalette, float Lifetime, int BaseDamage, int IFrames) : Projectile(Source.Position + Source.Radius * Source.Direction, ColorPalette, Lifetime, BaseDamage, IFrames, true)
{
    // PROPERTIES

    public Player Source = Source;
    public Vector2 Size = new(Width, Source.Radius * 8);
    

    // CONSTANTS

    public Vector2 Direction = Vector2.Normalize(Input.GetMousePosition() - Source.Position);
    public float ArcDegrees = 150;
    public Vector2 ActualDirection = Vector2.Normalize(Input.GetMousePosition() - Source.Position);

    // METHODS
    /// <summary>
    ///     Controls the animation of the sword swing.
    /// </summary>
    /// <param name="tValue">   The percent (ranging from 0.0f to 1.0f) of the animation.</param>
    public float AnimationControl(float tValue)
    {
        const float windUpLength = 1.3f;
        const float windUpAccel = windUpLength + 1;
        float windUp = windUpAccel * MathF.Pow(tValue, 3) - windUpLength * MathF.Pow(tValue, 2);

        const float overshootLength = 1.8f;
        const float overshootDamping = overshootLength + 1;
        float followThrough = 1 + overshootDamping * MathF.Pow(tValue - 1, 3) + overshootLength * MathF.Pow(tValue - 1, 2);

        float transitionWeight = MathF.Pow(MathF.Pow(tValue, 2) * (3 - 2 * tValue), 1.1f);

        return (1 - transitionWeight) * windUp + transitionWeight * followThrough;
    }
    /// <summary>
    ///     Handles collision detection for the projectile.
    /// </summary>
    /// <param name="Target">   The entity the projectile is checking for. Can be a Player, Minion, or Boss object.</param>
    /// <param name="Tolerance">    An amount of leeway the projectile's detection will allow for. Values greater than 1.0f mean the projectile doesn't need to be hitting it to detect the target.</param>
    /// <returns></returns>
    public override bool IsHittingTarget(dynamic Target, float Tolerance)
    {
        static float NormalizeAngle(float angle) { return ((angle % MathF.Tau) + MathF.Tau) % MathF.Tau; }
        float distanceToTarget = Vector2.Distance(Target.Position, Position);
        if (distanceToTarget > Size.Y)
        {
            return false;
        }
        if (distanceToTarget < 0)
        {
            return true;
        }

        float startAngle = NormalizeAngle(MathF.Atan2(Direction.Y, Direction.X) + (ArcDegrees / 2));
        float endAngle = NormalizeAngle(MathF.Atan2(Direction.Y, Direction.X) + (ArcDegrees / 2) - ArcDegrees);

        float swingArcSpan = NormalizeAngle(endAngle - startAngle);
        float angleToTarget = NormalizeAngle(NormalizeAngle(MathF.Atan2((Target.Position - Position).Y, (Target.Position - Position).X)) - startAngle);
        if (angleToTarget <= swingArcSpan)
        {
            return true;
        }
        return RayCircleDetection(Position, RotateVector(Vector2.UnitX, startAngle, false), 0, Target.Position, Target.Radius, Tolerance) | RayCircleDetection(Position, RotateVector(Vector2.UnitX, endAngle, false), 0, Target.Position, Target.Radius, Tolerance);
    }
    /// <summary>
    ///     This should be called once per frame.
    /// </summary>
    public override void Update()
    {
        Position = Source.Position + Source.Radius * Direction;
        Timer = MathF.Max(0, Timer - Time.DeltaTime);
        float animationPercent = AnimationControl(Math.Clamp(1 - (Timer / Lifetime), 0, 1));
        if (Timer == 0)
        {
            Expired = true;
        }
        else
        {
            
            Draw.FillColor = ColorPalette[0];
            Draw.LineSize = 0;
            if (Direction.X > 0)
            {
                ActualDirection = RotateVector(Direction, -(ArcDegrees / 2) - ArcDegrees * animationPercent, true);
                Draw.Quad(Position, Position + 0.25f * Size.Y * RotateVector(Direction, -(ArcDegrees / 2) + 15 + ArcDegrees * animationPercent, true), Position + Size.Y * RotateVector(Direction, -(ArcDegrees / 2) + ArcDegrees * animationPercent, true), Position + 0.25f * Size.Y * RotateVector(Direction, -(ArcDegrees / 2) - 15 + ArcDegrees * animationPercent, true));
            }
            else
            {
                ActualDirection = RotateVector(Direction, (ArcDegrees / 2) - ArcDegrees * animationPercent, true);
                Draw.Quad(Position, Position + 0.25f * Size.Y * RotateVector(Direction, (ArcDegrees / 2) + 15 - ArcDegrees * animationPercent, true), Position + Size.Y * RotateVector(Direction, (ArcDegrees / 2) - ArcDegrees * animationPercent, true), Position + 0.25f * Size.Y * RotateVector(Direction, +(ArcDegrees / 2) - 15 - ArcDegrees * animationPercent, true));
            }

        }
    }
}