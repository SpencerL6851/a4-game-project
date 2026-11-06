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
            Window.SetSize(1200, 900);
            Boss.Setup();
            Player.Setup();
        }

        /// <summary>
        ///     Update runs every frame.
        /// </summary>
        public void Update()
        {
            Window.ClearBackground(96);
            Text.Color = Color.Black;
            Text.Size = 20;
            //Text.Draw($"Current Action: {Boss.CurrentAction} \nNext Action: {Boss.NextAction} \nCounter: {Boss.ActionCounter} \nTimer: {Boss.ActionTimer} \nAnimation {float.Round(Boss.AnimationPercent * 100)}% Complete", new Vector2(0, 0));
            //Text.Draw($"Player Position: {Player.Position} \nPlayer Velocity: {Player.Velocity} \nPlayer Acceleration: {Player.Acceleration}", new(0, 6 * Text.Size));
            switch (Boss.CurrentPhase)
            {
                case 1: // Phase 1 Logic
                    {
                        switch (Boss.CurrentAction)
                        {
                            case "Shoot Bullets":
                                {
                                    Vector2 directionToPlayer = Vector2.Normalize(Player.Position - Boss.Position);
                                    if (Boss.ActionTimer < (0.5f * Boss.ActionCounter))
                                    {
                                        Particles.Add(new HomingBullet(Boss.Position + Boss.Radius * directionToPlayer, 12.5f * directionToPlayer, Vector2.Zero, [new(1, 0, 0), new(0)], 20, 5, 7, 5, Player, 30, -1));
                                        Boss.ActionCounter -= 1;
                                    }
                                    if (Boss.ActionTimer < 0)
                                    {
                                        Boss.CurrentAction = "Idle";
                                        Boss.ActionDuration = Boss.ActionTimer = 0.5f;
                                    }
                                    break;
                                }
                            case "Summon":
                                {
                                    break;
                                }
                            case "Flying":
                                {
                                    break;
                                }
                            case "Dash":
                                {
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
                                        }
                                    }
                                    else
                                    {
                                        Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f) + Random.Direction() * 3;
                                        if (Boss.ActionTimer <= Boss.ActionDuration - (0.05f * Boss.ActionCounter))
                                        {
                                            Particles.Add(new Shockwave(Boss.Position, [new(1f)], 0.75f, 25, 45, 1200));
                                            Boss.ActionCounter += 1;
                                        }
                                        if (Boss.AnimationPercent == 1)
                                        {
                                            Boss.Position = new Vector2(Window.Width * 0.5f, Window.Height * 0.25f);
                                            Boss.CurrentAction = "Idle";
                                            Boss.ActionDuration = Boss.ActionTimer = 0.5f;
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
                                                    Boss.NextAction = "Summon";
                                                    Boss.ActionDuration = Boss.ActionTimer = 5f;
                                                    Boss.ActionCounter = 10;
                                                    break;
                                                }
                                            case "Summon":
                                                {
                                                    Boss.NextAction = "Flying";
                                                    break;
                                                }
                                            case "Flying":
                                                {
                                                    Boss.ActionDuration = Boss.ActionTimer = Random.Integer(3, 6);
                                                    Boss.NextAction = "Dash";
                                                    break;
                                                }
                                            case "Dash":
                                                {
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
                            case "Shoot Bullet Fan":
                                {
                                    break;
                                }
                            case "Summon":
                                {
                                    break;
                                }
                            case "Beam":
                                {
                                    break;
                                }
                            case "Idle":
                                {
                                    if (Boss.AnimationPercent == 1)
                                    {
                                        switch (Boss.NextAction)
                                        {
                                            case "Shoot Bullet Fan":
                                                {
                                                    break;
                                                }
                                            case "Summon":
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
                // Phase 3 Logic
                case 3:
                    {
                        switch (Boss.CurrentAction)
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
                            case "Invincible":
                                {
                                    break;
                                }
                            case "Idle":
                                {
                                    if (Boss.AnimationPercent == 1 & Boss.CurrentAction != "Invincible")
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
                            case "Idle":
                                {
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
            for (int i = 0; i < Particles.Count; i++)
            {
                Particles[i].Update();
                if (Particles[i].Expired) { Particles.Remove(Particles[i]); }
            }
            List<dynamic> _ = [];
            for (int i = 0; i < Particles.Count; i++)
            {
                if (Particles[i] is Beam | Particles[i] is Bullet | Particles[i] is HomingBullet)
                {
                    if (Particles[i].IsHittingTarget(Player, 0.95f) & Player.IFrames == 0)
                    {
                        Player.Damage(Particles[i].Damage, Particles[i].IFrames);
                        if (Particles[i] is Bullet | Particles[i] is HomingBullet)
                        {
                            _.Add(new Shockwave(Particles[i].Position, Particles[i].ColorPalette, 0.2f, 3, Particles[i].Radius * 0.5f, Particles[i].Radius * 1.5f));
                            Particles.Remove(Particles[i]);
                        }
                    }
                }
            }
            Particles.AddRange( _ );
            Player.Update();
            Boss.Update();
        }
    }
}
