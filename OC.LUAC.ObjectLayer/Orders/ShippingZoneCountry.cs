using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Orders
{
    public class ShippingZoneCountry
    {
        public int Id { get; set; }
        public int ShippingZoneId { get; set; }
        public ShippingZone ShippingZone { get; set; }

        public string CountryCode { get; set; } = string.Empty; // ISO Alpha-2, e.g. "DE", "US"

        // New: Friendly way of displaying the country based off the ISO codes
        public string CountryName {  get; set; } = string.Empty;
    }
}
