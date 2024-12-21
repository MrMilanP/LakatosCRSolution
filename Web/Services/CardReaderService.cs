using LakatosCardReader.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PCSC.Exceptions;
using PCSC.Monitoring;
using Web.Hubs;


using System.Threading.Tasks;
using LakatosCardReader.Models;

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

        private async void OnCardInserted(object? sender, CardStatusEventArgs e)
        {
            var cardType = _cardReader.GetCardType(e.ReaderName).ToString();
            await _hubContext.Clients.All.SendAsync("ReceiveCardType", cardType);
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

        public void Dispose()
        {
            _cardReader.Dispose();
        }
    }
}
