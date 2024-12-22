using PCSC;
using PCSC.Monitoring;
using PCSC.Exceptions;
using System;
using LakatosCardReader.Interfaces;

namespace LakatosCardReader.CardReader
{
    public class LCardMonitor : ILCardMonitor, IDisposable
    {
        private readonly ISCardMonitor _monitor;
        public event EventHandler<StatusChangeEventArgs>? StatusChanged;
        public event EventHandler<CardStatusEventArgs>? CardInserted;
        public event EventHandler<CardStatusEventArgs>? CardRemoved;
        public event EventHandler<PCSCException>? MonitorException;

        public LCardMonitor()
        {
            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
            _monitor.StatusChanged += OnStatusChanged;
            _monitor.CardInserted += OnCardInserted;
            _monitor.CardRemoved += OnCardRemoved;
            _monitor.MonitorException += OnMonitorException;

        }



        public void Dispose()
        {
            _monitor.StatusChanged -= OnStatusChanged;
            _monitor.CardInserted -= OnCardInserted;
            _monitor.CardRemoved -= OnCardRemoved;
            _monitor.MonitorException -= OnMonitorException;
            _monitor.Dispose();
        }

        public void StartMonitoring(string[] readerNames)
        {
            try
            {
                foreach (var reader in readerNames)
                {
                    _monitor.Start(reader);
                }
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }


        public void StartMonitoring(string readerName)
        {
            try
            {
                _monitor.Start(readerName);           
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }

        public void StopMonitoring()
        {
            try
            {
                _monitor.Cancel();
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }


        //Asinhrone metode
        public async Task StartAsync(string[] readerNames)
        {
            try
            {
                foreach (var reader in readerNames)
                {
                    // Koristimo Task.Run da bi se Start poziv izvršio na posebnom thread-u iz Thread Pool-a
                    await Task.Run(() => _monitor.Start(reader));
                }
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }

        public async Task StartAsync(string readerName)
        {
            try
            {
                // Slično, koristimo Task.Run
                await Task.Run(() => _monitor.Start(readerName));
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }

        public async Task StopAsync()
        {
            try
            {
                // I ovde koristimo Task.Run
                await Task.Run(() => _monitor.Cancel());
            }
            catch (PCSCException ex)
            {
                MonitorException?.Invoke(this, ex);
                // Eventualno logovanje greške
            }
        }

        public bool IsStarted()
        {
            return _monitor.Monitoring;
        }

        public string[] GetReaders()
        {
            using var context = ContextFactory.Instance.Establish(SCardScope.System);
            return context.GetReaders();
        }

        private void OnStatusChanged(object sender, StatusChangeEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
            Console.WriteLine($"Reader: {e.ReaderName} LastState: {e.LastState}");
            Console.WriteLine($"Reader: {e.ReaderName} NewState: {e.NewState}");
        }

        //private void OnStatusChanged(object sender, CardStatusEventArgs e)
        //{
        //    StatusChanged?.Invoke(this, e);
        //}

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            CardInserted?.Invoke(this, e);
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            CardRemoved?.Invoke(this, e);
        }

        private void OnMonitorException(object sender, PCSCException ex)
        {
            MonitorException?.Invoke(this, ex);
        }
    }
}