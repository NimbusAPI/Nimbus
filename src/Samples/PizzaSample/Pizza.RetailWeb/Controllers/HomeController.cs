using System.Threading.Tasks;
using System.Web.Mvc;
using Nimbus;
using Pizza.Ordering.Messages;
using Pizza.RetailWeb.Models.Home;

namespace Pizza.RetailWeb.Controllers
{
    public class HomeController : AsyncController
    {
        private readonly IBus _bus;

        public HomeController(IBus bus)
        {
            _bus = bus;
        }

        public async Task<ActionResult> Index()
        {
            var averageWaitTime = await _bus.Request(new HowLongDoPizzasTakeRequest());
            var model = new IndexViewModel
                        {
                            AverageWaitTime = averageWaitTime.Minutes,
                        };
            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}