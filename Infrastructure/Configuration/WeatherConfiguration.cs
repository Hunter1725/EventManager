using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class WeatherConfiguration : IEntityTypeConfiguration<Weather>
    {
        public void Configure(EntityTypeBuilder<Weather> builder)
        {
            builder.HasOne(w => w.Event)
                .WithMany(e => e.WeatherInfos)
                .HasForeignKey(w => w.EventId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
