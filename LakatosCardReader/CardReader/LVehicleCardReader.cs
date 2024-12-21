using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Parsers;
using LakatosCardReader.Utils;
using PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LakatosCardReader.CardReader
{
    public class LVehicleCardReader: ILVehicleCardReader
    {
        //private readonly ILVehicleDataParser<LVehicleCardModel> _parser;
        private readonly ILCardReader _cardReader;

        //public event EventHandler<CardStatusEventArgs>? CardInserted;
        //public event EventHandler<CardStatusEventArgs>? CardRemoved;
        //public event EventHandler<PCSCException>? MonitorException;

        public LVehicleCardReader(ILCardReader cardReader)
        {
            // _parser = parser;
            _cardReader = cardReader;
        }

        public LVehicleCardReadResult ReadVechileCardData(string readerName)
        {

            LVehicleCardReadResult result = new LVehicleCardReadResult();

            if (!_cardReader.IsStarted())
            {
                result.Success = false;
                result.ErrorMessage = "Card reader is not started.";
                return result;
            }
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    // Inicijalizacija Files kolekcije ako već nije inicijalizovana
                    if (result.Files == null)
                    {
                        result.Files = new byte[4][];
                    }

                    for (byte i = 0; i <= 3; i++)
                    {
                        try
                        {
                            // Kreiranje APDU komande za čitanje fajla
                            byte[] apduCommand = new byte[] { 0xD0, (byte)(i * 0x10 + 0x01) };

                            // Čitanje fajla sa kartice
                            byte[] fileData = VehicleHelper.VReadFile(reader, apduCommand);

                            // Čuvanje podataka u Files kolekciji
                            result.Files[i] = fileData;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Reading document {i} file: {ex.Message}", ex);
                        }
                    }

                    // Parsiranje i spajanje BER podataka
                    try
                    {
                        // Kreiranje instanci za parsiranje
                        BER data = new BER();

                        for (byte i = 0; i <= 3; i++)
                        {
                            try
                            {
                                // Parsiranje BER podataka iz fajla
                                BER parsed = BER.ParseBER(result.Files[i]);

                                // Spajanje parsiranih podataka u 'data'
                                data.Merge(parsed);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Parsing {i} file: {ex.Message}", ex);
                            }
                        }


                        LVehicleCardParser lVehicleCardParser = new LVehicleCardParser();
                        //Mapiranje spojenih BER podataka u LVehicleCardModel
                        LVehicleCardModel lVehicleCardModel = new LVehicleCardModel();

                      

                        lVehicleCardModel.Document = lVehicleCardParser.ParseDocument(data);
                        lVehicleCardModel.Personal  = lVehicleCardParser.ParsePersonal(data);
                        lVehicleCardModel.Vehicle  = lVehicleCardParser.ParseVehicle(data);

                        // Dodeljivanje dokumenta rezultatu
                        result.VehicleCardData = lVehicleCardModel;
                        result.Success = true;

                        return result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error processing vehicle card data: {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.Success = false;
                return result;
            }        
        }


    }
}
