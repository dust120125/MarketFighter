using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;

namespace MarketFighter.GO
{
    public class StartButton : CatchableGameObject
    {

        private static ImageSource Button_u = GameEngine.GetImage("./sprite/button_u.png");
        private static ImageSource Button_d = GameEngine.GetImage("./sprite/button_d.png");

        public event Action OnPress;
        public event Action OnRemove;

        private bool Pressed { get; set; }
        private long PressedTime = -1;

        private AnimationProvider animationProvider = new AnimationProvider(
            new Animation(null,
                Button_d, 0d, 0d, 0d, null, null, null, 150, 1,
                Button_u, 0d, 0d, 0d, null, null, null, 1, -1),
            false);

        public StartButton() : this(null)
        {

        }

        public StartButton(string id) : base(id)
        {

        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            SetImage(Button_u);
        }

        public override void OnLoad()
        {

        }

        public override void Run()
        {
            if (!Pressed)
            {
                IGameObject go = GameEngine._GameManager.GetGameObjectByID("starter");
                if (go != null)
                {
                    double disX = Math.Abs(go.X - X);
                    double disY = Y - go.Y;
                    if (disY > 0 && disY < 50 && disX < ImageWidth / 2)
                    {
                        if (go.SpeedY > 750 || go.ActualSpeedY > 230)
                        {
                            Pressed = true;
                            PressedTime = GameEngine.GetMillisecond();
                            OnPress?.Invoke();
                            PressedAnimation();
                        }

                        go.SpeedX = GameEngine.Randomer.Next(800) - 400;
                        go.SpeedY *= -0.7;
                    }
                }
            }

            if (PressedTime != -1 && GameEngine.GetMillisecond() - PressedTime > 3000)
            {
                GameEngine._GameManager.RemoveGameObject(this);
                OnRemove?.Invoke();
            }

        }

        private void PressedAnimation()
        {
            RegisterAnimation(animationProvider);
            animationProvider.Reset(true);
            PressedParticle1();
        }

        private void PressedParticle1()
        {
            ParticleInfo info = new ParticleInfo()
            {
                Type = ParticleType.Text,
                X = X,
                Y = Y,
                Visual = "✦",
                Gravity = true,
                AirResistance = true,
                Opacity = 1,
                FontColor = Brushes.Purple,
                FontSize = 44,
                WaitAnimationEnd = false
            };

            for (int i = 0; i < 30; i++)
            {
                info.RotateDegree = GameEngine.Randomer.Next(-15, 15);
                info.SpeedX = GameEngine.Randomer.Next(-350, 350);
                info.SpeedY = GameEngine.Randomer.Next(-1250, -400);
                info.Life = GameEngine.Randomer.Next(1500, 2800);
                GameEngine._ParticleManager.AddRequest(info);
            }
        }

        private void PressedParticle2()
        {
            Animation HandGripAnim = new Animation(null,
                GameEngine.GetImage("./Sprite/hand.png"), 0d, 0d, 0d, null, null, null, 45, 1,
                GameEngine.GetImage("./Sprite/hand_grip.png"), 0d, 0d, 0d, null, null, null, 45, 0
            );

            ParticleInfo info = new ParticleInfo()
            {
                Type = ParticleType.Image,
                X = X,
                Y = Y,
                Visual = GameEngine.GetImage("./sprite/hand.png"),
                Gravity = true,
                AirResistance = true,
                Opacity = 1,
                WaitAnimationEnd = false
            };

            for (int i = 0; i < 15; i++)
            {
                info.AnimationProvider = new AnimationProvider(HandGripAnim, false);
                info.RotateDegree = GameEngine.Randomer.Next(-15, 15);
                info.SpeedX = GameEngine.Randomer.Next(-350, 350);
                info.SpeedY = GameEngine.Randomer.Next(-1350, -650);
                info.Life = GameEngine.Randomer.Next(1500, 3000);
                GameEngine._ParticleManager.AddRequest(info);
            }

            Animation TextAnim = new Animation(null,
                "OOOO", 0d, 0d, 0d, null, null, null, 45, 1,
                "XXXX", 0d, 0d, 0d, null, null, null, 45, 0
            );

            ParticleInfo tInfo = new ParticleInfo()
            {
                Type = ParticleType.Text,
                X = X,
                Y = Y,
                Visual = "WWWW",
                Gravity = true,
                AirResistance = true,
                Opacity = 1,
                FontColor = Brushes.Purple,
                FontSize = 36,
                FontStyle = FontStyles.Oblique,
                FontWeight = FontWeights.Heavy,
                WaitAnimationEnd = false
            };

            for (int i = 0; i < 15; i++)
            {
                tInfo.AnimationProvider = new AnimationProvider(TextAnim, false);
                tInfo.RotateDegree = GameEngine.Randomer.Next(-15, 15);
                tInfo.SpeedX = GameEngine.Randomer.Next(-350, 350);
                tInfo.SpeedY = GameEngine.Randomer.Next(-1350, -650);
                tInfo.Life = GameEngine.Randomer.Next(1500, 3000);
                GameEngine._ParticleManager.AddRequest(tInfo);
            }
        }

        public override bool InRange(IGameObject gameObject, double range)
        {
            return Distance(gameObject) < range + 30;
        }
    }
}
