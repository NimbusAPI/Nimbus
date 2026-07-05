namespace Nimbus.Tests.Integration.Configuration
{
    public class RabbitMqSettings
    {
        public string Host { get; set; }
        public int Port { get; set; } = 5672;
        public int ManagementPort { get; set; } = 15672;
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
