using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MarketFighter.Engine;
using MarketFighter.Engine.GameObject;
using MarketFighter.Engine.Manager;

namespace MarketFighter.GO
{
    public class CatchableTextObject : CatchableTextGameObject
    {

        public bool NeverCatched { get; set; } = true;

        public CatchableTextObject() : base(null)
        {

        }

        public override void Dispose()
        {
            
        }

        public override void OnInitialize()
        {
            
        }

        public override void OnLoad()
        {
            
        }

        public override void Release(ICatcherGameObject catcher)
        {
            base.Release(catcher);
            NeverCatched = false;
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

        public override bool InRange(IGameObject go, double range)
        {
            return Distance(X, go.X, Y, go.Y) < range + Math.Min(Width, Height) / 2;
        }
    }
}
