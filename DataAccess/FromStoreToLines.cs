using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FromStoresToLines
    {
        [Key, Column(Order = 0)]
        public int FromStoresID { get; set; }

        [Key, Column(Order = 1)]
        public int ToLineID { get; set; }

        [Key, Column(Order = 2)]
        public int PartID { get; set; }

        public virtual FromStores FromStores { get; set; }
        public virtual ToLine ToLine{ get; set; }
        public virtual Part Part { get; set; }

        public int FromStoresBalance { get; set; }
        public int ToLineBalance { get; set; }
    }
}
