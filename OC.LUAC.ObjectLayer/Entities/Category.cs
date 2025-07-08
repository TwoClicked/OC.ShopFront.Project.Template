using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OC.LUAC.ObjectLayer.Entities
{
    public class Category
    {

        public int id { get; set; }

        //Multi-language support
        public string Name_en { get; set; }
        public string Name_de { get; set; }

        //Soft delete support 
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        //Navigation properties

        public virtual ICollection<Product>? Products { get; set; }
    }
}
