using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using LakatosCardReader.Models;

namespace LakatosCardReader.Interfaces
{
    public interface ILIdentityCardReader 
    {


        // Metoda za čitanje lične karte
        LIdentityCardReadResult ReadIdentityCardData(string readerName);

        Task<LIdentityCardReadResult> ReadIdentityCardDataAsync(string readerName);


    }
}