using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketFighter.GO
{
    public interface ICommodity
    {

        bool InCart { get; }

        bool PutIntoCart(ShoppingCart cart);

    }
}
