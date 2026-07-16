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
        Embers = 1, //no fuel, cold
        Coals = 2, //no fuel, hot glowing coals
        Dwindling = 3, //some fuel, fire is barely going
        Steady = 4, //fueled, fire is going steady
        Raging = 5, //lots of fuel, fire is burning extremely hot
    }
}
