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
        private readonly bool debug = true;
        private float ActionSpeedMultiplier;
        public char Difficulty = 'H';
        public List<dynamic> Particles = [];
        public List<Minion> Enemies = [];
        public Boss Boss = new();
        public Player Player = new();

        // Audio
        Sound playerHurt = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Hit_1.wav");
        Sound playerBulletShoot = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Item_39.wav");
        Sound playerBulletImpact = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Killed_3.wav");
        Sound playerSwordSwing = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Item_152.wav");
        Sound slashWarning = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Avatar Of Emptiness Warp.mp3");
        Sound bossBulletShoot = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Item_42.wav");
        Sound beamCharge = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Hyper Goner Charge.mp3");
        Sound bossRoar = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Roar.wav");
        Sound bossDeath = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Killed_23.wav");
        Sound bossHurt = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Hit_42.wav");
        Sound minionDeath = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Killed_52.wav");
        Sound minionHurt = Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/NPC_Hit_18.wav");


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
            Window.TargetFPS = 60;
            Window.SetSize(1500, 900);
            ActionSpeedMultiplier = Difficulty switch
            {
                'E' => 0.75f,
                'N' => 1,
                'H' => 1.25f,
                _ => 1
            };
            Boss.Setup();
            Player.Setup(Difficulty);
            Console.Clear();
            string CurrentDirectory = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine(CurrentDirectory);


        }

        /// <summary>
        ///     Update runs every frame.
        /// </summary>
        public void Update()
        {
            Window.ClearBackground(96);
            Text.Color = Color.Black;
            Text.Size = 20;
            //Text.Draw($"Current Action: {Boss.CurrentAction} (Phase {Boss.CurrentPhase}) \nNext Action: {Boss.NextAction} \nCounter: {Boss.ActionCounter} \nTimer: {Boss.ActionTimer} \nAnimation {float.Round(Boss.AnimationPercent * 100)}% Complete \nPosition: {Boss.Position}", new Vector2(150, 0));
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
                                        Audio.Play(bossRoar);
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
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Shoot Bullets":
                                {
                                    Boss.Position = Boss.TargetPosition + new Vector2(150 * MathF.Sin(Boss.AnimationPercent * MathF.Tau), 0);
                                    Boss.Direction = Vector2.Normalize(Player.Position - Boss.Position);
                                    if (Boss.AttackCooldown == 0)
                                    {
                                        Audio.Play(bossBulletShoot);
                                        Particles.Add(new Bullet(
                                            Position: Boss.Position + Boss.Radius * Boss.Direction,
                                            Velocity: 12.5f * Boss.Direction * ActionSpeedMultiplier,
                                            Acceleration: Vector2.Zero,
                                            ColorPalette: [new(1, 0, 0), new(0)],
                                            Radius: 20,
                                            Lifetime: 5,
                                            BaseDamage: 7,
                                            IFrames: 5,
                                            Homing: true,
                                            IsFriendly: false
                                            ));
                                        Boss.AttackCooldown = Boss.ActionDuration / Boss.ActionCounter;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, Enemies.Count, "Basic"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, Enemies.Count, "Basic"));
                                    Boss.ActionDuration = Boss.ActionTimer = 0.25f / ActionSpeedMultiplier;
                                    Boss.CurrentAction = "Idle";
                                    break;
                                }
                            case "Flying":
                                {
                                    Boss.Position = Boss.TargetPosition + new Vector2(300 * MathF.Sin(Boss.AnimationPercent * MathF.Tau), 100 * MathF.Sin(2 * Boss.AnimationPercent * MathF.Tau));
                                    if (Boss.ActionTimer == 0)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                            Audio.Play(Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Deerclops_rubble_attack.wav"));
                                            Boss.Position.Y = Window.Height - Boss.Radius;
                                            Particles.Add(new Shockwave(Boss.Position + new Vector2(0, Boss.Radius), [new(1f)], 0.25f, 10, 40, 200, (int)(Boss.ContactDamage * 1.5f), 20, true, false));
                                            int dustAmount = Random.Integer(10, 18);
                                            for (int i = 0; i < dustAmount; i++)
                                            {
                                                Particles.Add(new Dust(Boss.Position + new Vector2(Random.Float(-Boss.Radius, Boss.Radius), Boss.Radius), 8 * Random.PointInCircle() + new Vector2(0, -7.5f), new Vector2(0, 20), [new(0)], Random.Float(1, 3), Random.Float(3, 6)));
                                            }
                                            Boss.Velocity = Vector2.Zero;
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.Direction = Vector2.UnitY;
                                            Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            if (Boss.ActionCounter == 1)
                                            {
                                                Boss.NextAction = "Shoot Bullets";
                                                Boss.ActionTimer = Boss.ActionDuration = 1.5f / ActionSpeedMultiplier;
                                            }
                                            else
                                            {
                                                Boss.NextAction = "Dash";
                                                Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                        Boss.Alpha = easedAnimationPercent;
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1.75f;
                                            Boss.ActionCounter = 0;
                                            Boss.TargetPosition = new(Window.Width * 0.5f, Window.Height * 0.25f);
                                        }
                                    }
                                    else
                                    {
                                        if (Boss.ActionCounter == 0) { Audio.Play(bossRoar); }
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
                                            Boss.LockToScreen = true;
                                            Boss.Direction = Vector2.UnitY;
                                            Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            Boss.CurrentAction = "Idle";
                                            Boss.ActionDuration = Boss.ActionTimer = 0.5f / ActionSpeedMultiplier;
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
                                            Boss.ActionDuration = Boss.ActionTimer = 7.5f / ActionSpeedMultiplier;
                                            Boss.ActionCounter = 20;
                                        }
                                        else if (Boss.NextAction == "Dash")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 0.25f / ActionSpeedMultiplier;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1.75f;
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
                                                    Boss.ActionDuration = Boss.ActionTimer = 7.5f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = Difficulty switch { 'E' => 15, 'N' => 20, 'H' => 25, _ => 20 };
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
                                                    Boss.ActionDuration = Boss.ActionTimer = Random.Float(3f, 6f) * ActionSpeedMultiplier;
                                                    Boss.NextAction = "Dash";
                                                    break;
                                                }
                                            case "Dash":
                                                {
                                                    Boss.ActionCounter = 6;
                                                    Boss.ActionDuration = Boss.ActionTimer = 0.25f / ActionSpeedMultiplier;
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
                                        Audio.Play(bossRoar);
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
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Shoot Bullet Fan":
                                {
                                    if (Boss.AttackCooldown == 0)
                                    {
                                        Boss.Direction = Vector2.Normalize(Player.Position - Boss.Position);
                                        Vector2[] _ = Difficulty switch { 'E' => Boss.GetBulletFanVectors(Boss.Direction, 75, 7), 'H' => Boss.GetBulletFanVectors(Boss.Direction, 90, 13), 'N' or _ => Boss.GetBulletFanVectors(Boss.Direction, 75, 7) };
                                        for (int i = 0; i < _.Length; i++)
                                        {
                                            Audio.Play(bossBulletShoot);
                                            Particles.Add(new Bullet(
                                                Position: Boss.Position + Boss.Radius * _[i],
                                                Velocity: 10f * _[i] * ActionSpeedMultiplier,
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
                                        Boss.AttackCooldown = Boss.ActionDuration / Boss.ActionCounter;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, Enemies.Count, "Flying"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, Enemies.Count, "Flying"));
                                    Enemies.Add(new Minion(Boss.Position + new Vector2(0, 190), new Vector2(0, 20), Vector2.Zero, Enemies.Count, "Basic"));
                                    Boss.ActionDuration = Boss.ActionTimer = 0.75f / ActionSpeedMultiplier;
                                    Boss.CurrentAction = "Idle";
                                    break;
                                }
                            case "Beam":
                                {

                                    if (Boss.ActionCounter % 2 == 0)
                                    {
                                        if (!Audio.IsPlaying(beamCharge)) { Audio.Play(beamCharge); }
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.ActionCounter--;
                                            Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                            Audio.Stop(beamCharge);
                                            Boss.TargetPosition = Boss.Position + Boss.Radius * Vector2.UnitY;
                                            Boss.ActionTimer = Boss.ActionDuration = 2f / ActionSpeedMultiplier;
                                            Particles.Add(new Beam(Boss, Boss.Direction, true, [new(1, 0, 0), new(0.15f, 0, 0)], Boss.ActionDuration, Boss.Radius * 1.25f, 1, 1, false));
                                            Boss.Velocity = Vector2.Zero;
                                            Boss.CurrentAction = "Zooming";
                                            Boss.ZoomStartPosition = Boss.Position;
                                            Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);

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
                                            Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
                                        }
                                        if (Boss.NextAction == "Idle")
                                        {
                                            Boss.NextAction = "Shoot Bullet Fan";
                                            Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1.75f;
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
                                                    Boss.ActionCounter = Difficulty switch { 'E' => 3, 'N' => 3, 'H' => 4, _ => 3 };
                                                    Boss.ActionTimer = Boss.ActionDuration = 1.5f / ActionSpeedMultiplier;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Beam";
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                        Audio.Play(bossRoar);
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 20f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, false));
                                        Boss.ActionCounter++;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentPhase++;
                                        Boss.Invincible = false;
                                        Boss.Velocity = Vector2.Zero;
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.TargetPosition = Boss.Position;
                                        Boss.NextAction = "Spin";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Charge":
                                {
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Audio.Play(beamCharge);
                                        Particles.Add(new Shockwave(Boss.Position, [new(1, 0, 0)], Boss.ActionDuration, Boss.Radius / 3, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, 0, false, false));
                                        Boss.ActionCounter++;
                                    }
                                    Vector2 randomDirection = Random.Direction();
                                    float randomLifetime = Random.Float(0.5f, MathF.Min(3, Boss.ActionTimer));
                                    float distanceFromCenter = Boss.Radius * Random.Float(2.75f, 5.5f);
                                    Particles.Add(new Dust(Boss.Position + randomDirection * distanceFromCenter, -randomDirection * distanceFromCenter / randomLifetime * Time.DeltaTime, Vector2.Zero, [new(1, 0, 0)], randomLifetime, Random.Float(8, 12)));
                                    Boss.TargetPosition = Player.Position;
                                    Boss.Direction = Vector2.Normalize(Boss.TargetPosition - Boss.Position);
                                    Draw.LineColor = new ColorF(0.75f, 0, 0, 0.5f * Boss.AnimationPercent);
                                    if (MathF.Floor(Time.SecondsElapsed * 4) % 2 == 0)
                                    {
                                        Draw.LineColor = new ColorF(0.75f, 0.5f * Boss.AnimationPercent);
                                    }
                                    Draw.LineSize = Boss.Radius * 8.5f * Boss.AnimationPercent;
                                    Draw.LineSharp(Boss.Position + Boss.Direction * Boss.Radius, Boss.TargetPosition + Vector2.Distance(Vector2.Zero, Window.Size) * Vector2.Normalize(Boss.TargetPosition - Boss.Position));
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.NextAction = "Beam";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.75f / ActionSpeedMultiplier;
                                        Boss.ActionCounter = 0;
                                    }
                                    break;
                                }
                            case "Beam":
                                {
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Boss.ActionCounter++;
                                        Particles.Add(new Beam(Boss, Boss.Direction, true, [new(1, 0, 0), new(0.15f, 0, 0)], Boss.ActionDuration, Boss.Radius * 4.25f, 4, 1, false));
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.NextAction = "Flying";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                        Vector2[] _ = Boss.GetBulletFanVectors(Vector2.UnitY, 45, Difficulty switch { 'E' => 3, 'N' => 5, 'H' => 7, _ => 5 });
                                        for (int i = 0; i < _.Length; i++)
                                        {
                                            Audio.Play(bossBulletShoot);
                                            Particles.Add(new Bullet(
                                                Position: Boss.Position + Boss.Radius * _[i],
                                                Velocity: 10f * _[i] * ActionSpeedMultiplier,
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
                                        Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    if (Boss.ActionCounter == 1)
                                    {
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(120, 150), new Vector2(10, 20), Vector2.Zero, Enemies.Count, "Basic"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(-120, 150), new Vector2(-10, 20), Vector2.Zero, Enemies.Count, "Basic"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(80, 170), new Vector2(7.5f, 20), Vector2.Zero, Enemies.Count, "Flying"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(-80, 170), new Vector2(-7.5f, 20), Vector2.Zero, Enemies.Count, "Flying"));
                                        Enemies.Add(new Minion(Boss.Position + new Vector2(0, 190), new Vector2(0, 20), Vector2.Zero, Enemies.Count, "Flying"));
                                        Boss.ActionCounter--;
                                        Boss.Invincible = true;
                                    }
                                    if (Enemies.Count == 0)
                                    {
                                        Boss.Invincible = false;
                                        Boss.ActionDuration = Boss.ActionTimer = 0.75f / ActionSpeedMultiplier;
                                        Boss.NextAction = "Charge";
                                        Boss.CurrentAction = "Idle";
                                    }
                                    else
                                    {
                                        Boss.Position = Boss.TargetPosition + new Vector2(350 * MathF.Sin(0.125f * (Time.SecondsElapsed - Boss.ActionStartTime) * MathF.Tau), 100 * MathF.Sin(0.25f * (Time.SecondsElapsed - Boss.ActionStartTime) * MathF.Tau));
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
                                            Boss.ActionTimer = Boss.ActionDuration = 0.75f / ActionSpeedMultiplier;
                                        }
                                        else if (Boss.NextAction == "Phase Change")
                                        {
                                            Boss.ActionDuration = Boss.ActionTimer = 1.75f;
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
                                        Draw.LineColor = new ColorF(0.75f, 0, 0, 0.5f);
                                        if (MathF.Floor(Time.SecondsElapsed * 12) % 2 == 0)
                                        {
                                            Draw.LineColor = new ColorF(0.75f, 0.5f);
                                        }
                                        Draw.LineSize = Boss.Radius * 8.5f;
                                        Draw.LineSharp(Boss.Position + Boss.Direction * Boss.Radius, Boss.TargetPosition + Vector2.Distance(Vector2.Zero, Window.Size) * Vector2.Normalize(Boss.TargetPosition - Boss.Position));
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Audio.Stop(beamCharge);
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {
                                            case "Charge":
                                                {
                                                    Boss.NextAction = "Beam";
                                                    Boss.ActionTimer = Boss.ActionDuration = 2f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    Boss.NextAction = "Flying";
                                                    Boss.ActionTimer = Boss.ActionDuration = 2f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Flying":
                                                {
                                                    Boss.NextAction = "Summon";
                                                    Boss.TargetPosition = Boss.Position;
                                                    Boss.ActionTimer = Boss.ActionDuration = 5f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Charge";
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
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
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Audio.Play(bossRoar);
                                        Audio.Play(bossDeath);
                                        foreach (dynamic _ in Particles) { _.Expired = true; }
                                        foreach (Minion _ in Enemies) { _.CurrentHealth = 0; }
                                        Boss.Alpha = 0;
                                        for (int i = 0; i < 100; i++)
                                        {
                                            Particles.Add(new Dust(Boss.Position, Random.PointInCircle() * 12.5f, new Vector2(0, Random.Float(12.5f, 17.5f)), [new(Random.Float(0.25f, 0.8f), 0, 0)], Random.Float(1, 3), Boss.Radius / Random.Float(3, 5)));
                                        }
                                        Particles.Add(new Shockwave(Boss.Position, [new(0.6f, 0.2f, 0.2f, 0.5f)], 1, Boss.Radius, 0, Vector2.Distance(new(), Window.Size) / 2, 0, 0, false, false));
                                        Particles.Add(new Shockwave(Boss.Position, [new(0.6f, 0.2f, 0.2f, 0.5f)], 1.5f, Boss.Radius / 1.5f, 0, Vector2.Distance(new(), Window.Size) / 2, 0, 0, false, false));
                                        Particles.Add(new Shockwave(Boss.Position, [new(0.6f, 0.2f, 0.2f, 0.5f)], 2, Boss.Radius / 2, 0, Vector2.Distance(new(), Window.Size) / 2, 0, 0, false, false));
                                        Boss.ActionCounter++;
                                    }
                                    Boss.Direction = -Vector2.UnitY;
                                    break;
                                }
                            case "Spin":
                                {
                                    Boss.Position = Boss.TargetPosition + 3 * Random.Direction();
                                    Boss.Direction = RotateVector(-Vector2.UnitY, Boss.AnimationPercent * 20 * MathF.Tau, false);
                                    if (Boss.AttackCooldown == 0)
                                    {
                                        Vector2 randomDirection = Random.Direction();
                                        float _ = Random.Float(7.5f, 12.5f);
                                        Audio.Play(bossBulletShoot);
                                        Particles.Add(new Bullet(
                                            Position: Boss.Position + Boss.Radius * 1.25f * randomDirection,
                                            Velocity: _ * 0.7f * randomDirection * ActionSpeedMultiplier,
                                            Acceleration: Vector2.UnitY * 0.75f * _,
                                            ColorPalette: [new(1, 0, 0), new(0)],
                                            Radius: 20 - _,
                                            Lifetime: 5,
                                            BaseDamage: 12,
                                            IFrames: 6,
                                            Homing: false,
                                            IsFriendly: false
                                            ));
                                        Boss.AttackCooldown = Boss.ActionDuration / 500;
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionCounter = -1;
                                        Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
                                    }
                                    break;
                                }
                            case "Slash":
                                {
                                    Audio.Stop(Audio.LoadSound("../../../../langdon-spencer-a4-game-project/Sound Effects/Asriel Beam Hold.mp3"));
                                    Boss.Invincible = true;
                                    switch (Boss.ActionCounter)
                                    {
                                        case 10:
                                            {
                                                float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                                Boss.Position = new Vector2(Window.Width / 2, (easedAnimationPercent * (Window.Height * 0.25f)) + ((1 - easedAnimationPercent) * (Window.Height * -0.5f)));
                                                Boss.Alpha = easedAnimationPercent;
                                                if (Boss.AnimationPercent == 1)
                                                {
                                                    Boss.Invincible = false;
                                                    Boss.LockToScreen = true;
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.75f / ActionSpeedMultiplier;
                                                    Boss.CurrentAction = "Idle";
                                                    Boss.ActionCounter = 0;
                                                }
                                                break;
                                            }
                                        case -1:
                                            {
                                                Boss.LockToScreen = false;
                                                float easedAnimationPercent = Boss.InOutSineEasing(Boss.AnimationPercent);
                                                Boss.Position = new Vector2(Window.Width / 2, (easedAnimationPercent * (Window.Height * -0.5f)) + ((1 - easedAnimationPercent) * (Window.Height * 0.25f)));
                                                Boss.Alpha = 1 - easedAnimationPercent;

                                                if (Boss.AnimationPercent == 1)
                                                {
                                                    Boss.ActionCounter++;
                                                    Boss.ActionTimer = Boss.ActionDuration;
                                                }
                                                break;
                                            }
                                        case int c when c % 2 == 0:
                                            {
                                                if (Boss.Direction == -Vector2.UnitY)
                                                {
                                                    Audio.Play(slashWarning);
                                                    Boss.Direction = RotateVector(Vector2.UnitX, Random.Float(-15, 15), true);
                                                }
                                                Boss.LockToScreen = false;
                                                Boss.Alpha = 1;
                                                Draw.LineColor = new ColorF(0.75f, 0, 0, 0.5f);
                                                if (MathF.Floor(Time.SecondsElapsed * 12) % 2 == 0)
                                                {
                                                    Draw.LineColor = new ColorF(0.75f, 0.5f);
                                                }
                                                Draw.LineSize = Boss.Radius * 2;
                                                Draw.LineSharp(new Vector2(Window.Width * 0.5f, Window.Height * 0.5f) - Boss.Direction * Window.Width, new Vector2(Window.Width * 0.5f, Window.Height * 0.5f) + Boss.Direction * Window.Width);

                                                if (Boss.AnimationPercent == 1)
                                                {
                                                    Boss.ActionCounter++;
                                                    Boss.ActionTimer = Boss.ActionDuration;
                                                }
                                                break;
                                            }
                                        case int c when c % 2 == 1:
                                            {
                                                if (Boss.Direction != -Vector2.UnitY)
                                                {
                                                    for (int i = -1; i <= 1; i += 2)
                                                    {
                                                        for (int j = 0; j < Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 }; j++)
                                                        {
                                                            Particles.Add(new Bullet(
                                                                Vector2.Lerp(new Vector2(Window.Width * 0.5f, Window.Height * 0.5f) - Boss.Direction * (Window.Width * 0.5f - 150), new Vector2(Window.Width * 0.5f, Window.Height * 0.5f) + Boss.Direction * (Window.Width * 0.5f - 150), (0.5f + j) / Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 }) + Boss.Radius * RotateVector(Boss.Direction, 90 * i, true),
                                                                0.01f * RotateVector(Boss.Direction, 90 * i, true),
                                                                25 * RotateVector(Boss.Direction, 90 * i, true),
                                                                [new(1, 0, 0), new(0)],
                                                                12.5f / ActionSpeedMultiplier,
                                                                2,
                                                                15,
                                                                5,
                                                                false,
                                                                false
                                                                ));
                                                        }
                                                    }
                                                    Particles.Add(new Beam(new Vector2(Window.Width * 0.5f, Window.Height * 0.5f) - Boss.Direction * Window.Width, Boss.Direction, [new(1, 0, 0), new(0.15f, 0, 0)], Boss.ActionDuration * 0.8f, Boss.Radius, 0, 0, false));
                                                    Boss.Direction = -Vector2.UnitY;
                                                }

                                                if (Boss.AnimationPercent == 1)
                                                {
                                                    Boss.ActionCounter++;
                                                    Boss.ActionTimer = Boss.ActionDuration;
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    if (Boss.ActionCounter == -1)
                                    {
                                        Boss.Invincible = true;
                                        Boss.ActionStartTime = Time.SecondsElapsed;
                                        Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 12.5f, Boss.Radius + 20, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, false, true));
                                        for (int i = 0; i < Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 }; i++) { Enemies.Add(new Minion(Boss.Position + (Boss.Radius + 60) * RotateVector(Vector2.UnitX, 360f / Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 } * i, true), Vector2.Zero, Vector2.Zero, i, "Orbiting")); }
                                        Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.ActionCounter++;
                                    }
                                    if (Enemies.Count == 0)
                                    {
                                        Boss.Invincible = false;
                                        Boss.ZoomStartPosition = Boss.Position;
                                        Boss.CurrentAction = "Zooming";
                                        Boss.NextAction = "Charge";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
                                    }
                                    else
                                    {
                                        Boss.Position = Boss.TargetPosition + new Vector2(350 * MathF.Sin(0.125f * (Time.SecondsElapsed - Boss.ActionStartTime) * MathF.Tau), 100 * MathF.Sin(0.25f * (Time.SecondsElapsed - Boss.ActionStartTime) * MathF.Tau));
                                    }
                                    break;
                                }
                            case "Charge":
                                {
                                    if (!Audio.IsPlaying(beamCharge))
                                    {
                                        Audio.Play(beamCharge);
                                        Particles.Add(new Shockwave(Boss.Position, [new(1, 0, 0)], Boss.ActionDuration, Boss.Radius / 3, Vector2.Distance(Vector2.Zero, Window.Size) / 2, 0, 0, 0, false, false));
                                    }
                                    Vector2 randomDirection = Random.Direction();
                                    float randomLifetime = Random.Float(0.5f, MathF.Min(3, Boss.ActionTimer));
                                    float distanceFromCenter = Boss.Radius * Random.Float(2.75f, 5.5f);
                                    Particles.Add(new Dust(Boss.Position + randomDirection * distanceFromCenter, -randomDirection * distanceFromCenter / randomLifetime * Time.DeltaTime, Vector2.Zero, [new(1, 0, 0)], randomLifetime, Random.Float(8, 12)));
                                    Boss.TargetPosition = Player.Position;
                                    Boss.Direction = Vector2.Normalize(Boss.TargetPosition - Boss.Position);
                                    Draw.LineColor = new ColorF(0.75f, 0, 0, 0.5f * Boss.AnimationPercent);
                                    if (MathF.Floor(Time.SecondsElapsed * 4) % 2 == 0)
                                    {
                                        Draw.LineColor = new ColorF(0.75f, 0.5f * Boss.AnimationPercent);
                                    }
                                    Draw.LineSize = Boss.Radius * 2f * Boss.AnimationPercent;
                                    Vector2[] _ = Boss.GetBulletFanVectors(Boss.Direction, Difficulty switch { 'E' => 95, 'N' => 120, 'H' => 145, _ => 5 }, Difficulty switch { 'E' => 3, 'N' => 5, 'H' => 9, _ => 5 });
                                    for (int i = 0; i < _.Length; i++)
                                    {
                                        Draw.LineSharp(Boss.Position + _[i] * Boss.Radius, Boss.Position + _[i] * Vector2.Distance(Vector2.Zero, Window.Size));
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.NextAction = "Beam";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.75f / ActionSpeedMultiplier;
                                        Boss.ActionCounter = 0;
                                    }
                                    break;
                                }
                            case "Beam":
                                {
                                    if (Boss.ActionCounter == 0)
                                    {
                                        Boss.ActionCounter++;
                                        Vector2[] _ = Boss.GetBulletFanVectors(Boss.Direction, Difficulty switch { 'E' => 95, 'N' => 120, 'H' => 145, _ => 5 }, Difficulty switch { 'E' => 3, 'N' => 5, 'H' => 9, _ => 5 });
                                        for (int i = 0; i < _.Length; i++)
                                        {
                                            Particles.Add(new Beam(Boss, _[i], true, [new(1, 0, 0), new(0.15f, 0, 0)], Boss.ActionDuration, Boss.Radius, 8, 1, false));
                                        }
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                        Boss.NextAction = "Spin";
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionTimer = Boss.ActionDuration = 0.25f / ActionSpeedMultiplier;
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
                                        if (Boss.NextAction == "Charge")
                                        {
                                            Boss.NextAction = "Beam";
                                            Boss.ActionTimer = Boss.ActionDuration = 3f / ActionSpeedMultiplier;
                                            Boss.ActionCounter = 1;
                                            break;
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
                                        Draw.LineColor = new ColorF(0.75f, 0, 0, 0.5f);
                                        if (MathF.Floor(Time.SecondsElapsed * 12) % 2 == 0)
                                        {
                                            Draw.LineColor = new ColorF(0.75f, 0.5f);
                                        }
                                        Draw.LineSize = Boss.Radius * 2f;
                                        Vector2[] _ = Boss.GetBulletFanVectors(Boss.Direction, Difficulty switch { 'E' => 95, 'N' => 120, 'H' => 145, _ => 5 }, Difficulty switch { 'E' => 3, 'N' => 5, 'H' => 9, _ => 5 });
                                        for (int i = 0; i < _.Length; i++)
                                        {
                                            Draw.LineSharp(Boss.Position + _[i] * Boss.Radius, Boss.Position + _[i] * Vector2.Distance(Vector2.Zero, Window.Size));
                                        }
                                    }
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        Audio.Stop(beamCharge);
                                        Boss.CurrentAction = Boss.NextAction;
                                        switch (Boss.NextAction)
                                        {
                                            case "Spin":
                                                {
                                                    Boss.NextAction = "Slash";
                                                    Boss.ActionTimer = Boss.ActionDuration = 5f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Slash":
                                                {
                                                    Boss.NextAction = "Summon";
                                                    Boss.ActionTimer = Boss.ActionDuration = 0.5f / ActionSpeedMultiplier;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Charge";
                                                    Boss.ActionCounter = -1;
                                                    break;
                                                }
                                            case "Charge":
                                                {
                                                    Boss.NextAction = "Beam";
                                                    Boss.ActionTimer = Boss.ActionDuration = 2.5f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
                                                    break;
                                                }
                                            case "Beam":
                                                {
                                                    Boss.NextAction = "Spin";
                                                    Boss.ActionTimer = Boss.ActionDuration = 1.25f / ActionSpeedMultiplier;
                                                    Boss.ActionCounter = 0;
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
            else if (Boss.CurrentHealth <= (int)(Boss.MaxHealth * (1 - 0.25f * Boss.CurrentPhase)) & !(Boss.CurrentAction == "Phase Change" | Boss.NextAction == "Phase Change") & !(Boss.CurrentAction == "Defeat" | Boss.NextAction == "Defeat"))
            {
                Boss.CurrentAction = "Zooming";
                Boss.ZoomStartPosition = Boss.Position;
                Boss.Invincible = true;
                Boss.NextAction = "Phase Change";
                Boss.ActionTimer = Boss.ActionDuration = 0.35f / ActionSpeedMultiplier;
                foreach (dynamic particle in Particles)
                {
                    if (particle is Beam)
                    {
                        particle.Expired = true;
                    }
                }
                Audio.Stop(beamCharge);
                Boss.ActionCounter = 0;
                Boss.TargetPosition = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
            }
            int damageMultiplier = 1;
            float attackSpeedMultiplier = 1;
            if (debug)
            {
                damageMultiplier = 50;
                attackSpeedMultiplier = 4f;
                if (Input.IsKeyboardKeyPressed(KeyboardKey.Backslash))
                {
                    Boss.Alpha = 1;
                    Boss.CurrentHealth = (int)(Boss.MaxHealth * (1 - 0.25f * Boss.CurrentPhase));
                }
            }
            if (Input.IsMouseButtonDown(MouseButton.Left) & Player.RangedCooldown == 0 & Boss.CurrentAction != "Defeat")
            {
                Audio.Play(playerBulletShoot);
                Particles.Add(new Bullet(
                    Position: Player.Position + Player.Direction * Player.Radius,
                    Velocity: 20f * Vector2.Normalize(RotateVector(Input.GetMousePosition() - Player.Position, Random.Float(-2.5f, 2.5f), true)),
                    Acceleration: Vector2.Zero,
                    ColorPalette: [new(0.25f, 0.25f, 1), new(0.5f, 0.5f, 1)],
                    Radius: 7.5f,
                    Lifetime: 5,
                    BaseDamage: 18 * damageMultiplier,
                    IFrames: 3,
                    Homing: false,
                    IsFriendly: true
                    ));
                Player.RangedCooldown = 0.125f / attackSpeedMultiplier;
            }
            if (Input.IsMouseButtonDown(MouseButton.Right) & Player.MeleeCooldown == 0 & Boss.CurrentAction != "Defeat")
            {
                Audio.Play(playerSwordSwing);
                Particles.Add(new SwordSwing(
                    Source: Player,
                    Width: 15,
                    Lifetime: 0.25f,
                    BaseDamage: 35 * damageMultiplier,
                    IFrames: 20
                    ));
                Player.MeleeCooldown = 0.275f;
            }


            // Projectile Collision Detection/Resolution
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i] is Projectile)
                {
                    if (Particles[i] is Beam)
                    {
                        if (Boss.CurrentAction != "Slash")
                        {
                            Particles.Add(new Dust(Particles[i].GetPointAlongBorder(), -Random.Float(4, 7) * Particles[i].Direction, Vector2.Zero, Particles[i].ColorPalette, 1, Particles[i].Width * 0.1f));
                        }
                    }
                    if (Particles[i].Damage > 0)
                    {
                        if (Particles[i].IsFriendly)
                        {
                            if (Particles[i].IsHittingTarget(Boss, 0.95f) & Boss.IFrames == 0 & !Boss.Invincible)
                            {
                                Audio.Play(bossHurt);
                                Boss.Damage(Particles[i].Damage, Particles[i].IFrames);
                                if (Particles[i] is Bullet)
                                {
                                    Audio.Play(playerBulletImpact);
                                    int dustAmount = Random.Integer(3, 7);
                                    for (int j = 0; j < dustAmount; j++)
                                    {
                                        Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 6 + Random.PointInCircle() * 4, new Vector2(0, 20), Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
                                    }
                                    Particles.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f, 0, 0, false, true));
                                    Particles[i].Expired = true;
                                }
                            }
                            foreach (Minion enemy in Enemies)
                            {
                                if (Particles[i].IsHittingTarget(enemy, 0.95f) & enemy.IFrames == 0)
                                {
                                    Audio.Play(minionHurt);
                                    enemy.Damage(Particles[i].Damage, Particles[i].IFrames);
                                    if (Particles[i] is Bullet)
                                    {
                                        Audio.Play(playerBulletImpact);
                                        int dustAmount = Random.Integer(3, 7);
                                        for (int j = 0; j < dustAmount; j++)
                                        {
                                            Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 6 + Random.PointInCircle() * 4, new Vector2(0, 20), Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
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
                                Audio.Play(playerHurt);
                                Player.Damage(Particles[i].Damage, Particles[i].IFrames);
                                if (Particles[i] is Bullet)
                                {
                                    int dustAmount = Random.Integer(3, 7);
                                    for (int j = 0; j < dustAmount; j++)
                                    {
                                        Particles.Add(new Dust(Particles[i].Position, -Vector2.Normalize(Particles[i].Velocity) * 6 + Random.PointInCircle() * 4, new Vector2(0, 20), Particles[i].ColorPalette, Random.Float(0.5f, 2), Random.Float(1, 4)));
                                    }
                                    Particles.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f, 0, 0, false, true));
                                    Particles[i].Expired = true;
                                }
                            }
                        }
                    }
                }
            }

            Boss.Update();
            Player.Update();
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
            }
            Particles.RemoveAll(p => p.Expired);

            if (Vector2.Distance(Player.Position, Boss.Position) <= Player.Radius + Boss.Radius & Player.IFrames == 0 & !Boss.Invincible)
            {
                Player.Damage(Boss.ContactDamage, 15);
            }

            // Enemy Updating and Collision Detection/Resolution
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].CurrentHealth == 0)
                {
                    int dustToAdd = Random.Integer(4, 8);
                    for (int j = 0; j < dustToAdd; j++)
                    {
                        Particles.Add(new Dust(Enemies[i].Position, Random.PointInCircle() * 4, new Vector2(0, -7.5f), Enemies[i].ColorPallete, Random.Float(1, 3), Enemies[i].Radius / Random.Float(3.5f, 5f)));
                    }
                    Audio.Play(minionDeath);
                    Enemies.Remove(Enemies[i]);
                }
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
                                    Particles.Add(new Bullet(enemy.Position + enemy.Radius * 1.4f * Vector2.Normalize(Player.Position - enemy.Position), 5 * Vector2.Normalize(Player.Position - enemy.Position) * ActionSpeedMultiplier, Vector2.Zero, [new(1, 0, 0), new(0)], enemy.Radius * 0.4f, 4, enemy.AttackDamage, 5, false, false));
                                    enemy.AttackCooldown = 1.25f;
                                }
                            }
                            else if (Vector2.Distance(enemy.Position, Player.Position) <= 1.5f * enemy.AttackRange)
                            {
                                enemy.Acceleration = 0.5f * Vector2.Normalize(Player.Position - enemy.Position);
                            }
                            break;
                        }
                    case "Orbiting":
                        {
                            float animationTime = Time.SecondsElapsed - Boss.ActionStartTime;
                            enemy.Position = Boss.Position + (Boss.Radius + 60) * RotateVector(Vector2.UnitX, 180 * animationTime + 360f / Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 } * enemy.EnemyID, true);
                            if (enemy.AttackCooldown == 0)
                            {
                                Particles.Add(new Bullet(
                                    enemy.Position + enemy.Radius * 1.8f * Vector2.Normalize(Player.Position - enemy.Position),
                                    12.5f * Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Player.Position - enemy.Position), RotateVector(Vector2.UnitX, 180 * animationTime + 360f / Difficulty switch { 'E' => 7, 'N' => 10, 'H' => 13, _ => 10 } * enemy.EnemyID, true), 0.75f)) / ActionSpeedMultiplier,
                                    Vector2.Zero,
                                    [new(1, 0, 0), new(0)],
                                    enemy.Radius * 0.8f,
                                    4,
                                    enemy.AttackDamage,
                                    5,
                                    true,
                                    false));
                                enemy.AttackCooldown = 1.25f / ActionSpeedMultiplier;
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


            // Borders
            Draw.FillColor = Color.Black;
            Draw.LineSize = 0;
            Draw.Rectangle(0, 0, 150, Window.Height);
            Draw.Rectangle(Window.Width - 150, 0, 150, Window.Height);

            // Player Health Bar
            Draw.FillColor = Draw.LineColor = Color.DarkGray;
            Draw.LineSize = 2;
            Draw.Rectangle(30, 400, 50, 480);
            Draw.FillColor = new ColorF(1 - (float)Player.CurrentHealth / Player.MaxHealth, (float)Player.CurrentHealth / Player.MaxHealth, 0.25f);
            Draw.LineSize = 0;
            Draw.Rectangle(new(32, 402 + 476 * (1 - (float)Player.CurrentHealth / Player.MaxHealth)), new(46, 476 * (float)Player.CurrentHealth / Player.MaxHealth));


            // Player Flight Timer Bar
            Draw.FillColor = Draw.LineColor = Color.DarkGray;
            Draw.LineSize = 2;
            Draw.Rectangle(100, 600, 20, 150);
            Draw.FillColor = new ColorF(0.25f, 0.7f, 1);
            Draw.LineSize = 0;
            Draw.Rectangle(new(102, 602 + 146 * (1 - Player.FlightTimer / 0.5f)), new(16, 146 * Player.FlightTimer / 0.5f));

            // Player Heal Cooldown Timer
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
                        if (Player.HealsRemaining != 0) { Draw.Arc(110, 840, 40, 40, 270, 270 + 360 * (Player.HealCooldown / 60)); }
                        Text.Draw($"{Player.HealsRemaining}", new Vector2(105, 830));
                        break;
                    }
            }

            // Boss Health Bar and Text
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
        }
    }
}
