using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;



namespace baiduGetLinks
{    
    class Program
    {
        static List<string> queries = new List<string>();
        static List<infoLinkOne> Links = new List<infoLinkOne>();
        static int ThrNum = 5; // кол-во потоков при парсинге тубов
        static void Main(string[] args)
        {
            if (File.Exists(@"result.txt"))
            {
                File.Delete(@"result.txt");
            }
            // загрузка запросов из файла
            if (!completeQueriList()) { return; }
            Console.WriteLine("Запросов в файле: " + queries.Count);
            
            // перечисление запросов
            foreach(string zapros in queries)
            {
                v_ku6_com vBaidu_com = new v_ku6_com(zapros, ThrNum);
                Links = mergeLists(Links, vBaidu_com.getLinks());
                
                //
            }
            

            using (StreamWriter file = new StreamWriter(@"result.txt"))
            {
                foreach (infoLinkOne linka in Links)
                {
                    file.WriteLine(linka.url + "5544" + linka.domain);
                }
            }

            Console.ReadKey();
        }
        static bool completeQueriList()
        {
            string line;
            try
            {
                StreamReader file = new StreamReader(@"queries.txt");
                while ((line = file.ReadLine()) != null)
                {
                    queries.Add(line.Replace("&", " "));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка при получении запросов из файла " + ex.Message);
                Console.ReadKey();
                return false;
            }
            Console.WriteLine("Запросы из файла загружены");
            return true;
        }
        static List<infoLinkOne> mergeLists(List<infoLinkOne> oneList, List<infoLinkOne> twoList)
        {
            foreach (infoLinkOne ilink in twoList)
            {
                oneList.Add(ilink);
            }
            return oneList;
        }
    }
}
