using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FG
    {
        public int FGID { get; set; }

        [StringLength(100)]
        public String FGRef { get; set; }

        public int? Quantity { get; set; }
        public DateTime? EntryTimestamp { get; set; }
        public int? JulianDate { get; set; }
        public int? Shift { get; set; }


        public virtual FGPart FGPart { get; set; }
        public virtual Part Part { get; set; }
        
        public virtual ICollection<ToLine> ToLines { get; set; }

    }
}
