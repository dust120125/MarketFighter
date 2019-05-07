using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public interface ICatcherGameObject : IGameObject
    {

        bool Catched { get; }

        bool Catch(ICatchableGameObject catchable);

        void Release();

    }
}
