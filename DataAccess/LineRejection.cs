using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class LineRejection
    {
        public int LineRejectionID {get;set;}
        public DateTime Timestamp { get; set; }
        public int? Quantity { get; set; }


        public String Reason { get; set; }
        public virtual ICollection<ToLine> ToLines { get; set; }

        public virtual Part Part { get; set; }

        public LineRejection()
        {
            Quantity = 0;
            ToLines = new List<ToLine>();
        }
    }
}
