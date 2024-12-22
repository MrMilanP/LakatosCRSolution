using LakatosCardReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LakatosCardReader.Interfaces
{
    public interface ILVehicleCardReader
    {
        LVehicleCardReadResult ReadVechileCardData(string readerName);

        Task<LVehicleCardReadResult> ReadVechileCardDataAsync(string readerName);
    }
}
