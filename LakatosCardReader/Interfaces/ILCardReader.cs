using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LakatosCardReader.Models.LCardTypeModel;

namespace LakatosCardReader.Interfaces
{
    public interface ILCardReader : IDisposable
    {
        event EventHandler<CardStatusEventArgs>? CardInserted;
        event EventHandler<CardStatusEventArgs>? CardRemoved;
        event EventHandler<PCSCException>? MonitorException;

        void Start();

        void Start(string readerName);

        void Stop();

        Task StartAsync();

        Task StartAsync(string readerName);

        Task StopAsync();

        CardType GetCardType(string readerName);

        Task<CardType> GetCardTypeAsync(string readerName);

        bool IsStarted();




    }
}
