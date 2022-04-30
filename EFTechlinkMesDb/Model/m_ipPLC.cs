namespace EFTechlinkMesDb.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class m_ipPLC
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string factory { get; set; }

        [StringLength(50)]
        public string process { get; set; }

        [Required]
        [StringLength(5)]
        public string line { get; set; }

        [Required]
        [StringLength(200)]
        public string modelPLC { get; set; }

        [Key]
        [StringLength(15)]
        public string IPPLC { get; set; }

        public bool isactive { get; set; }

        public DateTime datetimeRST { get; set; }

        [StringLength(10)]
        public string MQCversion { get; set; }
    }
}
