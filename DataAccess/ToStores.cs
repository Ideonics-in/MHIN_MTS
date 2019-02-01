using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ToStores
    {
        public int ToStoresID { get; set; }

        public String SUN { get; set; }
        public DateTime Timestamp { get; set; }
        public int? Quantity { get; set; }
        
        public bool Reject_Reuse { get; set; } 
        public String Reason { get; set; }

        public virtual Part Part { get; set; }

        
    }
}
