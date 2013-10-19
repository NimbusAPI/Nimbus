using System;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace Numbus.ConsoleRunner
{

    public class SimpleSenderReceiver
    {
        private static string connectionString;
        private static string entityName;
        private static Boolean runReceiver = true;
        private MessagingFactory factory;
        private MessageSender sender;
        private MessageReceiver receiver;
        private MessageListener messageListener;
        private Thread listenerThread;

        static void Main(string[] args)
        {
            try
            {
                if ((args.Length > 0) && args[0].ToLower().Equals("sendonly"))
                    runReceiver = false;

                string ConnectionStringKey = "Microsoft.ServiceBus.ConnectionString";
                string entityNameKey = "EntityName";
                entityName = ConfigurationManager.AppSettings[entityNameKey];
                connectionString = ConfigurationManager.AppSettings[ConnectionStringKey];
                SimpleSenderReceiver simpleSenderReceiver = new SimpleSenderReceiver();

                Console.WriteLine("Press [enter] to send a message. " +
                    "Type 'exit' + [enter] to quit.");

                while (true)
                {
                    string s = Console.ReadLine();
                    if (s.ToLower().Equals("exit"))
                    {
                        simpleSenderReceiver.Close();
                        System.Environment.Exit(0);
                    }
                    else
                        simpleSenderReceiver.SendMessage();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception: " + e.Message);
            }
        }

        public SimpleSenderReceiver()
        {
            factory = MessagingFactory.CreateFromConnectionString(connectionString);
            sender = factory.CreateMessageSender(entityName);

            if (runReceiver)
            {
                receiver = factory.CreateMessageReceiver(entityName);
                messageListener = new MessageListener(receiver);
                listenerThread = new Thread(messageListener.Listen);
                listenerThread.Start();
            }
        }

        public void Close()
        {
            messageListener.RequestStop();
            listenerThread.Join();
            factory.Close();
        }

        private void SendMessage()
        {
            var message =
                new BrokeredMessage("Test AMQP message from .NET");
            sender.Send(message);
            Console.WriteLine("Sent message with MessageID = "
                + message.MessageId);
        }

    }
}
