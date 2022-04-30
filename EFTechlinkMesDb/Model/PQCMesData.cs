namespace EFTechlinkMesDb
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ProcessHistory.PQCMesData")]
    public partial class PQCMesData
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long PQCMesDataId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string POCode { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public string LotNumber { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string Model { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string Site { get; set; }

        [Key]
        [Column(Order = 5)]
        [StringLength(20)]
        public string Line { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(10)]
        public string Process { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(20)]
        public string Attribute { get; set; }

        [Key]
        [Column(Order = 8)]
        [StringLength(20)]
        public string AttributeType { get; set; }

        [Key]
        [Column(Order = 9)]
        public decimal Quantity { get; set; }

        [StringLength(10)]
        public string Flag { get; set; }

        [StringLength(20)]
        public string Inspector { get; set; }

        [Key]
        [Column(Order = 10, TypeName = "datetime2")]
        public DateTime InspectDateTime { get; set; }
    }
}
