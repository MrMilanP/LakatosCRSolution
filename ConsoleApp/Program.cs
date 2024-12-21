using System;
using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Parsers;
using LakatosCardReader.CardReader;
using LakatosCardReader.Utils;

using PCSC.Exceptions;
using PCSC.Monitoring;
using PCSC;
using SixLabors.ImageSharp;
using System.Formats.Tar;
using static LakatosCardReader.Models.LCardTypeModel;

namespace LakatosCardReader.ConsoleApp
{
    class Program
    {
        // Reference na specifične čitače kartica
        private static ILIdentityCardReader? _identityCardReader;
        private static ILVehicleCardReader? _vehicleCardReader;
        private static ILCardReader? _cardReader;
        static void Main(string[] args)
        {
            // Inicijalizacija komponenti
            ILCardMonitor cardMonitor = new LCardMonitor();
            ILCardTypeParser cardTypeParser = new LCardTypeParser();
            _cardReader = new LCardReader(cardMonitor, cardTypeParser);
            ILIdentityDataParser identityCardParser = new LIdentityCardParser();
            ILVehicleDataParser vehicleCardParser = new LVehicleCardParser();
            _identityCardReader = new LIdentityCardReader( identityCardParser, _cardReader);
            _vehicleCardReader = new LVehicleCardReader(_cardReader);



            // Pretplata na događaje
            _cardReader.CardInserted += OnCardInserted;
            _cardReader.CardRemoved += OnCardRemoved;
            _cardReader.MonitorException += OnMonitorException;


            // Dohvat dostupnih čitača
            string[] readers = cardMonitor.GetReaders();
            if (readers.Length == 0)
            {
                Console.WriteLine("Nema dostupnih čitača kartica.");
                return;
            }


            // Prikaz dostupnih čitača
            Console.WriteLine("Dostupni čitači kartica:");
            for (int i = 0; i < readers.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {readers[i]}");
            }


            Console.Write("Izaberite čitač (unesite broj): ");
            if (!int.TryParse(Console.ReadLine(), out int selectedIndex) ||
                selectedIndex < 1 || selectedIndex > readers.Length)
            {
                Console.WriteLine("Nevažeći izbor.");
                return;
            }

            string selectedReader = readers[selectedIndex - 1];
            Console.WriteLine($"Izabrani čitač: {selectedReader}");

            // Startovanje monitoringa
            _cardReader.Start(selectedReader);

            Console.WriteLine("Čeka se umetanje kartice. Pritisnite Enter za izlaz...");
            Console.ReadLine();

            // Zaustavljanje monitoringa
            _cardReader.Stop();


            _cardReader?.Dispose();


            Console.WriteLine("Monitoring zaustavljen. Aplikacija se zatvara.");
        }

        private static void OnCardInserted(object? sender, CardStatusEventArgs e)
        {
            Console.WriteLine($"Kartica je umetnuta u čitač: {e.ReaderName}");

            if (_cardReader == null)
            {
                Console.WriteLine("Greška: _cardReader nije inicijalizovan.");
                return;
            }



            // Detekcija tipa kartice koristeći LCardReader
            CardType cardType = _cardReader.GetCardType(e.ReaderName);
            Console.WriteLine($"Detektovan tip kartice u čitaču {e.ReaderName}: {cardType}");

           


            // Delegiranje zadatka čitanja na odgovarajuću klasu
            switch (cardType)
            {
                case CardType.IdCardDocument:
                    // Čitanje ID kartice koristeći LIdentityCardReader


                    if (_identityCardReader == null)
                    {
                        Console.WriteLine("Greška: _identityCardReader nije inicijalizovan.");
                        break;
                    }

                    LIdentityCardReadResult readResult = _identityCardReader.ReadIdentityCardData(e.ReaderName);
                    if (readResult.Success && readResult.IdentityCardData != null)
                    {
                        DisplayIdentityDocumentData(readResult.IdentityCardData);

                        DisplayIdentityPersonalData(readResult.IdentityCardData);

                        DisplayIdentityVariableSata(readResult.IdentityCardData);
                    }
                    else
                    {
                        Console.WriteLine($"Greška pri čitanju ID kartice: {readResult.ErrorMessage}");
                    }
                    break;

                case CardType.VehicleDocument:
                    // Čitanje vozne kartice koristeći LVechicleCardReader
                    try
                    {

                        if (_vehicleCardReader == null)
                        {
                            Console.WriteLine("Greška: _vehicleCardReader nije inicijalizovan.");
                            break;
                        }

                        LVehicleCardReadResult vehicleReadResult = _vehicleCardReader.ReadVechileCardData(e.ReaderName);
                        if (vehicleReadResult.Success && vehicleReadResult.VehicleCardData != null)
                        {

                            DisplayVehicleDocumentData(vehicleReadResult.VehicleCardData);

                            DisplayVehicleVehicleData(vehicleReadResult.VehicleCardData);

                            DisplayVehiclePersonalData(vehicleReadResult.VehicleCardData);
                        }
                       else
                       {
                           Console.WriteLine($"Greška pri čitanju vozne kartice: {vehicleReadResult.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška pri čitanju vozne kartice: {ex.Message}");
                    }
                    break;


                case CardType.MedicalDocument:
                    // Implementiraj sličnu logiku za Medical kartice kada budeš imao MedicalCardReader klasu
                    Console.WriteLine("Medical kartica detektovana. To-do čitanje podataka.");
                    break;

                default:
                    Console.WriteLine("Nepoznat tip kartice.");
                    break;
            }
        }

        private static void OnCardRemoved(object? sender, CardStatusEventArgs e)
        {
            Console.WriteLine($"Kartica je uklonjena iz čitača: {e.ReaderName}");
        }

        private static void OnMonitorException(object? sender, PCSCException e)
        {
            Console.WriteLine($"Greška u monitoru: {e.Message}");
        }






        private static void DisplayIdentityDocumentData(LIdentityCardModel doc)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== PODACI IZ KARTICE =====");
            Console.WriteLine($"Document Serial Number: {doc.Document.DocumentSerialNumber}");
            Console.WriteLine($"Issuing Authority: {doc.Document.IssuingAuthority}");
            Console.WriteLine($"Document Name: {doc.Document.DocumentName}");
            Console.WriteLine($"Datum izdavanja: {doc.Document.IssuingDate}");
            Console.WriteLine($"Datum isteka: {doc.Document.ExpiryDate}");


        }
        private static void DisplayIdentityPersonalData(LIdentityCardModel doc)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== LIČNI PODACI =====");
            Console.WriteLine($"Personal Number: {doc.FixedPersonal.PersonalNumber}");
            Console.WriteLine($"Surname: {doc.FixedPersonal.Surname}");
            Console.WriteLine($"Given Name: {doc.FixedPersonal.GivenName}");
            Console.WriteLine($"Parent Given Name: {doc.FixedPersonal.ParentGivenName}");

            // Opcionalno: Sačuvajte fotografiju ako postoji
            if (doc.Portrait != null)
            {
                string photoPath = "portrait.jpg";
                doc.Portrait.Save(photoPath);
                Console.WriteLine($"Fotografija sačuvana u '{photoPath}'.");
            }
        }

        private static void DisplayIdentityVariableSata(LIdentityCardModel doc)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== PODACI IZ KARTICE =====");
            Console.WriteLine($"State: {doc.VariablePersonal.State}");
            Console.WriteLine($"Community: {doc.VariablePersonal.Community}");
            Console.WriteLine($"Place: {doc.VariablePersonal.Place}");
        }

        private static void DisplayVehicleDocumentData(LVehicleCardModel doc)
        {
            

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== PODACI O DOKUMENTU =====");
            Console.WriteLine($"State Issuing: {doc.Document.StateIssuing}");
            Console.WriteLine($"Competent Authority: {doc.Document.CompetentAuthority}");
            Console.WriteLine($"Authority Issuing: {doc.Document.AuthorityIssuing}");
            Console.WriteLine($"Unambiguous Number: {doc.Document.UnambiguousNumber}");
            Console.WriteLine($"Serial Number: {doc.Document.SerialNumber}");
            Console.WriteLine($"Expiry Date: {doc.Document.ExpiryDate}");
        }


        private static void DisplayVehicleVehicleData(LVehicleCardModel vehicle)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== PODACI O VOZILU =====");
            Console.WriteLine($"Date Of First Registration: {vehicle.Vehicle.DateOfFirstRegistration}");
            Console.WriteLine($"Vehicle Category: {vehicle.Vehicle.VehicleCategory}");
            Console.WriteLine($"Number of Axles: {vehicle.Vehicle.NumberOfAxles}");
            Console.WriteLine($"Vehicle Load: {vehicle.Vehicle.VehicleLoad}");
            Console.WriteLine($"Year of Production: {vehicle.Vehicle.YearOfProduction}");
            Console.WriteLine($"Engine ID Number: {vehicle.Vehicle.EngineIdNumber}");
            Console.WriteLine($"Type Approval Number: {vehicle.Vehicle.TypeApprovalNumber}");
            Console.WriteLine($"Power Weight Ratio: {vehicle.Vehicle.PowerWeightRatio}");
            Console.WriteLine($"Vehicle Make: {vehicle.Vehicle.VehicleMake}");
            Console.WriteLine($"Vehicle Type: {vehicle.Vehicle.VehicleType}");
            Console.WriteLine($"Commercial Description: {vehicle.Vehicle.CommercialDescription}");
            Console.WriteLine($"Maximum Permissible Laden Mass: {vehicle.Vehicle.MaximumPermissibleLadenMass}");
            Console.WriteLine($"Engine Capacity: {vehicle.Vehicle.EngineCapacity}");
            Console.WriteLine($"Maximum Net Power: {vehicle.Vehicle.MaximumNetPower}");
            Console.WriteLine($"Type of Fuel: {vehicle.Vehicle.TypeOfFuel}");
            Console.WriteLine($"Number of Seats: {vehicle.Vehicle.NumberOfSeats}");
            Console.WriteLine($"Number of Standing Places: {vehicle.Vehicle.NumberOfStandingPlaces}");
            Console.WriteLine($"Colour of Vehicle: {vehicle.Vehicle.ColourOfVehicle}");
        }

        private static void DisplayVehiclePersonalData(LVehicleCardModel personal)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===== LIČNI PODACI =====");
            Console.WriteLine($"User's Personal No: {personal.Personal.UsersPersonalNo}");
            Console.WriteLine($"Owner's Personal No: {personal.Personal.OwnersPersonalNo}");
            Console.WriteLine($"Owner's Surname or Business Name: {personal.Personal.OwnersSurnameOrBusinessName}");
            Console.WriteLine($"Owner Name: {personal.Personal.OwnerName}");
            Console.WriteLine($"Owner Address: {personal.Personal.OwnerAddress}");
            Console.WriteLine($"User's Surname or Business Name: {personal.Personal.UsersSurnameOrBusinessName}");
            Console.WriteLine($"User Name: {personal.Personal.UsersName}");
            Console.WriteLine($"User Address: {personal.Personal.UsersAddress}");
        }


    }
}
