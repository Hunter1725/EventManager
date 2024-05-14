using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Weather
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public decimal Temperature { get; set; }

        [Required]
        public decimal Humidity { get; set; }

        [Required]
        public decimal WindSpeed { get; set; }

        [Required]
        public string Description { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }
    }
}
