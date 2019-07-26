using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;

public class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static List<King> kings;

    //Gets JSON data and stores in List "kings"
    private static async Task ProcessRepositories()
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var streamTask = client.GetStreamAsync("http://mysafeinfo.com/api/data?list=englishmonarchs&format=json&token=DVLtWor4q7Q1jlVbBHJa0cueaZnDlkvY");
        var serializer = new DataContractJsonSerializer(typeof(List<King>));
        kings = serializer.ReadObject(await streamTask) as List<King>;

        
    }
    public class King
    {
        public int id { get; set; }
        public string nm { get; set; }
        public string cty { get; set; }
        public string hse { get; set; }
        public string yrs { get; set; }
        public int RuledYears //Property returns number of ruled years
        {
            get {
                string[] years = yrs.Split('-');
                if (years.Length == 2 && years[1] == "")
                    years[1] = DateTime.Now.ToString("yyyy");
                if (years.Length == 2)
                    return Int32.Parse(years[1]) - Int32.Parse(years[0]);
                return 0;
            }
        }
        public string FirstName //Property returns first name
        {
            get
            {
                return nm.Split(" ")[0];              
            }
        }
    }

    public static void Main()
    {
        //Calling to get the data about monarchs
        ProcessRepositories().Wait();

        //Prints how many monarchs are there in the list 
        Console.WriteLine("1) There are " + kings.Count + " monarchs in the list");

        //Prints which monarch ruled the longest (and for how long)
        var result2 = kings.OrderByDescending(x => x.RuledYears).First();   
        Console.WriteLine("2) " + result2.nm + " ruled " + result2.RuledYears + " years");

        //Prints Which house ruled the longest (and for how long)
        var result3 = (kings.GroupBy(x => x.hse)
        .Select(x => new { hse = x.Key, RuledYears = x.Sum(y => y.RuledYears) }).ToList())
        .OrderByDescending(x => x.RuledYears).First();
        Console.WriteLine("3) " + result3.hse + " ruled for " + result3.RuledYears + " years");

        //Prints What was the most common first name?
        var result4 = kings.GroupBy(x => x.FirstName)
        .Select(x => new { FirstName = x.Key, Count = x.Count()})
        .OrderByDescending(x => x.Count).First();
        Console.WriteLine("4) " + result4.FirstName + " is most common first name");
    }
}