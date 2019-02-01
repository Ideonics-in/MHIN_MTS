using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ToLine
    {
        public ToLine()
        {
            Quantity = 0;
            
        }
        public int ToLineID { get; set; }

        public String SMN { get; set; }
        public DateTime Timestamp { get; set; }
        public int? Quantity { get; set; }
        public String SUN { get; set; }
        public virtual Part Part { get; set; }
        public int? Balance { get; set; }
        
        public virtual ICollection<FromStoresToLines> FromStoresToLines { get; set; }
        public virtual ICollection<LineRejection> LineRejections { get; set; }

        public virtual ICollection<FG> FGs { get; set; }
    }

    
}
