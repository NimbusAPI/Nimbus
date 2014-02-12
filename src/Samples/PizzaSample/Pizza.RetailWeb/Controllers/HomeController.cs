using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nimbus;
using Pizza.Ordering.Messages;
using Pizza.RetailWeb.Models.Home;
using Pizza.RetailWeb.ReadModels;

namespace Pizza.RetailWeb.Controllers
{
    public class HomeController : AsyncController
    {
        private readonly IBus _bus;
        private readonly PizzaOrderStatusReadModel _orderStatusReadModel;

        public HomeController(IBus bus, PizzaOrderStatusReadModel orderStatusReadModel)
        {
            _bus = bus;
            _orderStatusReadModel = orderStatusReadModel;
        }

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> PlaceOrder(string customerName)
        {
            await _bus.Send(new OrderPizzaCommand {CustomerName = customerName});

            return Redirect("/");
        }

        public PartialViewResult OrderAPizzaWidget()
        {
            return PartialView();
        }

        public async Task<PartialViewResult> AverageWaitTimeWidget()
        {
            var averageWaitTime = await _bus.Request(new HowLongDoPizzasTakeRequest());

            var viewModel = new AverageWaitTimeWidgetViewModel
                            {
                                AverageWaitTime = averageWaitTime.Minutes,
                            };

            return PartialView(viewModel);
        }

        public async Task<PartialViewResult> OrderStatusWidget()
        {
            var viewModel = new OrderStatusWidgetViewModel
                            {
                                Orders = _orderStatusReadModel
                                    .Orders
                                    .OrderByDescending(o => o.Ordered)
                                    .ToArray(),
                            };
            return PartialView(viewModel);
        }
    }
}