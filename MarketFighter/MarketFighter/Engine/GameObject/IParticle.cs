using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public interface IParticle : IGameObject
    {
        ParticleType Type { get; }
        long Birth { get; set; }
        long Life { get; set; }
        double RotateDegree { get; set; }
        double MaxRotateDegree { get; set; }
        double MinRotateDegree { get; set; }
        double RotateAcceleration { get; set; }
        double AccelerationX { get; set; }
        double AccelerationY { get; set; }
        double MaxSpeedX { get; set; }
        double MaxSpeedY { get; set; }
        double MinSpeedX { get; set; }
        double MinSpeedY { get; set; }
        bool Gravity { get; set; }
        bool AirResistance { get; set; }
        Animation.AnimationProvider AnimationProvider { get; set; }       
        bool WaitAnimationEnd { get; set; }
    }
}
