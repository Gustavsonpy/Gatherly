using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.Birthday)
                .IsRequired();

            builder.Property(u => u.HashPassword)
                .IsRequired();

            builder.Property(u => u.Description)
                .HasMaxLength(500);

            builder.Property(u => u.City)
                .HasMaxLength(100);

            builder.Property(u => u.UrlPhoto)
                .HasMaxLength(2048);

            builder.Property(u => u.IsSuspended)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.IsAdmin)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(u => u.RegisterDate)
                .IsRequired()
                .HasDefaultValueSql("now() at time zone 'utc'");
        }
    }
}