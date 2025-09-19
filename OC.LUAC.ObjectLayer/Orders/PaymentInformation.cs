using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Orders
{
    public class PaymentInformation
    {
        public string AccountHolder { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
        public string Bank { get; set; }
    }
}
