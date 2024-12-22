using PCSC.Exceptions;
using PCSC.Monitoring;
using System;

namespace LakatosCardReader.Interfaces
{
    public interface ILCardMonitor: IDisposable
    {
         event EventHandler<StatusChangeEventArgs>? StatusChanged;
        event EventHandler<CardStatusEventArgs>? CardInserted;
        event EventHandler<CardStatusEventArgs>? CardRemoved;
        event EventHandler<PCSCException>? MonitorException;

        void StartMonitoring(string[] readerNames);
        void StartMonitoring(string readerName);
        void StopMonitoring();


        Task StartAsync(string[] readerNames);

        Task StartAsync(string readerName);

        Task StopAsync();

        string[] GetReaders();

        bool IsStarted();

    }
}