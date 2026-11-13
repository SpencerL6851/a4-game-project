// Include the namespaces (code libraries) you need below.
using System;
using System.Collections.Generic;
using System.Numerics;

// The namespace your code is in.
namespace MohawkGame2D
{
    /// <summary>
    ///     Your game code goes inside this class!
    /// </summary>
    public class Game
    {
        public char Difficulty = 'N';
        public List<dynamic> Particles = [];
        public List<Minion> Enemies = [];
        public Boss Boss = new();
        public Player Player = new();

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
                newAngle = (angle * MathF.PI / 180);
            }
            return new Vector2(vector.X * MathF.Cos(newAngle) - vector.Y * MathF.Sin(newAngle), vector.X * MathF.Sin(newAngle) + vector.Y * MathF.Cos(newAngle));
        }

        /// <summary>
        ///     Setup runs once before the game loop begins.
        /// </summary>
        public void Setup()
        {
            Window.SetSize(1500, 900);
            Boss.Setup();
            Player.Setup(Difficulty);
        }

        /// <summary>
        ///     Update runs every frame.
        /// </summary>
        public void Update()
        {
            Window.ClearBackground(96);
            Text.Color = Color.Black;
            Text.Size = 20;
            Text.Draw($"Current Action: {Boss.CurrentAction} (Phase {Boss.CurrentPhase}) \nNext Action: {Boss.NextAction} \nCounter: {Boss.ActionCounter} \nTimer: {Boss.ActionTimer} \nAnimation {float.Round(Boss.AnimationPercent * 100)}% Complete", new Vector2(150, 0));
            switch (Boss.CurrentPhase)
            {
                // Phase 1 Logic
                case 1:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Phase Change":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Boss.ActionCounter++;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase++;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.CurrentAction = "Idle";
                                        Boss.NextAction = "Shoot Bullet Fan";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
                                    break;
                                }
                            case "Shoot Bullets":
                                {
                                    Boss.Position = Boss.TargetPosition + new Vector2(150 * MathF.Sin(Boss.AnimationPercent * MathF.Tau), 0);
                                    Vector2 directionToPlayer = Vector2.Normalize(Player.Position - Boss.Position);
                                    if (Boss.ActionTimer < (0.375f * Boss.ActionCounter))
                                    {
                                        Boss.Direction = directionToPlayer;
                                        Particles.Add(new Bullet(
                                            Position: Boss.Position + Boss.Radius * directionToPlayer,
                                            Velocity: 12.5f * directionToPlayer,
                                            Acceleration: Vector2.Zero,
                                            ColorPalette: [new(1, 0, 0), new(0)],
                                            Radius: 20,
                                            Lifetime: 5,
                                            BaseDamage: 7,
                                            IFrames: 5,
                                            Homing: true,
                                            IsFriendly: false
                                            ));
                                        Boss.ActionCounter--;
                                    }
                                    if (Boss.ActionTimer <= 0)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, "Basic"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, "Basic"));
                                    Boss.ActionDuration = Boss.ActionTimer = 0.25f;
                                    Boss.CurrentAction = "Idle";
                                    break;
                                }
                            case "Flying":
                                {
                                    Boss.Position = Boss.TargetPosition + new Vector2(300 * MathF.Sin(Boss.AnimationPercent * MathF.Tau), 100 * MathF.Sin(2 * Boss.AnimationPercent * MathF.Tau));
                                    if (Boss.ActionTimer == 0)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                    }
                                    break;
                                }
                            case "Dash":
                                {
                                    if (Boss.ActionCounter % 2 == 0)
                                    {
                                        if (Boss.AnimationPercent != 1)
                                        {
                                            Boss.Direction = Vector2.Normalize(Player.Position - Boss.Position);
                                            Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f) + Random.Direction() * 2;
                                        }
                                        else
                                        {
                                            Boss.Velocity = 30 * Vector2.Normalize(Player.Position - Boss.Position);
                                            Boss.ActionCounter--;
                                        }
                                    }
                                    else
                                    {
                                        if (Boss.Position.X <= 150 + Boss.Radius | Boss.Position.X >= Window.Width - 150 - Boss.Radius)
                                        {
                                            Boss.Position.X = Math.Clamp(Boss.Position.X, 150 + Boss.Radius, Window.Width - 150 - Boss.Radius);
                                            Boss.Velocity.X *= -1;
                                        }
                                        if (Boss.Position.Y < Boss.Radius)
                                        {
                                            Boss.Position.Y = Boss.Radius;
                                            Boss.Velocity.Y *= -1;
                                        }
                                        if (Boss.Position.Y >= Window.Height - Boss.Radius)
                                        {
                                            Boss.Position.Y = Window.Height - Boss.Radius;
                                            Particles.Add(new Shockwave(Boss.Position + new Vector2(0, Boss.Radius), [new(1f)], 0.25f, 10, 40, 200, (int)(Boss.ContactDamage * 1.5f), 20, true, false));
                                            int dustAmount = Random.Integer(10, 18);
                                            for (int i = 0; i < dustAmount; i++)
                                            {
                                                Particles.Add(new Dust(Boss.Position + new Vector2(Random.Float(-Boss.Radius, Boss.Radius), Boss.Radius), 8 * Random.PointInCircle() + new Vector2(0, -15), new Vector2(0, 1), [new(0)], Random.Float(1, 3), Random.Float(3, 6)));
                                            }
                                            Boss.Velocity = Vector2.Zero;
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.Direction = Vector2.UnitY;
                                            Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            if (Boss.ActionCounter == 1)
                                            {
                                                Boss.NextAction = "Shoot Bullets";
                                                Boss.ActionTimer = Boss.ActionDuration = 3;
                                            }
                                            else
                                            {
                                                Boss.NextAction = "Dash";
                                                Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                            }
                                            Boss.ActionCounter--;

                                        }
                                    }
                                    break;
                                }
                            case "Spawn":
                                {
                                    if (Boss.ActionCounter == -1)
                                    {
                                        float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                        Boss.Position.Y = (easedAnimationPercent * (Window.Height * 0.25f)) + ((1 - easedAnimationPercent) * (Window.Height * -0.5f));
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 2.5f;
                                            Boss.ActionCounter = 0;
                                            Boss.TargetPosition = new(Window.Width * 0.5f, Window.Height * 0.25f);
                                        }
                                    }
                                    else
                                    {
                                        Window.ClearBackground(new Color(255, 0, 0, 8));
                                        Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                        if (Boss.ActionTimer <= Boss.ActionDuration * (1 - Boss.ActionCounter / (20 * Boss.ActionDuration)))
                                        {
                                            Boss.Direction = RotateVector(Vector2.UnitY, Random.Float(-5, 5), true);
                                            Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.25f, 7.5f, Boss.Radius, Boss.Radius * 3, 0, 0, false, true));
                                            Boss.ActionCounter++;
                                        }
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.Direction = Vector2.UnitY;
                                            Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            Boss.CurrentAction = "Idle";
                                            Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                            Boss.Invincible = false;
                                        }
                                    }

                                    break;
                                }
                            case "Zooming":
                                {
                                    float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                    Boss.Position = Vector2.Lerp(Boss.ZoomStartPosition, Boss.TargetPosition, easedAnimationPercent);
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        if (Boss.NextAction == "Shoot Bullets")
                                        {
                                            Boss.TargetPosition = Boss.Position;
                                            Boss.NextAction = "Summon";
                                            Boss.ActionDuration = Boss.ActionTimer = 7.5f;
                                            Boss.ActionCounter = 20;
                                        }
                                        else if (Boss.NextAction == "Dash")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 0.25f;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1f;
                                        }
                                    }
                                    break;
                                }
                            case "Idle":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {

                                            case "Shoot Bullets":
                                                {
                                                    Boss.TargetPosition = Boss.Position;
                                                    Boss.NextAction = "Summon";
                                                    Boss.ActionDuration = Boss.ActionTimer = 7.5f;
                                                    Boss.ActionCounter = 20;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Flying";
                                                    break;
                                                }
                                            case "Flying":
                                                {
                                                    Boss.TargetPosition = Boss.Position;
                                                    Boss.ActionDuration = Boss.ActionTimer = Random.Float(3f, 6f);
                                                    Boss.NextAction = "Dash";
                                                    break;
                                                }
                                            case "Dash":
                                                {
                                                    Boss.ActionCounter = 6;
                                                    Boss.ActionDuration = Boss.ActionTimer = 0.25f;
                                                    Boss.NextAction = "Shoot Bullets";
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                // Phase 2 Logic
                case 2:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Phase Change":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Boss.ActionCounter++;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase++;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.CurrentAction = "Idle";
                                        Boss.NextAction = "Charge";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
                                    break;
                                }
                            case "Shoot Bullet Fan":
                                {
                                    if (Boss.ActionTimer == 0)
                                    {
                                        if (Boss.ActionCounter > 0)
                                        {
                                            Boss.Direction = Vector2.Normalize(Player.Position - Boss.Position);
                                            Vector2[] _ = Boss.GetBulletFanVectors(Boss.Direction, 120 - 12.5f * Boss.ActionCounter, 15 - 2 * Boss.ActionCounter);
                                            for (int i = 0; i < _.Length; i++)
                                            {
                                                Particles.Add(new Bullet(
                                                    Position: Boss.Position + Boss.Radius * _[i],
                                                    Velocity: 10f * _[i],
                                                    Acceleration: Vector2.Zero,
                                                    ColorPalette: [new(1, 0, 0), new(0)],
                                                    Radius: 7.5f,
                                                    Lifetime: 5,
                                                    BaseDamage: 7,
                                                    IFrames: 5,
                                                    Homing: false,
                                                    IsFriendly: false
                                                    ));
                                            }
                                            Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                            Boss.ActionCounter--;
                                        }
                                        else
                                        {
                                            Boss.CurrentAction = "Idle";
                                            Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                        }
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, "Flying"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, "Flying"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(0, 190), new Vector2(0, 20), Vector2.Zero, "Basic"));
                                    Boss.ActionDuration = Boss.ActionTimer = 0.75f;
                                    Boss.CurrentAction = "Idle";
                                    break;
                                }
                            case "Beam":
                                {

                                    if (Boss.ActionCounter % 2 == 0)
                                    {
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.ActionCounter--;
                                            Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                            if (Random.CoinFlip())
                                            {
                                                Boss.TargetPosition = new(Window.Width - (150 + Boss.Radius), Window.Height * 0.25f);
                                            }
                                            else
                                            {
                                                Boss.TargetPosition = new(150 + Boss.Radius, Window.Height * 0.25f);
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.TargetPosition = Boss.Position + Boss.Radius * Vector2.UnitY;
                                            Particles.Add(new Beam(Boss, [new(1, 0, 0), new(0)], 2f, Boss.Radius * 1.25f, 1, 1, false, true));
                                            Boss.Velocity = Vector2.Zero;
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            Boss.ActionTimer = Boss.ActionDuration = 2f;
                                            if (Boss.ActionCounter == 1)
                                            {
                                                Boss.NextAction = "Idle";
                                            }
                                            else
                                            {
                                                Boss.NextAction = "Beam";
                                            }
                                            Boss.ActionCounter--;
                                        }
                                    }
                                    break;
                                }
                            case "Zooming":
                                {
                                    float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                    Boss.Position = Vector2.Lerp(Boss.ZoomStartPosition, Boss.TargetPosition, easedAnimationPercent);
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        if (Boss.NextAction == "Beam")
                                        {
                                            Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                        }
                                        if (Boss.NextAction == "Idle")
                                        {
                                            Boss.NextAction = "Shoot Bullet Fan";
                                            Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1f;
                                        }
                                    }
                                    break;
                                }
                            case "Idle":
                                {
                                    Boss.Direction = Vector2.UnitY;
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {
                                            case "Shoot Bullet Fan":
                                                {
                                                    Boss.NextAction = "Summon";
                                                    Boss.ActionCounter = 3;
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.15f;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Beam";
                                                    Boss.ActionCounter = 6;
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                // Phase 3 Logic
                case 3:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Phase Change":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, false));
                                        Boss.ActionCounter++;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase++;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.NextAction = "Spin";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
                                    break;
                                }
                            case "Charge":
                                {
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1, 0, 0)], Boss.ActionDuration, Boss.Radius / 3, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, 0, false, false));
                                        Boss.ActionCounter++;
                                    }
                                    Vector2 randomDirection = Random.Direction();
                                    float randomLifetime = Random.Float(0.5f, MathF.Min(3, Boss.ActionTimer));
                                    float distanceFromCenter = Boss.Radius * Random.Float(2.75f, 5.5f);
                                    Particles.Add(new Dust(Boss.Position + randomDirection * distanceFromCenter, -randomDirection * distanceFromCenter / randomLifetime * Time.DeltaTime, Vector2.Zero, [new(1, 0, 0)], randomLifetime, Random.Float(8, 12)));
                                    Boss.TargetPosition = Player.Position;
                                    Boss.Direction = Vector2.Normalize(Boss.TargetPosition - Boss.Position);
                                    Draw.LineColor = Color.Red;
                                    if (MathF.Floor(Time.SecondsElapsed * 4) % 2 == 0)
                                    {
                                        Draw.LineColor = Color.White;
                                    }
                                    Draw.LineSize = 5;
                                    Draw.Line(Boss.Position, Boss.TargetPosition + Vector2.Distance(Vector2.Zero, Window.Size) * Vector2.Normalize(Boss.TargetPosition - Boss.Position));
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.NextAction = "Beam";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.75f;
                                        Boss.ActionCounter = 0;
                                    }
                                    break;
                                }
                            case "Beam":
                                {
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Boss.ActionCounter++;
                                        Particles.Add(new Beam(Boss, [new(1, 0, 0), new(0)], Boss.ActionDuration, Boss.Radius * 2.75f, 4, 1, false, true));
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.NextAction = "Flying";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f;
                                    }
                                    break;
                                }
                            case "Flying":
                                {
                                    Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f) + new Vector2(300 * MathF.Sin(Boss.AnimationPercent * MathF.Tau), 100 * MathF.Sin(Boss.AnimationPercent * 2 * MathF.Tau));
                                    Boss.ActionCounter++;
                                    if (Boss.ActionCounter == 25)
                                    {
                                        Boss.ActionCounter = 0;
                                        Vector2[] _ = Boss.GetBulletFanVectors(Vector2.UnitY, 45, 5);
                                        for (int i = 0; i < _.Length; i++)
                                        {
                                            Particles.Add(new Bullet(
                                                Position: Boss.Position + Boss.Radius * _[i],
                                                Velocity: 10f * _[i],
                                                Acceleration: Vector2.Zero,
                                                ColorPalette: [new(1, 0, 0), new(0)],
                                                Radius: 12.5f,
                                                Lifetime: 5,
                                                BaseDamage: 7,
                                                IFrames: 5,
                                                Homing: false,
                                                IsFriendly: false
                                                ));
                                        }
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.ZoomStartPosition = Boss.Position;
                                        Boss.TargetPosition = new(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.NextAction = "Summon";
                                        Boss.CurrentAction = "Zooming";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    if (Boss.ActionCounter == 1)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, "Basic"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, "Basic"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(80, 170), new Vector2(7.5f, 20), Vector2.Zero, "Flying"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(-80, 170), new Vector2(-7.5f, 20), Vector2.Zero, "Flying"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(0, 190), new Vector2(0, 20), Vector2.Zero, "Flying"));
                                        Boss.ActionCounter--;
                                        Boss.Invincible = true;
                                    }
                                    if (Enemies.Count == 0)
                                    {
                                        Boss.Invincible = false;
                                        Boss.ActionDuration = Boss.ActionTimer = 0.75f;
                                        Boss.NextAction = "Charge";
                                        Boss.CurrentAction = "Idle";
                                    }

                                    break;
                                }
                            case "Zooming":
                                {
                                    float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                    Boss.Position = Vector2.Lerp(Boss.ZoomStartPosition, Boss.TargetPosition, easedAnimationPercent);
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        if (Boss.NextAction == "Summon")
                                        {
                                            Boss.NextAction = "Charge";
                                            Boss.ActionCounter = 1;
                                            Boss.ActionTimer = Boss.ActionDuration = 0.75f;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1f;
                                        }

                                    }
                                    break;
                                }
                            case "Idle":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    if (Boss.NextAction == "Beam")
                                    {
                                        Boss.Direction = Vector2.Normalize(Boss.TargetPosition - Boss.Position);
                                        Draw.LineColor = Color.Red;
                                        if (MathF.Floor(Time.SecondsElapsed * 16) % 2 == 0)
                                        {
                                            Draw.LineColor = Color.White;
                                        }
                                        Draw.LineSize = 5;
                                        Draw.Line(Boss.Position, Boss.TargetPosition + Vector2.Distance(Vector2.Zero, Window.Size) * Vector2.Normalize(Boss.TargetPosition - Boss.Position));
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {
                                            case "Charge":
                                                {
                                                    Boss.NextAction = "Beam";
                                                    Boss.ActionTimer = Boss.ActionDuration = 2f;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    Boss.NextAction = "Flying";
                                                    Boss.ActionTimer = Boss.ActionDuration = 2f;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Flying":
                                                {
                                                    Boss.NextAction = "Summon";
                                                    Boss.TargetPosition = Boss.Position;
                                                    Boss.ActionTimer = Boss.ActionDuration = 5f;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Charge";
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                // Phase 4 Logic
                case 4:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Defeat":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    break;
                                }
                            case "Spin":
                                {
                                    break;
                                }
                            case "Summon":
                                {
                                    break;
                                }
                            case "Charge":
                                {
                                    break;
                                }
                            case "Beam":
                                {
                                    break;
                                }
                            case "Zooming":
                                {
                                    float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                    Boss.Position = Vector2.Lerp(Boss.ZoomStartPosition, Boss.TargetPosition, easedAnimationPercent);
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        if (Boss.NextAction == "Idle")
                                        {
                                            Boss.NextAction = "Shoot Bullets";
                                            Boss.ActionTimer = Boss.ActionDuration = 0.5f;
                                        }
                                    }
                                    break;
                                }
                            case "Idle":
                                {
                                    Boss.Direction = -Vector2.UnitY;
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {
                                            case "Spin":
                                                {
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    break;
                                                }
                                            case "Charge":
                                                {
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    break;
                                                }
                                        }
                                    }
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                        break;
                    }
            }
            if (Boss.CurrentHealth == 0 & !(Boss.CurrentAction == "Defeat" | Boss.NextAction == "Defeat"))
            {
                Boss.CurrentAction = "Defeat";
                Boss.ActionCounter = 0;
                Boss.ActionDuration = Boss.ActionTimer = 0;
            }
            else if (Boss.CurrentHealth <= (int)(Boss.MaxHealth * (1 - 0.25f * Boss.CurrentPhase)) & !(Boss.CurrentAction == "Phase Change" | Boss.NextAction == "Phase Change"))
            {
                Boss.CurrentAction = "Zooming";
                Boss.ZoomStartPosition = Boss.Position;
                Boss.Invincible = true;
                Boss.NextAction = "Phase Change";
                Boss.ActionTimer = Boss.ActionDuration = 0.35f;
                foreach (dynamic particle in Particles)
                {
                    if (particle is Beam)
                    {
                        particle.Expired = true;
                    }
                }
                Boss.ActionCounter = 0;
                Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
            }

            if (Input.IsMouseButtonDown(MouseButton.Left) & Player.RangedCooldown == 0)
            {
                Particles.Add(new Bullet(
                    Position: Player.Position + Player.Direction * Player.Radius,
                    Velocity: 20f * RotateVector(Player.Direction, Random.Float(-2.5f, 2.5f), true),
                    Acceleration: Vector2.Zero,
                    ColorPalette: [new(0.25f, 0.25f, 1), new(0.5f, 0.5f, 1)],
                    Radius: 7.5f,
                    Lifetime: 5,
                    BaseDamage: 12,
                    IFrames: 3,
                    Homing: false,
                    IsFriendly: true
                    ));
                Player.RangedCooldown = 0.0625f;
            }
            if (Input.IsMouseButtonDown(MouseButton.Right) & Player.MeleeCooldown == 0)
            {
                Particles.Add(new SwordSwing(
                    Source: Player,
                    Width: 15,
                    ColorPalette: [new(0.5f, 0.5f, 1)],
                    Lifetime: 0.25f,
                    BaseDamage: 45,
                    IFrames: 20
                    ));
                Player.MeleeCooldown = 0.35f;
            }

            // Projectile Collision Detection and Resolution
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i] is Projectile)
                {
                    if (Particles[i] is Beam)
                    {

                        Vector2 beamDirection = Particles[i].Direction;
                        float[] tValues = [float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity];
                        if (beamDirection.X != 0)
                        {
                            tValues[0] = (150 - Particles[i].Position.X) / beamDirection.X;
                            tValues[1] = (1350 - Particles[i].Position.X) / beamDirection.X;
                        }
                        if (beamDirection.Y != 0)
                        {
                            tValues[2] = (900 - Particles[i].Position.Y) / beamDirection.Y;
                            tValues[3] = (-Particles[i].Position.Y) / beamDirection.Y;
                        }
                        for (int j = 0; j < tValues.Length; j++)
                        {
                            if (tValues[j] < 0)
                            {
                                tValues[j] = float.PositiveInfinity;
                            }
                        }
                        Vector2 borderIntersection = Particles[i].Position + MathF.Min(MathF.Min(tValues[0], tValues[1]), MathF.Min(tValues[2], tValues[3])) * beamDirection;

                        Particles.Add(new Dust(borderIntersection + Particles[i].Width * Random.PointOnCircle() + (Particles[i].Direction * 0.5f * Particles[i].Width), -Random.Float(4, 7) * Particles[i].Direction, Vector2.Zero, Particles[i].ColorPalette, 1, Particles[i].Width * 0.1f));
                    }
                    if (Particles[i].IsFriendly)
                    {
                        if (Particles[i].IsHittingTarget(Boss, 0.95f) & Boss.IFrames == 0 & !Boss.Invincible)
                        {
                            Boss.Damage(Particles[i].Damage, Particles[i].IFrames);
                            if (Particles[i] is Bullet)
                            {
                                int dustAmount = Random.Integer(3, 7);
                                for (int j = 0; j < dustAmount; j++)
                                {
                                    Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 4 + Random.PointInCircle() * 2, Vector2.Zero, Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
                                }
                                Particles.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f, 0, 0, false, true));
                                Particles[i].Expired = true;
                            }
                        }
                        foreach (Minion enemy in Enemies)
                        {
                            if (Particles[i].IsHittingTarget(enemy, 0.95f) & enemy.IFrames == 0)
                            {
                                enemy.Damage(Particles[i].Damage, Particles[i].IFrames);
                                if (Particles[i] is Bullet)
                                {
                                    int dustAmount = Random.Integer(3, 7);
                                    for (int j = 0; j < dustAmount; j++)
                                    {
                                        Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 4 + Random.PointInCircle() * 2, Vector2.Zero, Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
                                    }
                                    Particles.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f, 0, 0, false, true));
                                    Particles[i].Expired = true;

                                }
                            }
                        }
                    }
                    else
                    {
                        if (Particles[i].IsHittingTarget(Player, 0.95f) & Player.IFrames == 0)
                        {
                            Player.Damage(Particles[i].Damage, Particles[i].IFrames);
                            if (Particles[i] is Bullet)
                            {
                                int dustAmount = Random.Integer(3, 7);
                                for (int j = 0; j < dustAmount; j++)
                                {
                                    Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 4 + Random.PointInCircle() * 2, Vector2.Zero, Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
                                }
                                Particles.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f, 0, 0, false, true));
                                Particles[i].Expired = true;
                            }
                        }
                    }
                }
            }

            Boss.Update();
            Player.Update();

            if (Vector2.Distance(Player.Position, Boss.Position) <= Player.Radius + Boss.Radius & Player.IFrames == 0 & !Boss.Invincible)
            {
                Player.Damage(Boss.ContactDamage, 15);
            }

            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].CurrentHealth == 0) { Enemies.Remove(Enemies[i]); }
            }
            foreach (Minion enemy in Enemies)
            {
                switch (enemy.EnemyType)
                {
                    case "Basic":
                        {
                            if (Vector2.Distance(enemy.Position, Player.Position) <= enemy.AttackRange & enemy.AttackCooldown == 0 & enemy.Position.Y == Window.Height - enemy.Radius)
                            {
                                enemy.Velocity = 20 * new Vector2(MathF.Sign(Vector2.Normalize(Player.Position - enemy.Position).X), -1);
                                enemy.AttackCooldown = 1.25f;
                            }
                            enemy.Acceleration.X = 0.5f * Vector2.Normalize(Player.Position - enemy.Position).X;
                            break;
                        }
                    case "Flying":
                        {
                            if (Vector2.Distance(enemy.Position, Player.Position) <= 0.5f * enemy.AttackRange)
                            {
                                enemy.Acceleration = -0.75f * Vector2.Normalize(Player.Position - enemy.Position);
                            }
                            else if (Vector2.Distance(enemy.Position, Player.Position) <= enemy.AttackRange)
                            {
                                if (enemy.AttackCooldown == 0)
                                {
                                    Particles.Add(new Bullet(enemy.Position + enemy.Radius * 1.4f * Vector2.Normalize(Player.Position - enemy.Position), 5 * Vector2.Normalize(Player.Position - enemy.Position), Vector2.Zero, [new(1, 0, 0), new(0)], enemy.Radius * 0.4f, 4, enemy.AttackDamage, 5, false, false));
                                    enemy.AttackCooldown = 1.25f;
                                }
                            }
                            else if (Vector2.Distance(enemy.Position, Player.Position) <= 1.5f * enemy.AttackRange)
                            {
                                enemy.Acceleration = 0.5f * Vector2.Normalize(Player.Position - enemy.Position);
                            }
                            break;
                        }
                }
                enemy.Update();
                float overlap = enemy.Radius + Player.Radius - Vector2.Distance(enemy.Position, Player.Position);
                Vector2 directionToOther = Vector2.Normalize(Player.Position - enemy.Position);
                if (overlap > 0)
                {
                    enemy.Position -= directionToOther * overlap;
                    enemy.Position += directionToOther * overlap;
                    if (Player.IFrames == 0) { Player.Damage(enemy.ContactDamage, 20); }
                }
                foreach (Minion otherEnemy in Enemies)
                {
                    if (enemy == otherEnemy) { continue; }
                    overlap = enemy.Radius + otherEnemy.Radius - Vector2.Distance(enemy.Position, otherEnemy.Position);
                    directionToOther = Vector2.Normalize(otherEnemy.Position - enemy.Position);
                    if (overlap > 0)
                    {
                        enemy.Position -= directionToOther * overlap;
                        otherEnemy.Position += directionToOther * overlap;
                    }
                    else { continue; }
                }

            }

            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i] is Bullet)
                {
                    if (Particles[i].Homing)
                    {
                        Particles[i].CurveToTarget(Player);
                    }
                }
                Particles[i].Update();
                if (Particles[i].Expired) { Particles.Remove(Particles[i]); }
            }
            // Rendering Borders and Health Bars
            Draw.FillColor = Color.Black;
            Draw.LineSize = 0;
            Draw.Rectangle(0, 0, 150, Window.Height);
            Draw.Rectangle(Window.Width - 150, 0, 150, Window.Height);

            Draw.FillColor = Draw.LineColor = Color.DarkGray;
            Draw.LineSize = 2;
            Draw.Rectangle(30, 400, 50, 480);
            Draw.FillColor = new ColorF(1 - (float)Player.CurrentHealth / Player.MaxHealth, (float)Player.CurrentHealth / Player.MaxHealth, 0.25f);
            Draw.LineSize = 0;
            Draw.Rectangle(new(32, 402 + 476 * (1 - (float)Player.CurrentHealth / Player.MaxHealth)), new(46, 476 * (float)Player.CurrentHealth / Player.MaxHealth));

            // Rendering Boss Health Text
            Text.Size = 20;
            Text.Color = Color.Red;
            float textOffset;
            if (Boss.CurrentHealth > 0)
            {
                textOffset = 30 - 10 * MathF.Floor(MathF.Log10(Boss.CurrentHealth));
            }
            else
            {
                textOffset = 30;
            }
            Text.Draw($"{Boss.CurrentHealth}/{Boss.MaxHealth}", new(Window.Width - (125 - textOffset), Window.Height - 70));
            Draw.FillColor = Draw.LineColor = Color.DarkGray;
            Draw.LineSize = 2;
            Draw.Rectangle(1375, 100, 100, 700);
            Draw.FillColor = new ColorF(1, 0, 0);
            Draw.LineSize = 0;
            Draw.Rectangle(new(1377, 102 + 696 * (1 - (float)Boss.CurrentHealth / Boss.MaxHealth)), new(96, 696 * (float)Boss.CurrentHealth / Boss.MaxHealth));

            // Rendering Heal Cooldown Timer
            Draw.LineColor = Draw.FillColor = Color.DarkGray;
            Draw.LineSize = 2;
            Draw.Circle(new Vector2(110, 840), 20);
            Text.Color = new(0, 196, 0);
            Draw.FillColor = new(24, 128, 24);

            switch (Difficulty)
            {
                case 'E':
                    {
                        Draw.Arc(110, 840, 40, 40, 270, 270 + 360 * (Player.HealCooldown / 45));
                        Text.Draw("∞", new Vector2(100, 830));
                        break;
                    }
                case 'N' or 'H':
                    {
                        Draw.Arc(110, 840, 40, 40, 270, 270 + 360 * (Player.HealCooldown / 60));
                        Text.Draw($"{Player.HealsRemaining}", new Vector2(105, 830));
                        break;
                    }
            }

        }
    }
}
