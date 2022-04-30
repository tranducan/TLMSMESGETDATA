using EFTechlinkMesDb.Model;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace EFTechlinkMesDb
{
    public partial class EFTechLinkMESModel : DbContext
    {
        public EFTechLinkMESModel()
            : base("name=EFTechLinkMESModel")
        {
        }

        public virtual DbSet<m_ipPLC> m_ipPLC { get; set; }

        public virtual DbSet<EntryPQCMesData> EntryPQCMesDatas { get; set; }
        public virtual DbSet<PQCMesData> PQCMesDatas { get; set; }
        public virtual DbSet<PQCQRRecord> PQCQRRecords { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PQCMesData>()
                .Property(e => e.Quantity)
                .HasPrecision(6, 0);

            modelBuilder.Entity<m_ipPLC>()
                .Property(e => e.MQCversion)
                .IsUnicode(false);
        }
    }
}
