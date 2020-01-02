using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace baiduGetLinks
{    
    class v_ku6_com
    {
        private string kw;
        private int trnumb;
        private List<infoLinkOne> Links = new List<infoLinkOne>();
        private static int curPage = 1;

        //очередь адресов для закачки
        static Queue<string> URLs = new Queue<string>();
        //список скачанных страниц
        static List<string> HTMLs = new List<string>();
        //локер для очереди адресов
        static object URLlocker = new object();
        //локер для списка скачанных страниц
        static object HTMLlocker = new object();
        //очередь ошибок
        static Queue<Exception> exceptions = new Queue<Exception>();

        public v_ku6_com(string zapros, int threadsNum)
        {
            kw = zapros;
            trnumb = threadsNum; 
        }
        public List<infoLinkOne> getLinks()
        {
            //создаем массив хендлеров, для контроля завершения потоков
            ManualResetEvent[] handles = new ManualResetEvent[trnumb];
            //создаем и запускаем 3 потока
            for (int i = 0; i < trnumb; i++)
            {
                handles[i] = new ManualResetEvent(false);
                (new Thread(new ParameterizedThreadStart(Download))).Start(handles[i]);
            }
            //ожидаем, пока все потоки отработают
            WaitHandle.WaitAll(handles);
            //проверяем ошибки, если были - выводим
            foreach (Exception ex in exceptions)
                Console.WriteLine(ex.Message);
            //сохраняем закачанные страницы в файлы
            try
            {
                //for (int i = 0; i < HTMLs.Count; i++)
                    //File.WriteAllText("c:\\" + i + ".html", HTMLs[i]);
                //Console.WriteLine(HTMLs.Count + " files saved");
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            //
            Console.WriteLine("Download completed");
            
            curPage = 1;
            //Console.ReadLine();

            return Links;
        }

        private void Download(object handle)
        {
            //будем крутить цикл, пока не закончатся ULR в очереди
            
            bool nexpages = true;
            while (nexpages)
            {
                string URL = "http://so.ku6.com/search?q=" + kw + "&start=[curpage]";
                //блокируем очередь URL и достаем оттуда один адрес
                lock (URLlocker)
                {
                    /*if (URLs.Count == 0)
                        break;//адресов больше нет, выходим из метода, завершаем поток
                    else
                        URL = URLs.Dequeue();*/
                    URL = URL.Replace("[curpage]", curPage.ToString());
                    curPage++;
                }
                Console.WriteLine(URL + " - start downloading ...");
            lab1:
                try
                {
                    
                    //скачиваем страницу
                    WebRequest request = WebRequest.Create(URL);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string HTML = (new StreamReader(response.GetResponseStream())).ReadToEnd();
                    //блокируем список скачанных страниц, и заносим туда свою страницу
                    lock (HTMLlocker)
                    {
                        HTML = getTextBlock(HTML);
                        if (HTML.Length > 0)
                        {
                            // парсинг ссылок с блока
                            Regex newReg = new Regex("<h3><a href=\"(?<urlVid>.*?)from=my\" target=\"_blank\" title", RegexOptions.Singleline);
                            MatchCollection matches = newReg.Matches(HTML);

                            if (matches.Count > 0)
                            {
                                //Console.WriteLine("++++++++++++++++");
                                foreach (Match mat in matches)
                                {
                                    //textblock = mat.Groups["urlVid"].Value;
                                    //return textblock;
                                    infoLinkOne inf1 = new infoLinkOne();
                                    inf1.url = mat.Groups["urlVid"].Value + "from=my";
                                    inf1.domain = "v_ku6_com";
                                    Links.Add(inf1);
                                }
                            }
                        }
                        else
                        {
                            //continue;
                        }
                        //HTMLs.Add(HTML);
                        if (HTML.IndexOf("class=\"ckl_las\">下一页") != -1)
                        {
                            //
                        }
                        else
                        {
                            // next не найден
                            nexpages = false;
                        }
                    }
                    //
                    Console.WriteLine(URL + " - downloaded (" + HTML.Length + " bytes)");
                }
                catch (ThreadAbortException)
                {
                    //это исключение возникает если главный поток хочет завершить приложение
                    //просто выходим из цикла, и завершаем выполнение
                    break;
                }
                catch (Exception ex)
                {
                    //в процессе работы возникло исключение
                    //заносим ошибку в очередь ошибок, предварительно залочив ее
                    lock (exceptions)
                        exceptions.Enqueue(ex);
                    goto lab1;
                    //берем следующий URL
                    continue;
                }
            }
            //устанавливаем флажок хендла, что бы сообщить главному потоку о том, что мы отработали
            ((ManualResetEvent)handle).Set();
        }

        private string getTextBlock(string html)
        {
            string textblock = "";
            Regex newReg = new Regex("div class=\"ckl_cotcent cfix\"(?<text>.*?)<div class=\"fr ckl_cenright\"", RegexOptions.Singleline);
            MatchCollection matches = newReg.Matches(html);

            if (matches.Count > 0)
            {
                //Console.WriteLine("++++++++++++++++");
                foreach (Match mat in matches)
                {
                    textblock = mat.Groups["text"].Value;
                    return textblock;                    
                }
            }

            return "";
        }
    }
}
