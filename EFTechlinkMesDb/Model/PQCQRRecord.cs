namespace EFTechlinkMesDb.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ProcessHistory.PQCQRRecord")]
    public partial class PQCQRRecord
    {
        [Key]
        [Column(Order = 0)]
        public long Id { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(255)]
        public string QR { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "datetime2")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? EndDate { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OutputQty { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NGQty { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RWQty { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TotalQty { get; set; }

        [Key]
        [Column(Order = 7, TypeName = "datetime2")]
        public DateTime LastUpdated { get; set; }

        [StringLength(10)]
        public string Line { get; set; }

        [StringLength(50)]
        public string TL01 { get; set; }

        [StringLength(50)]
        public string TL02 { get; set; }

        [Key]
        [Column(Order = 8)]
        public DateTimeOffset LastModifiedDateTimeOffset { get; set; }

        [Key]
        [Column(Order = 9)]
        [StringLength(50)]
        public string LastModifiedUser { get; set; }

        [Key]
        [Column(Order = 10, TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] VersionNumber { get; set; }
    }
}
