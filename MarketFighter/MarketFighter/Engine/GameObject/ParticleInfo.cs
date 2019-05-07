using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MarketFighter.Engine.GameObject
{

    public enum ParticleType { Image, Text }

    public struct ParticleInfo
    {
        public double X;
        public double Y;
        public double SpeedX;
        public double SpeedY;
        public double Opacity;
        public double Direction;

        public ParticleType Type;
        public object Visual;
        public int Life;
        public double RotateDegree;
        public double MaxRotateDegree;
        public double MinRotateDegree;
        public double RotateAcceleration;
        public double AccelerationX;
        public double AccelerationY;
        public double MaxSpeedX;
        public double MaxSpeedY;
        public double MinSpeedX;
        public double MinSpeedY;
        public bool Gravity;
        public bool AirResistance;
        public Animation.AnimationProvider AnimationProvider;
        public bool WaitAnimationEnd;
        public int ZIndex;

        public Brush FontColor;
        public double FontSize;
        public FontWeight FontWeight;
        public FontStyle FontStyle;

        public ParticleInfo(ParticleType type, object visual, int life) : this()
        {
            Type = type;
            Visual = visual;
            Life = life;
        }
    }
}
