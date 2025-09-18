namespace FleetMonitor.Core.Models {
    public class Alert {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public double AutonomyHours { get; set; }
        public Device Device { get; set; }
    }
}
