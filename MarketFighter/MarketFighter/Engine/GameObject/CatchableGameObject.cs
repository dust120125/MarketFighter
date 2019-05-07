using MarketFighter.Engine.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public abstract class CatchableGameObject : GameObject, ICatchableGameObject
    {

        public const double RADIAN = Math.PI / 180;

        public ICatcherGameObject Catcher { get; protected set; }

        public bool Inertia { get; set; } = true;

        public bool Catched { get; protected set; }

        public bool Catchable { get; set; } = true;

        public bool FollowCatcher { get; protected set; } = true;

        public bool DuplicateCatch { get; set; }

        public double CatchOffsetX { get; protected set; }

        public double CatchOffsetY { get; protected set; }

        public double ReleaseSpeed;

        public CatchableGameObject() : this(null)
        {

        }

        public CatchableGameObject(string id) : base(id)
        {

        }

        public virtual ICatchableGameObject Catch(ICatcherGameObject catcher)
        {
            if (!Catchable) return null;
            if (Catched && !DuplicateCatch) return null;
            if (Catcher != null) Catcher.Release();
            SetSpeed(0, 0);
            Catcher = catcher;
            CatchOffsetX = X - catcher.X;
            CatchOffsetY = Y - catcher.Y;
            Catched = true;
            return this;
        }

        public virtual void Release(ICatcherGameObject catcher)
        {
            Catched = false;
            if (Inertia)
            {
                double angle = (Math.Atan(ActualSpeedY / ActualSpeedX) / RADIAN);
                if (ActualSpeedX < 0) angle += 180;
                if (Double.IsNaN(angle)) angle = 0;
                Direction = angle;
                ReleaseSpeed = Math.Sqrt(Math.Pow(ActualSpeedX, 2) + Math.Pow(ActualSpeedY, 2));
            }

            SetSpeed(ReleaseSpeed, Direction);
            Catcher = null;
        }

        public abstract bool InRange(IGameObject gameObject, double range);
    }
}
