using System;

namespace DevExpressBlazorExtensions.Data
{
    public class WeatherForecast
    {
        public WeatherForecast()
        {
            Id = GetHashCode();
        }
        
        public int Id { get; set; }
        
        public DateTime Date { get; set; }

        public string TemperatureC { get; set; }
        
        public string Summary { get; set; }
    }
}