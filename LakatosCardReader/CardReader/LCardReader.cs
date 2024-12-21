using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;
using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using System.Linq;
using System.Text;
using static LakatosCardReader.Models.LCardTypeModel;


namespace LakatosCardReader.CardReader
{
    public class LCardReader : ILCardReader, IDisposable
    {
        private readonly ILCardMonitor _monitor;
        private readonly ILCardTypeParser _parser;
        //private readonly ILIdentityCardReader _identityCardReader;
        //private readonly IVehicleCardReader _vehicleCardReader;


        public event EventHandler<CardStatusEventArgs>? CardInserted;
        public event EventHandler<CardStatusEventArgs>? CardRemoved;
        public event EventHandler<PCSCException>? MonitorException;
        //public event EventHandler<CardTypeDetectedEventArgs>? CardTypeDetected;

        public LCardReader(ILCardMonitor monitor, ILCardTypeParser parser/*, ILIdentityCardReader identityCardReader, IVehicleCardReader vehicleCardReader*/)
        {
            _monitor = monitor;
            _parser = parser;
           // _identityCardReader = identityCardReader;
           // _vehicleCardReader = vehicleCardReader;
            _monitor.CardInserted += (sender, args) => CardInserted?.Invoke(this, args);
            _monitor.CardRemoved += (sender, args) => CardRemoved?.Invoke(this, args);
            _monitor.MonitorException += (sender, ex) => MonitorException?.Invoke(this, ex);
        }

        public  void Dispose()
        {
            _monitor.CardInserted -= (sender, args) => CardInserted?.Invoke(this, args);
            _monitor.CardRemoved -= (sender, args) => CardRemoved?.Invoke(this, args);
            _monitor.MonitorException -= (sender, ex) => MonitorException?.Invoke(this, ex);
            _monitor.Dispose();
        }

        public void Start()
        {
            var readerNames = _monitor.GetReaders();
            if (readerNames.Length == 0)
            {
                // Nema dostupnih čitača
                return;
            }

            _monitor.StartMonitoring(readerNames);
        }


        public void Start(string readerName)
        {
            _monitor.StartMonitoring(readerName);
        }

        public void Stop()
        {
            _monitor.StopMonitoring();
        }

        public bool IsStarted()
        {
            return _monitor.IsStarted();
        }

        //private void OnCardInsertedHandler(object sender, CardStatusEventArgs e)
        //{
        //    CardInserted?.Invoke(this, e);
        //    DetectCardType(e.ReaderName);
        //}
        public CardType GetCardType(string readerName)
        {
            // Povežimo sve u jednu metodu
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    var status = reader.GetStatus();
                    var atr = status.GetAtr();

                    // Dobićemo dictionary od parsera, npr: { "GEMALTO_ATR_1": [IdCardDocument, VehicleDocument] }
                    var dict = _parser.GetCardType(atr);
                    if (dict.Count == 0)
                    {
                        return CardType.Unknown;
                    }

                    // U našem slučaju je dict uvek 1 key -> 1 lista, pa možemo:
                    var kvp = dict.First();
                    var possibleTypes = kvp.Value; // List<CardType>

                    if (possibleTypes.Count == 0)
                    {
                        return CardType.Unknown;
                    }
                    if (possibleTypes.Count == 1)
                    {
                        return possibleTypes[0];
                    }
                    else
                    {
                        // Više tipova, npr. IdCard i Vehicle
                        // Testiramo redom
                        foreach (var t in possibleTypes)
                        {
                            if (TryConfirmType(reader, t))
                            {
                                return t;
                            }
                        }
                        return CardType.Unknown;
                    }
                }
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                return CardType.Unknown;
            }
        }

        // Ova metoda šalje komande ka kartici da proba inicijalizaciju.
        private bool TryConfirmType(ICardReader reader, CardType type)
        {
            switch (type)
            {
                case CardType.IdCardDocument:
                    return CardTypeTesterHelper.TestIdCard(reader);
                case CardType.VehicleDocument:
                    return CardTypeTesterHelper.TestVehicleCard(reader);
                case CardType.MedicalDocument:
                    return CardTypeTesterHelper.TestMedicalCard(reader);
                default:
                    return false;
            }
        }


    }
}