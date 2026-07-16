using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Enums
{
    public enum FireFuelItemState
    {

        None = 0,
        Ash = 1, //item is burnt out, ashes, can be culled
        Embers = 2, //item is reduced to burning as embers
        Coals = 3, //item is reduced to burning as coals and is still hot
        Burning = 4, //item is burning
        Heating = 5, //item is catching fire
        Cold = 6, //item is freshly added to the fire or not currently burning

    }
}
