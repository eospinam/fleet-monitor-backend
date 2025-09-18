namespace FleetMonitor.Core.Models {
    public class Device {
        public Guid Id { get; set; }
        public string DeviceIdentifier { get; set; }
        public double TankCapacityLiters { get; set; }

        public string MaskedIdentifier => Mask(DeviceIdentifier);
        static string Mask(string id) {
            if (string.IsNullOrEmpty(id) || id.Length < 6) return "****";
            return id.Substring(0,4) + "-" + "****" + "-" + id.Substring(id.Length-4);
        }
    }
}
