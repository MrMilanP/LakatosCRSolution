using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using PCSC;
using System.Diagnostics;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

       // private readonly ICardService _cardService;


        private readonly CardReaderService _cardReaderService;
        private readonly ILCardMonitor _cardMonitor;

        public HomeController(ILogger<HomeController> logger,/* ICardService cardService,*/ CardReaderService cardReaderService, ILCardMonitor cardMonitor)
        {
            _logger = logger;
           // _cardService = cardService;
            _cardReaderService = cardReaderService;
            _cardMonitor = cardMonitor;

        }

        public IActionResult Index()
        {           // Dobijanje liste ?ita?a
            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
            {
                var _readers = context.GetReaders();
                ViewBag.Readers = _readers;
            }

            var readers = _cardMonitor.GetReaders();
            return View(readers);
           // return View();
        }




        [HttpPost]
        public IActionResult ReadIdentityCard(string readerName)
        {
            LIdentityCardReadResult readResult = _cardReaderService.ReadIdentityCard(readerName);

            if (readResult.Success && readResult.IdentityCardData != null)
            {
                // Serializujemo podatke o identifikacionoj kartici u JSON
                var response = new
                {
                    success = true,
                    data = new
                    {
                        document = readResult.IdentityCardData.Document,
                        fixedPersonal = readResult.IdentityCardData.FixedPersonal,
                        variablePersonal = readResult.IdentityCardData.VariablePersonal,
                        portraitBytes = readResult.IdentityCardData.PortraitBytes
                    }
                };
                return Json(response);
            }
            else
            {
                // Vraćamo grešku u JSON formatu
                return Json(new
                {
                    success = false,
                    error = readResult.ErrorMessage,
                    availableReaders = GetAvailableReaders() // Metoda za dobijanje dostupnih čitača
                });
            }
        }

        [HttpPost]
        public IActionResult ReadVehicleCard(string readerName)
        {
            LVehicleCardReadResult readResult = _cardReaderService.ReadVehicleCard(readerName);

            if (readResult.Success && readResult.VehicleCardData != null)
            {
                // Serializujemo podatke o saobracajnoj kartici u JSON
                var response = new
                {
                    success = true,
                    data = new
                    {
                        document = readResult.VehicleCardData.Document,
                        vehicle = readResult.VehicleCardData.Vehicle,
                        personal = readResult.VehicleCardData.Personal
                    }
                };
                return Json(response);
            }
            else
            {
                // Vraćamo grešku u JSON formatu
                return Json(new
                {
                    success = false,
                    error = readResult.ErrorMessage,
                    availableReaders = GetAvailableReaders() // Metoda za dobijanje dostupnih čitača
                });
            }
        }
        //Asinhrona metoda u kontroleru
        [HttpPost]
        public async Task<IActionResult> AReadVehicleCardAsync(string readerName)
        {
            LVehicleCardReadResult readResult = await _cardReaderService.GetVehicleDataAsync(readerName);

            if (readResult.Success && readResult.VehicleCardData != null)
            {
                // Serializujemo podatke o saobracajnoj kartici u JSON
                var response = new
                {
                    success = true,
                    data = new
                    {
                        document = readResult.VehicleCardData.Document,
                        vehicle = readResult.VehicleCardData.Vehicle,
                        personal = readResult.VehicleCardData.Personal
                    }
                };
                return Json(response);
            }
            else
            {
                // Vraćamo grešku u JSON formatu
                return Json(new
                {
                    success = false,
                    error = readResult.ErrorMessage,
                    availableReaders = GetAvailableReaders() // Metoda za dobijanje dostupnih čitača
                });
            }
        }

        //Asinhrona metoda u kontroleru
        [HttpPost]
        public async Task<IActionResult> AReadIdentityCardAsync(string readerName)
        {
            LIdentityCardReadResult readResult = await _cardReaderService.GetIdentityDataAsync(readerName);

            if (readResult.Success && readResult.IdentityCardData != null)
            {
                // Serializujemo podatke o licnoj kartici u JSON
                var response = new
                {
                    success = true,
                    data = new
                    {
                        document = readResult.IdentityCardData.Document,
                        fixedPersonal = readResult.IdentityCardData.FixedPersonal,
                        variablePersonal = readResult.IdentityCardData.VariablePersonal,
                        portraitBytes = readResult.IdentityCardData.PortraitBytes
                    }
                };
                return Json(response);
            }
            else
            {
                // Vraćamo grešku u JSON formatu
                return Json(new
                {
                    success = false,
                    error = readResult.ErrorMessage,
                    availableReaders = GetAvailableReaders() // Metoda za dobijanje dostupnih čitača
                });
            }
        }

        // Pomoćna metoda za dobijanje dostupnih čitača
        private List<string> GetAvailableReaders()
        {
            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
            {
                return context.GetReaders().ToList();
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        [HttpPost]
        public IActionResult StopMonitoringReader(string selectedReader)
        {
            _cardReaderService.StopMonitoring();
            return Json(new { success = true, monitoring = false });
        }

        // Opcionalno: Akcija za prikaz detalja kartice
        public IActionResult CardDetails(string type)
        {
            // Implementirajte logiku za čitanje i prikaz podataka na osnovu tipa kartice
            return View();
        }

        //asinhrone metode
        [HttpPost]
        public async Task<IActionResult> StartAsyncMonitoringReader(string selectedReader)
        {
            await Task.Run(() => _cardReaderService.StartAsync(selectedReader));
            return Json(new { success = true, monitoring = true });
        }

        [HttpPost]
        public async Task<IActionResult> StopAsyncMonitoringReader(string selectedReader)
        {
            await Task.Run(() => _cardReaderService.StopAsync());
            return Json(new { success = true, monitoring = false });
        }

    }
}
