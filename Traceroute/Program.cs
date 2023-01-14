using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Traceroute
{
    class Program
    {
        static bool d = false;
        static int maxTTL = 30;
        static int maxMS = 1000;
        static void Main(string[] args)
        {
            string sAppPath = Environment.CurrentDirectory;
            while (true)
            {
                Console.Write(sAppPath + @":\>");
                string tmp = Console.ReadLine();
                string[] command = Command(tmp);
                bool ok = ControllCommand(command);
                if (ok)
                    CommandReserch(command);
            }
        }

        public static void GetTraceRoute(string hostname)
        {
            const int timeout = 10000;
            const int bufferSize = 32;

            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            Ping pinger = new Ping();
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                PingReply pingReplyName = pinger.Send(hostname);
                Console.WriteLine();
                Console.WriteLine("Traccia instradamento verso {0} [{1}] \nsu un massimo di {2} punti di passaggio: ", hostname, pingReplyName.Address, maxTTL);
                Console.WriteLine();

                for (int ttl = 1; ttl <= maxTTL; ttl++)
                {
                    PingOptions options = new PingOptions(ttl, true);
                    stopwatch.Restart();
                    stopwatch.Start();
                    PingReply reply = pinger.Send(hostname, timeout, buffer, options);
                    stopwatch.Stop();
                    if (reply.Status == IPStatus.Success || reply.Status == IPStatus.TtlExpired)
                    {
                        if (maxMS>=stopwatch.Elapsed.TotalMilliseconds)
                        {
                            try
                            {
                                if (d == true)
                                {
                                    Console.WriteLine($"{ttl}\t {((int)stopwatch.Elapsed.TotalMilliseconds) + "ms"}\t {reply.Address}\t");
                                }
                                else
                                {
                                    Console.WriteLine($"{ttl}\t {((int)stopwatch.Elapsed.TotalMilliseconds) + "ms"}\t {reply.Address}\t {Dns.GetHostEntry(reply.Address).HostName}\t");
                                }

                            }
                            catch { Console.WriteLine($"{ttl}\t {((int)stopwatch.Elapsed.TotalMilliseconds) + "ms"}\t {reply.Address}\t"); }
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            Thread.Sleep(1000);
                            Console.WriteLine("Request TimeOut");
                        }
                    }

                    if (reply.Status != IPStatus.TtlExpired && reply.Status != IPStatus.TimedOut)
                        break;
                }
                Console.WriteLine();
                if (maxTTL < 30)
                {
                    Console.WriteLine("Tracert non riuscito");
                }
                else
                {
                    Console.WriteLine("Tracciatura completa");
                }
                Console.WriteLine();
            }
            catch { Console.WriteLine("Host sconosciuto"); }

            
        }

        static void CommandReserch(string[] command)
        {
            if (command[1] == "/?")
            {
                Console.WriteLine("Sintassi: tracert [-d] [-h max_salti] [-j elenco-host] [-w timeout \n [-R][-S indorig][-4][-6] nome_destinazione \n Opzioni: \n -d                 Non risolve gli indirizzi in nome host. \n - h max_salti       Numero massimo di punti di passaggio per \n la destinazione. \n - j elenco - host     Instradamento libero lungo l'elenco host (solo IPv4). \n - w timeout         Timeout in millisecondi per ogni risposta. \n - R                 Traccia percorso andata e ritorno(solo IPv6 \n - S indorig         Indirizzo di origine da utilizzare(solo IPv6). \n - 4                 Impone l'uso di IPv4. \n - 6                 Impone l'uso di IPv6.");
                Console.WriteLine("");
            }

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i].StartsWith("-d"))
                {
                    d = true;
                }
                if (command[i].StartsWith("-h"))
                {
                    string[] commandN = command[i].Split(':');
                    maxTTL = int.Parse(commandN[1]);
                }
                if (command[i].StartsWith("-w"))
                {
                    string[] commandN = command[i].Split(':');
                    maxMS = int.Parse(commandN[1]);
                }
            }

            GetTraceRoute(command[command.Length - 1]);
        }

        static string[] Command(string command)
        {
            return command.Split(' ');
        }

        static bool ControllCommand(string[] command)
        {
            if (command[0] == "tracert")
                return true;
            return false;
        }
    }
}
