using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using MarketFighter.Engine;
using MarketFighter.Engine.GameObject;

namespace MarketFighter.GO
{
    public class SoySauce : CatchableGameObject, ICommodity
    {
        public static readonly int Power = 3;
        public bool InCart { get; private set; }

        public bool NeverCatched { get; private set; } = true;

        public override void Dispose()
        {
            
        }

        public override void OnInitialize()
        {
            DuplicateCatch = true;
            SetImage(GameEngine.GetImage("./sprite/soy_sauce.png"));
            double w = GameEngine.FieldWidth, h = GameEngine.FieldHeight;
            double x = 0, y = 0;
            int dir = GameEngine.Randomer.Next(3);
            switch (dir)
            {
                //Top
                case 0:
                    x = GameEngine.Randomer.Next(-50, (int)w + 50);
                    y = -50;
                    break;
                //Left
                case 1:
                    x = -50;
                    y = GameEngine.Randomer.Next(-50, (int)(h * 0.33));                    
                    break;
                //Right
                case 2:
                    x = w + 50;
                    y = GameEngine.Randomer.Next(-50, (int)(h * 0.33));
                    break;
            }

            ScaleX = 0.3;
            ScaleY = 0.3;

            SetPosition(x, y);

            double disX = X - w / 2;
            double disY = Y - h / 2;
            double angle = (Math.Atan(disY / disX) / RADIAN);
            if (disX > 0) angle += 180;

            double speed = GameEngine.Randomer.Next(150, 250);
            SetSpeed(speed, angle);
            RotatableSprite = true;
        }

        public override void OnLoad()
        {
            
        }

        public override ICatchableGameObject Catch(ICatcherGameObject catcher)
        {
            var re = base.Catch(catcher);
            if (re != null) NeverCatched = false;
            return re;            
        }

        public override void Run()
        {
            if (NeverCatched)
            {
                Direction += 5;
            }
            else
            {
                SpeedX *= 0.99;
                SpeedY += 980 / GameEngine.ProcessingRate;
            }
            
            if (X < -60 || X > GameEngine.FieldWidth + 60 ||
                Y < -60 || Y > GameEngine.FieldHeight + 60)
            {
                GameEngine._GameManager.RemoveGameObject(this);
            }

        }

        public bool PutIntoCart(ShoppingCart cart)
        {
            if (InCart) return false;

            InCart = true;

            ParticleInfo info = new ParticleInfo()
            {
                Type = ParticleType.Text,
                X = X,
                Y = Y,
                Gravity = true,
                AirResistance = true,
                Opacity = 1,
                FontStyle = FontStyles.Oblique,
                FontWeight = FontWeights.Heavy,
                WaitAnimationEnd = false
            };

            for (int i = 0; i < 25; i++)
            {
                int tmp = GameEngine.Randomer.Next(3);
                if (tmp == 0)
                {
                    info.Visual = "▲";
                    info.FontColor = Brushes.Gray;
                    info.FontSize = 20;
                }
                else
                {
                    info.Visual = "●";
                    info.FontColor = Brushes.Brown;
                    info.FontSize = 10;
                }
                info.RotateDegree = GameEngine.Randomer.Next(-5, 5);
                info.SpeedX = GameEngine.Randomer.Next(-250, 250);
                info.SpeedY = GameEngine.Randomer.Next(-250, 0);
                info.Life = GameEngine.Randomer.Next(1500, 3000);
                GameEngine._ParticleManager.AddRequest(info);
            }

            GameEngine._GameManager.RemoveGameObject(this);
            return true;
        }

        public override bool InRange(IGameObject gameObject, double range)
        {
            return Distance(gameObject) < range + 20;
        }
    }
}
