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
            //Text.Draw($"Player Position: {Player.Position} \nPlayer Velocity: {Player.Velocity} \nPlayer Acceleration: {Player.Acceleration}", new(150, 6 * Text.Size));
            switch (Boss.CurrentPhase)
            {
                // Phase 1 Logic
                case 1:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Phase Change":
                                {
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Boss.ActionCounter += 1;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase += 1;
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
                                        Boss.ActionCounter -= 1;
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
                                    Boss.Position = Boss.TargetPosition + new Vector2(130 * MathF.Cos(3 * Boss.AnimationPercent * MathF.Tau), 80 * MathF.Sin(4 * Boss.AnimationPercent * MathF.Tau));
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
                                            Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f) + Random.Direction() * 2;
                                        }
                                        else
                                        {
                                            Boss.Velocity = 30 * Vector2.Normalize(Player.Position - Boss.Position);
                                            Boss.ActionCounter -= 1;
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
                                            Boss.ActionCounter -= 1;

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
                                            Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.25f, 7.5f, Boss.Radius, Boss.Radius * 3, 0, 0, false, true));
                                            Boss.ActionCounter += 1;
                                        }
                                        if (Boss.AnimationPercent == 1)
                                        {
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
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Boss.ActionCounter += 1;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase += 1;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.NextAction = "Shoot Bullet Fan";
                                        Boss.CurrentAction = "Idle";
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
                                            Vector2[] _ = Boss.GetBulletFanVectors(Vector2.Normalize(Player.Position - Boss.Position), 100 - 10 * Boss.ActionCounter, 15 - 2 * Boss.ActionCounter);
                                            for (int i = 0; i < _.Length; i++)
                                            {
                                                Particles.Add(new Bullet(
                                                    Position: Boss.Position + Boss.Radius * _[i],
                                                    Velocity: 8 * _[i],
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
                                            Boss.ActionTimer = Boss.ActionDuration = 1.25f;
                                            Boss.ActionCounter -= 1;
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
                                            Boss.ActionCounter -= 1;
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
                                            Particles.Add(new Beam(Boss, [new(1, 0, 0), new(0)], 2f, Vector2.UnitY, Boss.Radius * 1.25f, 20, 15, false));
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
                                            Boss.ActionCounter -= 1; 
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
                                    Window.ClearBackground(new Color(255, 0, 0, 8));
                                    Boss.Position = Boss.TargetPosition + Random.Direction() * 3;
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Boss.ActionCounter += 1;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase += 1;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
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
                            case "Flying":
                                {
                                    break;
                                }
                            case "Summon":
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
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1f;
                                        }

                                    }
                                    break;
                                }
                            case "Idle":
                                {
                                    Boss.CurrentAction = Boss.NextAction;
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        switch (Boss.NextAction)
                                        {
                                            case "Charge":
                                                {
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    break;
                                                }
                                            case "Flying":
                                                {
                                                    break;
                                                }
                                            case "Summon":
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
                // Phase 4 Logic
                case 4:
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Defeat":
                                {
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
                                    Boss.CurrentAction = Boss.NextAction;
                                    if (Boss.AnimationPercent == 1)
                                    {
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
                Boss.ActionCounter = 0;
                Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
            }

            if (Input.IsMouseButtonDown(MouseButton.Left) & Player.RangedCooldown == 0)
            {
                Particles.Add(new Bullet(
                    Position: Player.Position + Player.Direction * Player.Radius,
                    Velocity: 20f * Player.Direction,
                    Acceleration: Vector2.Zero,
                    ColorPalette: [new(0, 0, 1), new(0.5f, 0.5f, 1)],
                    Radius: 7.5f,
                    Lifetime: 5,
                    BaseDamage: 75,
                    IFrames: 3,
                    Homing: false,
                    IsFriendly: true
                    ));
                Player.RangedCooldown = 0.125f;
            }
            if (Input.IsMouseButtonDown(MouseButton.Right) & Player.MeleeCooldown == 0)
            {
                Particles.Add(new SwordSwing(
                    Source: Player,
                    Width: 15,
                    ColorPalette: [new(0.5f, 0.5f, 1)],
                    Lifetime: 0.25f,
                    BaseDamage: 135,
                    IFrames: 20
                    ));
                Player.MeleeCooldown = 0.35f;
            }
            
            // Projectile Collision Detection and Resolution
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i] is Projectile)
                    {
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
                                    enemy.AttackCooldown = 3.5f;
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
