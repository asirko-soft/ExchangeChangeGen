using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeChangeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = args[0];
            string[] credentials = new string[] {args[1],args[2]};
            int amountToGenerate = Convert.ToInt32(args[3]);
            int generateTime = Convert.ToInt32(args[4]);
            int messageSize = Convert.ToInt32(args[5]);

            // Certificate validation
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
            ExchangeGenerator server1 = new ExchangeGenerator(serverIP, credentials, amountToGenerate, generateTime, messageSize);
            server1.startGenerator();
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
    }
}
