using System;
using System.Threading.Tasks;
using System.Timers;
using Cafe.Messages;
using Nimbus.InfrastructureContracts;

namespace Cashier.Services
{
    public class CustomerOrderGenerator : IDisposable
    {
        private static readonly double _interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
        private static readonly string[] _customerNames = {"Damian", "Nick", "Andrew"};
        private static readonly string[] _coffeeOrders = {"Extra shot flat white", "Doppio", "Flat white"};

        private readonly IBus _bus;
        private readonly Random _random = new Random();

        private Timer _timer;

        public CustomerOrderGenerator(IBus bus)
        {
            _bus = bus;
        }

        public void Dispose()
        {
            _timer?.Stop();
        }

        public void Start()
        {
            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
            _timer.Interval = 1;
            _timer.Enabled = true;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var timer = (Timer) sender;
#pragma warning disable 4014
            PlaceFakeOrder();
#pragma warning restore 4014
            timer.Interval = _interval;
            timer.Enabled = true;
        }

        private async Task PlaceFakeOrder()
        {
            var customer = _customerNames[_random.Next(_customerNames.Length)];
            var coffee = _coffeeOrders[_random.Next(_coffeeOrders.Length)];
            var command = new PlaceOrderCommand(Guid.NewGuid(), customer, coffee);
            await _bus.Send(command);
        }
    }
}