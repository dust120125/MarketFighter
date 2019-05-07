using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using MarketFighter.Engine;
using MarketFighter.Engine.GameObject;

namespace MarketFighter.GO
{
    public class ItemTable : GameObject
    {

        private static ImageSource Image;
        private IGameObject CarriedItem;

        public ItemTable(string id) : base(id)
        {

        }

        public ItemTable() : this(null)
        {

        }

        public override void Dispose()
        {

        }

        public override void OnInitialize()
        {
            SetImage(Image);
            GameEngine.SetZIndex(_SpriteView, -2);

            ScaleX = 0.3;
            ScaleY = 0.3;
        }

        public override void OnLoad()
        {
            if (Image == null) Image = GameEngine.GetImage("./sprite/table.png");
            
        }

        public override void Run()
        {
            
        }

        public bool Carry(IGameObject go)
        {
            if (CarriedItem != null) return false;

            CarriedItem = go;
            return true;
        }

        public void Drop()
        {
            CarriedItem = null;
        }

    }
}
