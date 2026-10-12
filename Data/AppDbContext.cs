using Land_plans_Report.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace Land_plans_Report.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Signature> Signatures => Set<Signature>();
        //--------------------------------------------------------------
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<ReportPlanDetails> ReportPlanDetails => Set<ReportPlanDetails>();
        public DbSet<ReportPlotDetails> ReportPlotDetails => Set<ReportPlotDetails>();
        public DbSet<ReportRawLandDetails> ReportRawLandDetails => Set<ReportRawLandDetails>();
        public DbSet<ReportUnplannedLandDetails> ReportUnplannedLandDetails => Set<ReportUnplannedLandDetails>();
        //----------------------------------------------------------------------------------------------------------
        public DbSet<ReportPhoto> ReportPhotos => Set<ReportPhoto>();// ممكن يحذف
        public DbSet<ReportApproval> ReportApprovals => Set<ReportApproval>();
        public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();
        // ممكن يحذف

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // كل الجداول أسماؤها بقاعدة البيانات مطابقة تماماً لأسماء DbSet هنا،
            // ما عدا AuditLogEntry (اسم الكلاس مختلف عن اسم الجدول الفعلي AuditLog)
            modelBuilder.Entity<AuditLogEntry>().ToTable("AuditLog");

            // مفاتيح مركبة (لا يوجد عمود Id مفرد بهذي الجداول)


            // علاقات 1-إلى-1 بين Reports وجداول التفاصيل (المفتاح الأساسي = المفتاح الأجنبي بنفس الوقت)
            modelBuilder.Entity<Report>()
                .HasOne(r => r.PlanDetails)
                .WithOne(d => d.Report!)
                .HasForeignKey<ReportPlanDetails>(d => d.ReportId);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.PlotDetails)
                .WithOne(d => d.Report!)
                .HasForeignKey<ReportPlotDetails>(d => d.ReportId);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.RawLandDetails)
                .WithOne(d => d.Report!)
                .HasForeignKey<ReportRawLandDetails>(d => d.ReportId);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.UnplannedLandDetails)
                .WithOne(d => d.Report!)
                .HasForeignKey<ReportUnplannedLandDetails>(d => d.ReportId);

            // دقة الأعمدة العشرية (لازم تطابق DECIMAL بقاعدة البيانات وإلا EF يقص الأرقام)
            modelBuilder.Entity<Report>().Property(r => r.Latitude).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<Report>().Property(r => r.Longitude).HasColumnType("decimal(9,6)");
            modelBuilder.Entity<ReportPlanDetails>().Property(d => d.Area).HasColumnType("decimal(12,2)");
            modelBuilder.Entity<ReportRawLandDetails>().Property(d => d.Area).HasColumnType("decimal(12,2)");
            modelBuilder.Entity<ReportUnplannedLandDetails>().Property(d => d.Area).HasColumnType("decimal(12,2)");
            modelBuilder.Entity<ReportPlanDetails>().Property(d => d.DistanceFromCenter).HasColumnType("decimal(6,2)");
            modelBuilder.Entity<ReportRawLandDetails>().Property(d => d.DistanceFromCenter).HasColumnType("decimal(6,2)");
            modelBuilder.Entity<ReportUnplannedLandDetails>().Property(d => d.DistanceFromCenter).HasColumnType("decimal(6,2)");
        }
    }
}
