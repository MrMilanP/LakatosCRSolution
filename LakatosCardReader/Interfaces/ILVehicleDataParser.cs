using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using System;

namespace LakatosCardReader.Interfaces
{
    public interface ILVehicleDataParser
    {
        LVehicleCardModel.DocumentData ParseDocument(BER berData); // Parsiranje podataka o dokumentu
        LVehicleCardModel.VehicleData ParseVehicle(BER berData);   // Parsiranje podataka o vozilu
        LVehicleCardModel.PersonalData ParsePersonal(BER berData); // Parsiranje ličnih podataka
    }
}

