using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Manager;

namespace MarketFighter.GO
{
    public class ShoppingCart : CatchableGameObject
    {

        private static ImageSource Image;
        private ItemTable CarrierTable;
        private bool OnTable;

        private AnimationProvider PutItemAnimation;

        public List<Clothes> CarriedClothes { get; private set; }

        public ShoppingCart() : this(null)
        {

        }

        public ShoppingCart(string id) : base(id)
        {

        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            DuplicateCatch = true;
            SetImage(Image);
            ScaleX = 0.3;
            ScaleY = 0.3;


            GameEngine.RunOnUIThread(
                new Action<System.Windows.UIElement, int>(System.Windows.Controls.Panel.SetZIndex),
                GetSprite(), -1);


            int interval = 35;
            Animation anime = new Animation(new ImageAnimationFrame(null, 0, 0, 0, ScaleX, ScaleY, null),
                        null, 0d, 0d, 0d, ScaleX * 1.25, ScaleY * 1.15, null, interval, 1,
                        null, 0d, 0d, 0d, ScaleX * 1.20, ScaleY * 1.1, null, interval, 2,
                        null, 0d, 0d, 0d, ScaleX * 1.15, ScaleY * 1.05, null, interval, 3,
                        null, 0d, 0d, 0d, ScaleX * 1.10, ScaleY * 0.95, null, interval, 4,
                        null, 0d, 0d, 0d, ScaleX * 1.05, ScaleY, null, interval, 5,
                        null, 0d, 0d, 0d, ScaleX * 0.95, ScaleY, null, interval, -1);
            PutItemAnimation = new AnimationProvider(anime, false);
        }

        public override void OnLoad()
        {
            if (Image == null)
                Image = GameEngine.GetImage("./sprite/shopping_cart.png");

            CarriedClothes = new List<Clothes>(10);
        }

        public override ICatchableGameObject Catch(ICatcherGameObject catcher)
        {
            if (OnTable)
            {
                CarrierTable.Drop();
                OnTable = false;
            }
            return base.Catch(catcher);
        }

        private void PlayPutItemAnimation()
        {
            PutItemAnimation.Reset(false);
            if (PutItemAnimation.State == AnimationProvider.PlayState.Initialized ||
                PutItemAnimation.State == AnimationProvider.PlayState.Dead)
                RegisterAnimation(PutItemAnimation);

            PutItemAnimation.Play();
        }

        public override void Run()
        {
            if (!Catched)
            {
                if (!OnTable)
                {
                    var gos = GameEngine._GameManager.GetGameObjectsByName("table");
                    foreach (ItemTable t in gos)
                    {
                        double dis = Distance(X, t.X, Y, t.Y - 135);
                        if (dis < 70)
                        {
                            if (!t.Carry(this)) continue;
                            SetPosition(t.X, t.Y - 135);
                            SetSpeed(0, 0);
                            OnTable = true;
                            CarrierTable = t;
                            return;
                        }
                    }
                    SpeedY += 980 / GameManager.ProcessingRate;
                }
            }

            Clothes clothes;
            SoySauce soySauce;
            var commodityList = GameEngine._GameManager.GetGameObjectsByNames("commodity");
            foreach (ICommodity com in commodityList)
            {
                if (com is Clothes)
                {
                    clothes = com as Clothes;
                    if (!clothes.NeverCatched && clothes.InRange(this, 35))
                    {
                        if (!clothes.PutIntoCart(this)) continue;
                        CarriedClothes.Add(clothes);
                        PlayPutItemAnimation();
                    }
                }
                else if (com is SoySauce)
                {
                    soySauce = com as SoySauce;
                    if (Distance(soySauce) < 70)
                    {
                        if (!soySauce.PutIntoCart(this)) continue;
                        int power = SoySauce.Power;
                        for (int i = 0, s = 0; i < CarriedClothes.Count && s < power; i++)
                        {
                            var c = CarriedClothes[i];
                            if (c.Stained) continue;

                            c.Staine();
                            s++;
                        }
                    }
                }
            }

            if (Y > GameEngine.FieldHeight + 120 ||
                X < -200 || X > GameEngine.FieldWidth + 200)
                GameEngine._GameManager.RemoveGameObject(this);
        }

        public override bool InRange(IGameObject gameObject, double range)
        {
            return Distance(gameObject) < range + 70;
        }
    }
}
