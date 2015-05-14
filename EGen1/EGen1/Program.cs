using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace ExchangeChangeGenerator
{
    class Program
    {
        private static readonly object _lock = new object();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify path to .csv file with server parameters.\nExample: ExchangeChGen.exe \"C:\\User\\Administrator\\Desktop\\servers.csv\"");
            }
            else
            {
                Program.writeToLog("---------------------------------------------------------");
                ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;

                List<ExchangeGenerator> serverList = new List<ExchangeGenerator>();

                TextFieldParser parser = new TextFieldParser(args[0]);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    serverList.Add(new ExchangeGenerator(fields[0], new string[] { fields[1], fields[2] },
                        Convert.ToInt32(fields[3]), Convert.ToInt32(fields[4]), Convert.ToInt32(fields[5])));

                }

                Parallel.ForEach(serverList, s => s.startGenerator());
            }
        }

        // Forced certificate validation to return always true
        private static bool CertificateValidationCallBack(
        object sender,
        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        System.Security.Cryptography.X509Certificates.X509Chain chain,
        System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static void writeToLog(string logMessage, string serverIP)
        {
            lock (_lock)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    w.WriteLine("[{0} - {1}] - {2}", serverIP, DateTime.Now.ToString(), logMessage);
                }
            }
        }

        public static void writeToLog(string logMessage)
        {
            lock (_lock)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    w.WriteLine("[{0}] - {1}", DateTime.Now.ToString(), logMessage);
                }
            }
        }
    }
}
