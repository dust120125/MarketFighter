using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.Engine.GameObject
{
    public class MoveTask
    {

        public double DestX;
        public double DestY;
        public double Speed;
        public Action Callback;
        public bool Force;

    }
}
