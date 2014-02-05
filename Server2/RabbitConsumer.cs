﻿using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.MessagePatterns;


namespace Server2
{
    public class RabbitConsumer
    {
        private const string HostName = "localhost";
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string QueueName = "Server2.Sample.Queue";
        private const bool IsDurable = true;
        //The two below settings are just to illustrate how they can be used but we are not using them in
        //this sample as we will use the defaults
        private const string VirtualHost = "";
        private int Port = 0;

        public delegate void OnReceiveMessage(string message);

        public bool Enabled { get; set; }

        private ConnectionFactory c_connectionFactory;
        private IConnection c_connection;
        private IModel c_model;
        private Subscription c_subscription;


        public RabbitConsumer()
        {
            this.DisplaySettings();
            this.c_connectionFactory = new ConnectionFactory
            {
                HostName = HostName,
                UserName = UserName,
                Password = Password
            };

            if (string.IsNullOrEmpty(VirtualHost) == false)
                this.c_connectionFactory.VirtualHost = VirtualHost;
            if (Port > 0)
                this.c_connectionFactory.Port = Port;

            this.c_connection = this.c_connectionFactory.CreateConnection();
            this.c_model = this.c_connection.CreateModel();
            this.c_model.BasicQos(0, 1, false);
        }
        

        private void DisplaySettings()
        {
            Console.WriteLine("Host: {0}", HostName);
            Console.WriteLine("Username: {0}", UserName);
            Console.WriteLine("Password: {0}", Password);
            Console.WriteLine("QueueName: {0}", QueueName);
            Console.WriteLine("VirtualHost: {0}", VirtualHost);
            Console.WriteLine("Port: {0}", Port);
            Console.WriteLine("Is Durable: {0}", IsDurable);
        }
        

        public void Start()
        {
            this.c_subscription = new Subscription(this.c_model, QueueName, false);

            var _consumer = new ConsumeDelegate(Poll);
            _consumer.Invoke();
        }


        private delegate void ConsumeDelegate();


        private void Poll()
        {
            while (Enabled)
            {
                //Get next message
                var _deliveryArgs = this.c_subscription.Next();
                //Deserialize message
                var _message = Encoding.Default.GetString(_deliveryArgs.Body);

                //Handle Message
                Console.WriteLine("Message Recieved - {0}", _message);

                //Acknowledge message is processed
                this.c_subscription.Ack(_deliveryArgs);
            }
        }
        

        public void Dispose()
        {
            if (this.c_model != null)
                this.c_model.Dispose();
            if (this.c_connection != null)
                this.c_connection.Dispose();

            this.c_connectionFactory = null;

            GC.SuppressFinalize(this);
        }
    }
}