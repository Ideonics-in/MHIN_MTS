using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FGLog
    {
        public int FGLogID { get; set; }
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }

        public virtual FGPart FGPart {get;set;}
        
    }
}
