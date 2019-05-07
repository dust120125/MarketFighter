using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public class ImageParticle : GameObject, IParticle
    {
        public ParticleType Type { get { return ParticleType.Image; } }
        public long Birth { get; set; }
        public long Life { get; set; }
        public double RotateDegree { get; set; }
        public double MaxRotateDegree { get; set; }
        public double MinRotateDegree { get; set; }
        public double RotateAcceleration { get; set; }
        public double AccelerationX { get; set; }
        public double AccelerationY { get; set; }
        public double MaxSpeedX { get; set; }
        public double MaxSpeedY { get; set; }
        public double MinSpeedX { get; set; }
        public double MinSpeedY { get; set; }
        public bool Gravity { get; set; }
        public bool AirResistance { get; set; }
        public Animation.AnimationProvider AnimationProvider { get; set; }
        public bool WaitAnimationEnd { get; set; }

        public ImageParticle() : base(null)
        {
            RotatableSprite = true;
        }

        public override long GetAge()
        {
            return GameEngine.GetMillisecond() - Birth;
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

        public override void Run()
        {
            
        }
    }
}
