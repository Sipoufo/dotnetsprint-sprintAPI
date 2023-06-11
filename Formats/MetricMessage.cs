namespace Formats
{
    public class MetricMessage {
        public MetricMessage (int device_representation, int value, String unit)
        {
            this.device_representation = device_representation;
            this.value = value;
            this.unit = unit;
        }
        
        public int device_representation { get; set; }
        public int value { get; set; }
        public String? unit { get; set; }
    }
}