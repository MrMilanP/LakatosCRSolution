using LakatosCardReader.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PCSC.Exceptions;
using PCSC.Monitoring;
using Web.Hubs;


using System.Threading.Tasks;
using LakatosCardReader.Models;
using static LakatosCardReader.Models.LCardTypeModel;

namespace Web.Services
{
    // Servis za upravljanje čitačem kartica i emitovanje događaja putem SignalR huba.
    public class CardReaderService : IDisposable
    {
        private readonly ILCardReader _cardReader;
        private readonly IHubContext<CardReaderHub> _hubContext;

        private readonly ILIdentityCardReader _identityCardReader;

        private readonly ILVehicleCardReader _vehicleCardReader;

        public CardReaderService(ILCardReader cardReader, IHubContext<CardReaderHub> hubContext, ILIdentityCardReader identityCardReader, ILVehicleCardReader vehicleCardReader)
        {
            _cardReader = cardReader;
            _hubContext = hubContext;

            _identityCardReader = identityCardReader;
            _vehicleCardReader = vehicleCardReader;
            // Pretplata na događaje
            _cardReader.CardInserted += OnCardInserted;
            _cardReader.CardRemoved += OnCardRemoved;
            _cardReader.MonitorException += OnMonitorException;
        }

        public LIdentityCardReadResult ReadIdentityCard(string readerName)
        {
            return _identityCardReader.ReadIdentityCardData(readerName);
        }

        public LVehicleCardReadResult ReadVehicleCard(string readerName)
        {
            return _vehicleCardReader.ReadVechileCardData(readerName);
        }

        //Asinhroni poziv za čitanje lične karte
        public async Task<LIdentityCardReadResult> GetIdentityDataAsync(string readerName)
        {
            return await _identityCardReader.ReadIdentityCardDataAsync(readerName);
        }

        //Asinhroni poziv za čitanje saobracajne dozvole
        public async Task<LVehicleCardReadResult> GetVehicleDataAsync(string readerName)
        {
            return await _vehicleCardReader.ReadVechileCardDataAsync(readerName);
        }

        //private async void OnCardInserted(object? sender, CardStatusEventArgs e)
        //{
        //    //var cardType = _cardReader.GetCardType(e.ReaderName).ToString();

        //    //prelazimo na asinhroni poziv
        //    var cardType = _cardReader.GetCardTypeAsync(e.ReaderName);
        //    await _hubContext.Clients.All.SendAsync("ReceiveCardType", cardType.Result.ToString());
        //}
        private async void OnCardInserted(object? sender, CardStatusEventArgs e)
        {
            try
            {
                // Pokreni asinhronu operaciju dobijanja tipa kartice
                Task<CardType> cardTypeTask = _cardReader.GetCardTypeAsync(e.ReaderName);

                // Sačekaj da se operacija završi
                CardType cardType = await cardTypeTask;

                // Pošalji poruku SignalR klijentima (asinhrono)
                await _hubContext.Clients.All.SendAsync("ReceiveCardType", cardType.ToString());
            }
            catch (Exception ex)
            {
                // Obrada greške - pošalji poruku o grešci i/ili loguj
                await _hubContext.Clients.All.SendAsync("Error", ex.Message);
                Console.WriteLine($"Greška prilikom obrade događaja CardInserted: {ex.Message}");
            }
        }
        private async void OnCardRemoved(object? sender, CardStatusEventArgs e)
        {
            await _hubContext.Clients.All.SendAsync("CardRemoved", e.ReaderName);
        }

        private async void OnMonitorException(object? sender, PCSCException e)
        {
            await _hubContext.Clients.All.SendAsync("MonitorException", e.Message);
        }

        public void StartMonitoring(string selectedReader)
        {
            _cardReader.Start(selectedReader);
        }



        public void StopMonitoring()
        {
            _cardReader.Stop();
        }

        //asinhrone metode
        public async void StartAsync(string readerName)
        {
            await _cardReader.StartAsync(readerName);
        }

        public async void StopAsync()
        {
            await _cardReader.StopAsync();
        }

        public void Dispose()
        {
            _cardReader.Dispose();
        }
    }
}
