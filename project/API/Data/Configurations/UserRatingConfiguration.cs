using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class UserRatingConfiguration : IEntityTypeConfiguration<UserRating>
    {
        public void Configure(EntityTypeBuilder<UserRating> builder)
        {
            builder.ToTable("UserRating");

            builder.HasKey(ur => ur.Id);

            builder.Property(ur => ur.Rate)
                .IsRequired();

            builder.Property(ur => ur.Description)
                .HasMaxLength(300);

            builder.Property(ur => ur.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now() at time zone 'utc'");

            builder.HasOne(ur => ur.Reporter)
                .WithMany()
                .HasForeignKey(ur => ur.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ur => ur.RatedUser)
                .WithMany()
                .HasForeignKey(ur => ur.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(ur => ur.Event)
                .WithMany()
                .HasForeignKey(ur => ur.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ur => new { ur.ReporterId, ur.RatedUserId, ur.EventId })
                .IsUnique()
                .HasDatabaseName("IX_UserRate_Reporter_Rated_Event");

            builder.HasCheckConstraint(
                "CK_UserRate_NoSelfRate",
                "\"ReporterId\" <> \"RatedUserId\"");
        }
    }
}