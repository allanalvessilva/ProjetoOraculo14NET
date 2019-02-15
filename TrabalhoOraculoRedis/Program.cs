using StackExchange.Redis;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace TrabalhoOraculoRedis
{
    class Program
    {
        const string url = "https://www.google.com.br/search?q=";

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();


        }

        async static Task MainAsync(string[] args)
        {
            var redis = ConnectionMultiplexer.Connect("40.122.106.36");
            IDatabase db = redis.GetDatabase();

            var sub = redis.GetSubscriber();
            sub.Subscribe("perguntas", (ch, msg) =>
            {
                PrepareUrl(msg, db);
            });

            Console.ReadLine();
        }

        public static async void PrepareUrl(string question, IDatabase db)
        {
            string resposta = string.Empty;

            string url = "https://www.google.com/search?q=+%20";

            string[] aux = question.Split(":");

            url += aux[1].Replace(" ", "+").Replace("?", "");

            url = url + "%3F";

            resposta = await BuscaNoGoogle(url);

            HashEntry[] resp = new HashEntry[]
            {
                new HashEntry("ALR", resposta)
            };

            db.HashSet(aux[0], resp);

            Console.ReadLine();
        }

        public async static Task<string> BuscaNoGoogle(string url)
        {
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var node = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='FSP1Dd']");

            if (node == null)
            {
                node = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='KpMaL']");
            }

            if (node == null)
            {
                node = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='cwos']");
            }

            if (node == null)
            {
                return "Se fossem com laranjas eu saberia";
            }

            return node.InnerHtml;
        }
    }
}
