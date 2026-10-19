using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
    {
        public void Configure(EntityTypeBuilder<EventParticipant> builder)
        {
            builder.ToTable("EventParticipants");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.RegisterDate)
                .IsRequired()
                .HasDefaultValueSql("now() at time zone 'utc'");

            builder.HasOne(p => p.User)
                .WithMany(u => u.EventParticipations)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Event)
                .WithMany(e => e.Participants)
                .HasForeignKey(p => p.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => new { p.UserId, p.EventId }).IsUnique();
            builder.HasIndex(p => p.EventId);
        }
    }
}