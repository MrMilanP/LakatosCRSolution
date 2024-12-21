using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LakatosCardReader.Models.LCardTypeModel;

namespace LakatosCardReader.Interfaces
{
    public interface ILCardTypeParser
    {
        Dictionary<string, List<CardType>> GetCardType(byte[] atr);
    }
}
