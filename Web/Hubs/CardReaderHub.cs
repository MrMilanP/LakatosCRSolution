using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace Web.Hubs
{
    public class CardReaderHub : Hub
    {
        // Ova klasa služi kao SignalR hub za emitovanje događaja vezanih za čitač kartica svim povezanim klijentima.
        // Kontekst logika je implementirana u CardReaderService.
    }
}
