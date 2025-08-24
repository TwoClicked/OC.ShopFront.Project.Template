using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Orders
{
    public class ShippingZone
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;   // e.g. "EU Zone", "Rest of World"

        public decimal BaseCost { get; set; }              // e.g. €5
        public decimal FreeShippingThreshold { get; set; } // e.g. €50

        public bool IsDefault { get; set; } = false;       // fallback if no zone match

        // Countries in this zone
        public List<ShippingZoneCountry> Countries { get; set; } = new();
    }
}
