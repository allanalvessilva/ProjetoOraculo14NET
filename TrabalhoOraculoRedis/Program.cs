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
            //var pergunta = "quanto é 2 + 2?";
            //var pergunta = "Qual é a capital do brasil?".Replace(" ","+");
            //var pergunta = "Quem descobriu o brasil?";
            // nesse caso nao retiorna nada, poderia procurar no wikipedia, se for omlink dele, clicar e fazer o scrapper da pagina do wikipedia
            //var pergunta = "Quantas luas tem em saturno?";



            // ------------------------------

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
            //question = "pq: quanto é 1500 + 1500?";
            string asd = string.Empty;

            //if (question.Contains("+"))
            //{
            //    asd = PerguntaComSoma(question);
            //}
            //else
            //{


                string url = "https://www.google.com/search?q=+%20";

                string[] aux = question.Split(":");

                url += aux[1].Replace(" ", "+").Replace("?", "");

                url = url + "%3F";



                asd = await BuscaNoGoogle(url);
            //}
            HashEntry[] resp = new HashEntry[]
            {
                new HashEntry("ALR", asd)
            };

            db.HashSet(aux[0], resp);

            //Console.WriteLine(url);
            //Console.WriteLine(asd);
            Console.ReadLine();
        }

        //private static string PerguntaComSoma(string question)
        //{
        //    int number1;
        //    int number2;

        //    if (int.TryParse(question.Substring(question.IndexOf('+'), (question.IndexOf('?') - question.IndexOf('+'))), out number1))
        //    {
        //        if (true)
        //        {

        //        }
        //    }
            

        //}

        public async static Task<string> BuscaNoGoogle(string url)
        {
            var httpClient = new HttpClient();

            var html = await httpClient.GetStringAsync(url);

            //Console.WriteLine(html);

            //Console.ReadLine();

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
