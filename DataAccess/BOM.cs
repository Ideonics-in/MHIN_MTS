using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class BOM
    {
      
        public int PartID { get; set; }
        public int FGPartID{ get; set; }
        public double? PartQuantity { get; set; }
        public bool Optional { get; set; }

    }
}
