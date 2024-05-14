using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.WeatherDTO
{
    public class WeatherForRead
    {
        public string Location { get; set; }
        public DateTime Time { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal WindSpeed { get; set; }
        public string Description { get; set; }
    }
}
