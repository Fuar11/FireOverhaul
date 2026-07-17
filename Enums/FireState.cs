using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Enums
{
    public enum FireState
    {

        None = 0, //fire is completely out
        Starting = 1, //fire is being started from nothing
        Embers = 2, //no fuel, cold
        Coals = 3, //no fuel, hot glowing coals
        Dwindling = 4, //some fuel, fire is barely going
        Steady = 5, //fueled, fire is going steady
        Raging = 6, //lots of fuel, fire is burning extremely hot
    }
}
