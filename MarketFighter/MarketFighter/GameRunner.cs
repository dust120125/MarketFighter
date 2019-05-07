using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;
using MarketFighter.GO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MarketFighter
{
    public class GameRunner
    {
        public static int GameTime = 60000;

        ItemTable table, table2;

        public readonly static HashSet<Hand> Hands = new HashSet<Hand>();

        public readonly static HashSet<ShoppingCart> Carts = new HashSet<ShoppingCart>();

        public readonly static List<Clothes> GotClothes = new List<Clothes>();

        private List<Decorations> TitleDecorations = new List<Decorations>(15);

        private bool Checkouted;
        private bool Ended;

        //Price 花費，Value 價值
        private TextObject PriceCounter;
        private TextObject ValueCounter;
        private int TotalPrice;
        private int TotalValue;

        private int inGameCarts;

        public GameRunner()
        {
            try
            {
                LoadConfig();
            }
            catch { }
        }

        public void Start()
        {
            CatchableTextObject titleL = new CatchableTextObject()
            {
                Text = "Market",
                FontColor = Brushes.Aqua,
                FontSize = 120,
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Effect = new DropShadowEffect()
            };
            titleL.SetPosition(GameEngine.FieldWidth / 2 - 225, GameEngine.FieldHeight * 0.2);
            titleL.SpriteInitialized += () => { GameEngine.SetZIndex(titleL.GetSprite(), 100); };
            GameEngine._GameManager.AddGameObject(titleL);

            CatchableTextObject titleR = new CatchableTextObject()
            {
                Text = "Fighter",
                FontColor = Brushes.Aqua,
                FontSize = 120,
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Effect = new DropShadowEffect()
};
            titleR.SetPosition(GameEngine.FieldWidth / 2 + 225, GameEngine.FieldHeight * 0.2);
            titleR.SpriteInitialized += () => { GameEngine.SetZIndex(titleR.GetSprite(), 99); };
            GameEngine._GameManager.AddGameObject(titleR);

            StartButton button = new StartButton();
            button.SetPosition(GameEngine.FieldWidth / 2, GameEngine.FieldHeight - 50);
            GameEngine._GameManager.AddGameObject(button);
            button.OnPress += () =>
            {
                titleL.NeverCatched = false;
                titleR.NeverCatched = false;
                foreach (Decorations de in TitleDecorations)
                {
                    de.NeverCatched = false;
                }
            };

            button.OnRemove += () =>
            {
                new Thread(RunGame).Start();
            };

            for (int i = 0; i < 10; i++)
            {
                int index = GameEngine.Randomer.Next(Clothes.Types);
                Clothes.ClothesType type = (Clothes.ClothesType)Enum.GetValues(
                    typeof(Clothes.ClothesType)).GetValue(index);

                Decorations d1 = new Decorations(Clothes.GetClothesImage(type))
                {
                    ScaleX = 0.5,
                    ScaleY = 0.5,
                    CatchRange = 50,
                    Direction = GameEngine.Randomer.Next(0, 360),
                    RotatableSprite = true
                };
                d1.SetPosition(
                    GameEngine.Randomer.Next((int)(GameEngine.FieldWidth * 0.15), (int)(GameEngine.FieldWidth * 0.85)),
                    GameEngine.Randomer.Next((int)(GameEngine.FieldHeight * 0.30), (int)(GameEngine.FieldHeight * 0.65)));
                GameEngine._GameManager.AddGameObject(d1);

                TitleDecorations.Add(d1);
            }

            Decorations gripHand = new Decorations(GameEngine.GetImage("./Sprite/hand_grip.png"), "starter")
            {
                Direction = 295,
                CatchRange = 30,
                RotatableSprite = true
            };
            gripHand.SetPosition(GameEngine.FieldWidth * 0.1, GameEngine.FieldHeight * 0.1);
            GameEngine._GameManager.AddGameObject(gripHand);
        }

        private void LoadConfig()
        {
            var lines = File.ReadAllLines("config.ini");
            foreach(string line in lines)
            {
                var tmp = line.Split(':');
                if (tmp.Length < 2) continue;
                switch (tmp[0].Trim().ToLower())
                {
                    case "time":
                        if (int.TryParse(tmp[1].Trim(), out int t))
                            GameTime = t;
                        break;
                }
            }
        }

        private void RunGame()
        {
            inGameCarts = 0;

            table = new ItemTable() { Name = "table" };
            table.SetPosition(92, GameEngine.FieldHeight - 90);
            GameEngine._GameManager.AddGameObject(table);

            table2 = new ItemTable() { Name = "table" };
            table2.SetPosition(GameEngine.FieldWidth - 92, GameEngine.FieldHeight - 90);
            GameEngine._GameManager.AddGameObject(table2);

            ShoppingCart cart = new ShoppingCart();
            cart.SetPosition(100, 0);
            cart.EnterGame += () => { inGameCarts++; };
            cart.LeftGame += () => { inGameCarts--; };
            GameEngine._GameManager.AddGameObject(cart);
            Carts.Add(cart);

            GameTimer tgo = new GameTimer(true)
            {
                MaxTime = GameTime
            };
            tgo.SetPosition(GameEngine.FieldWidth / 2, GameEngine.FieldHeight - 70);
            GameEngine._GameManager.AddGameObject(tgo);
            tgo.Start();
            tgo.CountdownEnds += () => 
            {
                foreach(EnemyHand eh in GameEngine._GameManager.GetGameObjectsByName("trep").
                    Where(_ => _ is EnemyHand))
                {
                    eh.Release();
                }

                var mt = new MoveTask()
                {
                    DestX = GameEngine.FieldWidth + 200,
                    DestY = GameEngine.FieldHeight * 0.3,
                    Speed = 900,
                    Force = true
                };
                foreach (ShoppingCart sc in Carts.Where(_ => _.InGame))
                {
                    GotClothes.AddRange(sc.CarriedClothes);
                    sc.Catchable = false;
                    sc.MoveTask = mt;
                }

                Checkouted = true;
                Checkout(GotClothes);
            };

            while (!Ended && !Checkouted)
            {
                int rate = GameEngine.Randomer.Next(10);

                if (rate < 5)
                {
                    Clothes c1 = new Clothes()
                    {
                        Name = "commodity",
                        RotatableSprite = true
                    };
                    c1.SetPosition(GameEngine.Randomer.Next(50, (int)GameEngine.FieldWidth - 50), -70);
                    c1.SetSpeed(50, 90);
                    GameEngine._GameManager.AddGameObject(c1);
                }

                int trapRate = GameEngine.Randomer.Next(7);
                if (trapRate == 0)
                {
                    SoySauce ss = new SoySauce()
                    {
                        Name = "commodity"
                    };
                    GameEngine._GameManager.AddGameObject(ss);
                }

                Console.WriteLine("In Game Carts: " + inGameCarts);
                if (inGameCarts < 4)
                {
                    int cartRate = GameEngine.Randomer.Next(10);
                    if (cartRate == 0)
                    {
                        ShoppingCart exCart = new ShoppingCart();
                        exCart.SetPosition(GameEngine.Randomer.Next(50, (int)GameEngine.FieldWidth - 50), 0);
                        exCart.EnterGame += () => { inGameCarts++; };
                        exCart.LeftGame += () => { inGameCarts--; };
                        GameEngine._GameManager.AddGameObject(exCart);
                        Carts.Add(exCart);
                    }
                }

                int eHandRate = GameEngine.Randomer.Next(5);
                if (eHandRate == 0)
                {
                    double x, y, speedX;                    
                    if (GameEngine.Randomer.Next(2) == 0)
                    {
                        x = -40;
                        speedX = GameEngine.Randomer.Next(100, 300);
                    }
                    else
                    {
                        x = GameEngine.FieldWidth + 40;
                        speedX = GameEngine.Randomer.Next(-300, -100);
                    }

                    if (GameEngine.Randomer.Next(10) == 0)
                    {
                        y = GameEngine.Randomer.Next(
                            (int)(GameEngine.FieldHeight - 280),
                            (int)(GameEngine.FieldHeight - 190));
                    }
                    else
                    {
                        y = GameEngine.Randomer.Next(100, (int)(GameEngine.FieldHeight * 0.6));
                    }

                    double dir = GameEngine.Randomer.Next(0, 360); ;

                    EnemyHand eh = new EnemyHand()
                    {
                        Name = "trap",
                        Direction = dir,
                        SpeedX = speedX,
                        RotatableSprite = true
                    };
                    eh.SetPosition(x, y);
                    GameEngine._GameManager.AddGameObject(eh);
                }

                Thread.Sleep(1000);
            }

        }

        private void Checkout(List<Clothes> clothes)
        {
            for (int i = 0; i < GotClothes.Count; i++)
            {
                Clothes c = GotClothes[i];
                TotalPrice += (int)(c.Price * (100.0 - c.Discount) / 100);

                double X = GameEngine.FieldWidth + 1000 + i * 420;
                double Y = GameEngine.FieldHeight * 0.4;
                c.SetPosition(X, Y);
                c.Direction = 0;
                c.SpeedX = -500;
                c.SpeedY = 0;
                c.Checkout = true;
                c.Catchable = false;
                c.OnPriceCheckout += (p) => { ValueCounter.Text = "總價值：$" + (TotalValue += p); };
                GameEngine._GameManager.AddGameObject(c);
            }

            PriceCounter = new TextObject();
            PriceCounter.SpriteInitialized += () =>
            {
                PriceCounter.Effect = new DropShadowEffect();
                PriceCounter.FontColor = Brushes.Green;
                PriceCounter.FontSize = 40;
                PriceCounter.FontWeight = FontWeights.Bold;
                PriceCounter.Text = "總花費：$" + TotalPrice;
            };
            PriceCounter.SetPosition(GameEngine.FieldWidth / 2, GameEngine.FieldHeight * 0.1);
            GameEngine._GameManager.AddGameObject(PriceCounter);

            PriceCounter.RegisterAnimation(new AnimationProvider(GetTextZoomInAnimation(), true));

            ValueCounter = new TextObject();
            ValueCounter.SpriteInitialized += () =>
            {
                ValueCounter.Effect = new DropShadowEffect();
                ValueCounter.FontColor = Brushes.Green;
                ValueCounter.FontSize = 40;
                ValueCounter.FontWeight = FontWeights.Bold;
                ValueCounter.Text = "總價值：$" + TotalValue;
            };
            ValueCounter.SetPosition(GameEngine.FieldWidth / 2, GameEngine.FieldHeight * 0.4 + 280);
            GameEngine._GameManager.AddGameObject(ValueCounter);


            foreach (Hand h in Hands)
            {
                h.Release();
            }

            CleanField();

            End();
        }

        private Animation GetTextZoomInAnimation()
        {
            List<AnimationFrameInfo> infos = new List<AnimationFrameInfo>();
            int interval = 25;
            int frames = 25;
            int endFrames = 10;
            double max = 3;
            double si = (max - 1) / frames;
            for (int i = 0; i < frames + endFrames; i++)
            {
                AnimationFrameInfo info;
                int next = i == frames + endFrames - 1 ? -1 : i + 1;
                if (i < frames)
                {
                    info = new AnimationFrameInfo(
                        new TextAnimationFrame(null, 0d, 0d, 0d, max - i * si, max - i * si, null),
                        Math.Max(10, interval - i),
                        next);
                }
                else
                {
                    int ox = GameEngine.Randomer.Next(-10, 10);
                    int oy = GameEngine.Randomer.Next(-10, 10);
                    info = new AnimationFrameInfo(
                        new TextAnimationFrame(null, ox, oy, 0d, null, null, null),
                        interval,
                        next);
                }
                infos.Add(info);
            }
            return new Animation(null, infos);
        }

        private void CleanField()
        {
            foreach(IGameObject go in GameEngine._GameManager.GetGameObjectsByNames("commodity", "trap"))
            {
                GameEngine._GameManager.RemoveGameObject(go);
            }
            GameEngine._GameManager.RemoveGameObject(table);
            GameEngine._GameManager.RemoveGameObject(table2);
        }

        public void End()
        {
            Ended = true;
        }

    }
}
