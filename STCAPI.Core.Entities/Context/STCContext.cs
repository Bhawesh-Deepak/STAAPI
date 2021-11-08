﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using STCAPI.Core.Entities.InvoiceDetails;
using STCAPI.Core.Entities.Reconcilation;
using STCAPI.Core.Entities.Report;
using STCAPI.Core.Entities.RequestDetail;
using STCAPI.Core.Entities.STCVAT;
using STCAPI.Core.Entities.UserManagement;
using STCAPI.Core.Entities.VATDetailUpload;

namespace STCAPI.Core.Entities.Context
{
    public class STCContext:DbContext
    {
        private readonly string _connectionString;

        public STCContext() { }
        public STCContext(IConfiguration configuration) {
            _connectionString = configuration.GetSection("ConnectionStrings:DefaultConnection").Value;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(_connectionString,ServerVersion.AutoDetect(_connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DemoConfiguration());
            modelBuilder.ApplyConfiguration(new STCVATConfiguration());
        }
         
        public virtual DbSet<STCVATForm> STCVATForms { get; set; }
        public virtual DbSet<STCPostValidation> STCPostValidations { get; set; }
        public virtual DbSet<StageModel> StageModels { get; set; }
        public virtual DbSet<MainLevel> MainLevels { get; set; }
        public virtual DbSet<StreamModel> StreamModels { get; set; }
        public virtual DbSet<RecincilationSummary> RecincilationSummaries { get; set; }
        public virtual DbSet<InputVATDataFile> InputVATDataFiles { get; set; }
        public virtual DbSet<VATTrailBalanceModel> VATTrailBalanceModels { get; set; }
        public virtual DbSet<STCVATOutputModel> STCVATOutputModels { get; set; }

        public virtual DbSet<VATReturnModel> VATReturnModels { get; set; }
        public virtual DbSet<UploadInvoiceDetail> UploadInvoiceDetails { get; set; }
        public virtual DbSet<RequestDetailModel> RequestDetailModels { get; set; }
        public virtual DbSet<PortalMenuMaster> PortalMenuMasters { get; set; }
        public virtual DbSet<UserManagement.PortalAccess> PortalAccesses { get; set; }
        public virtual DbSet<UploadInvoiceDetails> UploadInvoiceDetailses { get; set; }
        public virtual DbSet<NewReportModel> NewReportModels { get; set; }
       public virtual DbSet<MainStreamModel> MainStreamModels { get; set; }
    }
}
