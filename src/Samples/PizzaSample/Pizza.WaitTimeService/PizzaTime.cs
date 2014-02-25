using System;

namespace Pizza.WaitTimeService
{
    public class PizzaTime
    {
        public DateTime OrderRecieved { get; set; }
        public DateTime? PizzaCooked { get; set; }
    }
}