using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MarketFighter.Engine;
using MarketFighter.Engine.Animation;
using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Manager;

namespace MarketFighter.GO
{
    public class Hand : GameObject, ICatcherGameObject
    {

        protected ImageSource image_Hand;
        protected ImageSource image_Hand_Grip;

        public bool Gripped { get; private set; }
        public bool Catched { get; private set; }
        //private Clothes CatchedClothes;
        private ICatchableGameObject CatchedObject;
        

        public Hand() : this(null)
        {

        }
        public Hand(string id) : base(id)
        {
            
        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            SetImage(image_Hand);
            GameEngine.RunOnUIThread(
                new Action<System.Windows.UIElement, int>(System.Windows.Controls.Panel.SetZIndex),
                GetSprite(), 99999);
        }

        public override void OnLoad()
        {
            image_Hand = GameEngine.GetImage("./Sprite/hand.png");
            image_Hand_Grip = GameEngine.GetImage("./Sprite/hand_grip.png");            
        }

        public bool Catch()
        {
            SetImage(image_Hand_Grip);
            Gripped = true;
            //Catched = false;
            var gos = GameEngine._GameManager.GetGameObjects();
            gos = gos.Where(g => g is ICatchableGameObject).Reverse();

            foreach (ICatchableGameObject c in gos)
            {
                if (c is Clothes)
                {
                    var co = c as Clothes;
                    if (co.InRange(this, 35))
                    {
                        if (Catch(co)) return true;
                    }
                }
                else
                {
                    if (c.InRange(this, 35))
                    {
                        if (Catch(c)) return true;
                    }
                }
            }
            return false;
        }

        public bool Catch(ICatchableGameObject catchable)
        {
            var cago = catchable.Catch(this);
            if (cago == null) return false;
            CatchedObject = catchable;
            Catched = true;
            return true;
        }

        public void Release()
        {

            SetImage(image_Hand);
            Gripped = false;
            Catched = false;
            if (CatchedObject != null)
            {
                CatchedObject.Release(this);
                CatchedObject = null;
            }
        }

        public override void Run()
        {

        }
    }
}
