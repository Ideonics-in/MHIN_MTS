using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class FGPart
    {
        public int FGPartID { get; set; }

        [StringLength(100)]
        [Index(IsUnique = true)]
        public String PartNo { get; set; }

        public String Description { get; set; }
        public String Location { get; set; }
        public int? PalletQuantity { get; set; }

        [NotMapped]
        public String ImageReference { get; set; }

        public virtual ICollection<BOM> BOM { get; set; }
        public virtual ICollection<FGLog> Logs { get; set; }
    }
}
