using System.Threading;
using Nimbus.SampleApp.MessageContracts;

namespace Nimbus.SampleApp
{
    public class Heartbeat
    {
        private readonly IBus _bus;
        private Timer _timer;

        public Heartbeat(IBus bus)
        {
            _bus = bus;
        }

        public void Run()
        {
            var callback = new TimerCallback(Beat);
            _timer = new Timer(callback, null, 0, 5000);
        }

        private void Beat(object state)
        {
            _bus.Publish(new HeartbeatEvent());
        }
    }
}