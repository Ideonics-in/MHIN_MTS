using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Part
    {
        public int PartID { get; set; }

        [StringLength(100)]
        [Index(IsUnique = true)]
        public String PartNo { get; set; }

        public String Description { get; set; }
        public String Location { get; set; }
        public int? Quantity{ get; set; }
        public int? Minimum { get; set; }
        public int? Maximum{ get; set; }
        public bool? Kanban { get; set; }
        public DateTime? LastUpdated { get; set; }
        public String LastActivity { get; set; }
        public int? ToLineReference { get; set; }

        [NotMapped]
        public String ImageReference { get; set; }

        [NotMapped]
        public int LineQuantity { get; set; }

        [NotMapped]
        public int RejectedQuantity { get; set; }


        public virtual ICollection<FromStores> FromStoresRecords { get; set; }
        public virtual ICollection<ToLine> ToLineRecords { get; set; }
        public virtual ICollection<FromLine> FromLineRecords { get; set; }
        public virtual ICollection<ToStores> ToStoresRecords { get; set; }
        public virtual ICollection<BOM> BOM{ get; set; }
        public virtual ICollection<FG> FGRecords { get; set; }
        public virtual ICollection<InventoryLog> Logs { get; set; }
        public virtual ICollection<LineRejection> LineRejections { get; set; }

        public void UpdateLineRejectionQuantity()
        {
            foreach (ToLine t in ToLineRecords)
                LineQuantity += t.Balance.Value;
            foreach (LineRejection r in LineRejections)
                RejectedQuantity += r.Quantity.Value;

        }
    }
}
