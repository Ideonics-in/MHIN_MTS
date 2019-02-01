using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class InventoryLog
    {
        public int InventoryLogID { get; set; }
        public String Message { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual Part Part { get; set; }
    }
}
