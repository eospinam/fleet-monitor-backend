namespace FleetMonitor.Core.Models {
    public class Telemetry {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKph { get; set; }
        public double FuelLiters { get; set; }
        public double TemperatureC { get; set; }
        public Device Device { get; set; }
    }
}
