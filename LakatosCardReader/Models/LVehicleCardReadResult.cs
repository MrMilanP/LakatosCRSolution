using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LakatosCardReader.Models
{
    public class LVehicleCardReadResult
    {
        public LVehicleCardModel? VehicleCardData { get; set; }
        public bool Success { get; set; } 
        public string? ErrorMessage { get; set; }

        public byte[][]? Files { get; set; }
    }
}
