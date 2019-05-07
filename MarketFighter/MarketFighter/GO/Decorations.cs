using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using MarketFighter.Engine;
using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Manager;

namespace MarketFighter.GO
{
    public class Decorations : CatchableGameObject
    {

        private ImageSource Image;

        public bool NeverCatched { get; set; } = true;

        public int CatchRange { get; set; }

        public Decorations(ImageSource image) : this(image, null)
        {

        }

        public Decorations(ImageSource image, string id) : base(id)
        {
            Image = image;
        }

        public override void Dispose()
        {
            
        }

        public override void OnInitialize()
        {
            SetImage(Image);
            RotatableSprite = false;
        }

        public override void OnLoad()
        {
            
        }

        public override void Release(ICatcherGameObject catcher)
        {
            base.Release(catcher);
            NeverCatched = false;
        }

        public override bool InRange(IGameObject gameObject, double range)
        {
            return Distance(gameObject) < range + 30;
        }

        public override void Run()
        {
            if (!Catched && !NeverCatched)
            {
                SpeedX *= 0.99;
                SpeedY += 980 / GameManager.ProcessingRate;
            }

            if (Y > GameEngine.FieldHeight + 200) GameEngine._GameManager.RemoveGameObject(this);
        }
    }
}
