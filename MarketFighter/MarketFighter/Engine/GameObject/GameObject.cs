using MarketFighter.Engine.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MarketFighter.Engine.GameObject
{
    public abstract class GameObject : BaseGameObject<Image>
    {
        //public Image ImageSprite { get; private set; }
        public double ImageWidth { get; private set; }
        public double ImageHeight { get; private set; }

        private Action<ImageSource> SetImageAction;

        //########## Methods ##########

        public GameObject(string id) : base(id)
        {
            SetImageAction = new Action<ImageSource>(SetImageDelegate);
        }

        public void SetImage(ImageSource image)
        {
            GameEngine.RunOnUIThread(SetImageAction, image);
            ImageWidth = image.Width;
            ImageHeight = image.Height;
        }

        public override void CenterToSprite()
        {
            CenterX = ImageWidth * ScaleX / 2;
            CenterY = ImageHeight * ScaleY / 2;
        }

        private void SetImageDelegate(ImageSource image)
        {
            _SpriteView.Source = image;
            if (AlignCenter) CenterToSprite();
            _RotateTransform.CenterX = ImageWidth * ScaleX / 2;
            _RotateTransform.CenterY = ImageHeight * ScaleY / 2;
        }

        public override void SetActualScale(double scaleX, double scaleY)
        {
            _ScaleTransform.ScaleX = scaleX;
            _ScaleTransform.ScaleY = scaleY;
            _RotateTransform.CenterX = ImageWidth * scaleX / 2;
            _RotateTransform.CenterY = ImageHeight * scaleY / 2;
        }
    }
}
