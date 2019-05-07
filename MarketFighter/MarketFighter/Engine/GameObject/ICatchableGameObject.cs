using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public interface ICatchableGameObject : IGameObject
    {

        ICatcherGameObject Catcher { get; }

        bool Catchable { get; }

        bool Catched { get; }

        bool FollowCatcher { get; }

        bool DuplicateCatch{ get; }

        double CatchOffsetX { get; }

        double CatchOffsetY { get; }

        bool InRange(IGameObject gameObject, double range);

        ICatchableGameObject Catch(ICatcherGameObject catcher);

        void Release(ICatcherGameObject catcher);

    }
}
