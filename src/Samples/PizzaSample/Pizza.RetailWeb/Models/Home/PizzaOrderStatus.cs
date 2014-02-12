using System;

namespace Pizza.RetailWeb.Models.Home
{
    public class PizzaOrderStatus
    {
        public string CustomerName { get; set; }
        public DateTimeOffset Ordered { get; set; }
        public DateTimeOffset? Ready { get; set; }
    }
}