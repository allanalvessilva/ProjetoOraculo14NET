using StackExchange.Redis;
using System;

namespace TrabalhoOraculoRedis
{
    class Program
    {
        static void Main(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();

            var sub = redis.GetSubscriber();
            sub.Subscribe("PERGUNTAS", (ch, msg) =>
            {
                PrepareUrl(msg, db);
            });

            Console.ReadLine();
        }

        public static void PrepareUrl(string question, IDatabase db)
        {
            string url = "https://www.google.com/search?q=+%20";

            string[] aux = question.Split(":");

            url += aux[1].Replace(" ", "+").Replace("?", "");

            url = url + "%3F";

            HashEntry[] resp = new HashEntry[]
            {
                new HashEntry("ALR", url)
            };

            db.HashSet(aux[0], resp);

            Console.WriteLine(url);
        }
    }
}
