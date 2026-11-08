using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class UserReportConfiguration
        : IEntityTypeConfiguration<UserReport>
    {
        public void Configure(
            EntityTypeBuilder<UserReport> builder)
        {
            builder.ToTable("UserReports");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Reason)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.CustomReason)
                .HasMaxLength(200);

            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql(
                    "now() at time zone 'utc'");

            builder.HasOne(r => r.Reporter)
                .WithMany()
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ReportedUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Event)
                .WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(r => new { r.ReporterId, r.ReportedUserId, r.EventId, r.Reason })
                .IsUnique()
                .HasDatabaseName("IX_UserReports_Reporter_Reported_Event_Reason");

            builder.HasCheckConstraint(
                "CK_UserReport_NoSelfReport",
                "\"ReporterId\" <> \"ReportedUserId\"");
        }
    }
}
