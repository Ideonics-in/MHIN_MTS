using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FromStores
    {
        public int FromStoresID { get; set; }

        public String SUN { get; set; }
        public DateTime Timestamp { get; set; }
        public int? Quantity { get; set; }
        public int? Balance { get; set; }

        public virtual Part Part { get; set; }

       
        public virtual ICollection<FromStoresToLines> FromStoresToLines { get; set; }
    }
}
