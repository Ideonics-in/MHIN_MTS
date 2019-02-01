using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FromLine
    {
        public int FromLineID { get; set; }

        public DateTime Timestamp { get; set; }
        public int? Quantity { get; set; }


        public virtual ToLine ToLineRecord { get; set; }

        public virtual Part Part{get;set;}
    }
}
