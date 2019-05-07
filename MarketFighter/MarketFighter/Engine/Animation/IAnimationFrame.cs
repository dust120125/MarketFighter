
using System.Windows.Media;

namespace MarketFighter.Engine.Animation
{
    public abstract class AnimationFrame
    {
        public abstract object Visual { get; }
        public double AnimationOffsetX { get; private set; }
        public double AnimationOffsetY { get; private set; }
        public double AnimationRotation { get; private set; }
        public double? ScaleX { get; private set; }
        public double? ScaleY { get; private set; }
        public double? Opacity { get; private set; }

        public AnimationFrame(double animationOffsetX, double animationOffsetY,
                double animationRotation, double? scaleX, double? scaleY, double? opacity)
        {
            AnimationOffsetX = animationOffsetX;
            AnimationOffsetY = animationOffsetY;
            AnimationRotation = animationRotation;
            ScaleX = scaleX;
            ScaleY = scaleY;
            Opacity = opacity;
        }
    }
}
