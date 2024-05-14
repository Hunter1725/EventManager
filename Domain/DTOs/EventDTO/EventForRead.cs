using Domain.DTOs.WeatherDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.EventDTO
{
    public class EventForRead
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime Time { get; set; }
        public int UserId { get; set; }
        public string OwnerEmail { get; set; }
        public List<WeatherForRead> WeatherInfos { get; set; }
    }
}
