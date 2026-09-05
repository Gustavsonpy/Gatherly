using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Data.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.DateTime)
                .IsRequired();

            builder.Property(e => e.Localization) // Ver se tem que ser outra tabela
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.MaxCapacity)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.City) // Ver se tem que ser outra tabela
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(e => e.Level) // Ver se tem que ser outra tabela
                .HasMaxLength(200);

            builder.Property(e => e.UrlImage)
                .IsRequired();

            builder.Property(e => e.RegisterDate)
                .IsRequired()
                .HasDefaultValueSql("now() at time zone 'utc'");

            //Relationship
            builder.HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserId);

            builder.HasIndex(e => e.UserId);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId);

            builder.HasIndex(e => e.CategoryId);
        }
    }
}