using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("metricCalculateWeek")]
    public class MetricWeek
    {
        public MetricWeek()
        {
            this.Unit = "";
        }

        public MetricWeek(int deviceRepresentation, int numberSave, double somme, double average, string unit, long time)
        {
            this.DeviceRepresentation = deviceRepresentation;
            this.NumberSave = numberSave;
            this.Somme = somme;
            this.Average = average;
            this.Unit = unit;
            this.Time = time;
        }

        [Key]
        [Required]
        [Column("id")]
        public long Id { get; set; }

        [Column("device_representation")]
        public int DeviceRepresentation { get; set; }

        [Column("number_save")]
        public int NumberSave { get; set; }

        [Column("somme")]
        public double Somme { get; set; }

        [Column("average")]
        public double Average { get; set; }

        [Column("unit")]
        public string Unit { get; set; }

        [Column("time")]
        public long Time { get; set; } 
    }    
}