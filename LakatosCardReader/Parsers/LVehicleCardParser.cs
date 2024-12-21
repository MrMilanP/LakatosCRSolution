using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using System;


namespace LakatosCardReader.Parsers
{
    public class LVehicleCardParser : ILVehicleDataParser
    {
        // Parsiranje podataka o dokumentu
        public LVehicleCardModel.DocumentData ParseDocument(BER berData)
        {
            var doc = new LVehicleCardModel.DocumentData();
            string? tempValue = null;


            AssignValue(berData, ref tempValue, 0x71, 0x8D);

            FormatDateYMD(ref tempValue);

            doc.ExpiryDate = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x8E);

            FormatDateYMD(ref tempValue);

            doc.IssuingDate = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x9F33);
            doc.StateIssuing = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x9F35);
            doc.CompetentAuthority = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x9F36);
            doc.AuthorityIssuing = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x9F38);
            doc.UnambiguousNumber = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xC9);
            doc.SerialNumber = tempValue;


            return doc;
        }

        // Parsiranje podataka o vozilu
        public LVehicleCardModel.VehicleData ParseVehicle(BER berData)
        {
            var doc = new LVehicleCardModel.VehicleData();
            string? tempValue = null;

            AssignValue(berData, ref tempValue, 0x71, 0x82);

            FormatDateYMD(ref tempValue);

            doc.DateOfFirstRegistration = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0x98);
            doc.VehicleCategory = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0x99);
            doc.NumberOfAxles = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xC4);
            doc.VehicleLoad = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x8A);
            doc.VehicleIdNumber = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x81);
            doc.RegistrationNumberOfVehicle = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x8C);
            doc.VehicleMass = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xC5);
            doc.YearOfProduction = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xA5, 0x9E);
            doc.EngineIdNumber = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x8F);
            doc.TypeApprovalNumber = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0x93);
            doc.PowerWeightRatio = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA3, 0x87);
            doc.VehicleMake = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA3, 0x88);
            doc.VehicleType = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA3, 0x89);
            doc.CommercialDescription = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA4, 0x8B);
            doc.MaximumPermissibleLadenMass = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA5, 0x90);
            doc.EngineCapacity = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA5, 0x91);
            doc.MaximumNetPower = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA5, 0x92);
            doc.TypeOfFuel = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA6, 0x94);
            doc.NumberOfSeats = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA6, 0x95);
            doc.NumberOfStandingPlaces = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0x9F24);
            doc.ColourOfVehicle = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xC1);

            SpecFormatDate(ref tempValue);

            doc.RestrictionToChangeOwner = tempValue;

            return doc;
        }

        // Parsiranje ličnih podataka
        public LVehicleCardModel.PersonalData ParsePersonal(BER berData)
        {
            var doc = new LVehicleCardModel.PersonalData();
            string? tempValue = null;

            AssignValue(berData, ref tempValue, 0x72, 0xC3);
            doc.UsersPersonalNo = tempValue;

            AssignValue(berData, ref tempValue, 0x72, 0xC2);
            doc.OwnersPersonalNo = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA2, 0x83);
            doc.OwnersSurnameOrBusinessName = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA2, 0x84);
            doc.OwnerName = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA2, 0x85);
            doc.OwnerAddress = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA9, 0x83);
            doc.UsersSurnameOrBusinessName = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA9, 0x84);
            doc.UsersName = tempValue;

            AssignValue(berData, ref tempValue, 0x71, 0xA1, 0xA9, 0x85);
            doc.UsersAddress = tempValue;

            return doc;
        }

        // Pomoćna metoda za dodavanje vrednosti
        private void AssignValue(BER berData, ref string? tempValue, params uint[] tags)
        {
            tempValue = String.Empty;
            berData.AssignFrom(ref tempValue, tags);
        }

        private void FormatDateYMD(ref string? input)
        {
            if (string.IsNullOrEmpty(input) || input.Length != 8)
            {
                return;
            }

            // Formatiranje iz "YYYYMMDD" u "DD.MM.YYYY"
            input = $"{input.Substring(6, 2)}.{input.Substring(4, 2)}.{input.Substring(0, 4)}";
        }

        private static void SpecFormatDate(ref string? date)
        {
            if (!string.IsNullOrEmpty(date) && date.Length == 8)
            {
                try
                {
                    date = DateTime.ParseExact(date, "ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture)
                                   .ToString("dd.MM.yyyy");
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid date format. Expected format: ddMMyyyy");
                }
            }
            else
            {
                throw new ArgumentException("Input date must be 8 characters long and not null or empty.");
            }
        }
    }
}
