namespace EFTechlinkMesDb.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Logging.EntryPQCMesData")]
    public partial class EntryPQCMesData
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string MessageTemplate { get; set; }

        [StringLength(128)]
        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Exception { get; set; }

        [Column(TypeName = "xml")]
        public string Properties { get; set; }
    }
}
