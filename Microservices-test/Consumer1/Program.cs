﻿using System;
using System.Threading;
using Infrastructure;
using Infrastructure.Brokers.RabbitMq;

namespace Consumer1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start c1");
            
            var rabbit = new RabbitMqClient
            {
                AutoAck = false
            };

            rabbit.SubscribeOnQueue((model, arg) =>
            {
                var message = arg.Body.TransformToString();
                
                Thread.Sleep(2000);
                Console.WriteLine($" [{message}] Done");
                
                rabbit.Ack(arg);
            });
        }
    }
}