using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeChangeGenerator
{
    class WMIConnector
    {
        private string username, password, authority, scopePath;
        public string Username
        {
            set
            {
                username = HelperMethods.separateNameAndDomain(value, HelperMethods.NameParts.name);
                authority = "ntlmdomain:" + HelperMethods.separateNameAndDomain(value, HelperMethods.NameParts.domain);
            }
        }
        public string Password { get { return password; } set { password = value; } }
        public string ScopePath { get { return scopePath; } set { scopePath = "\\\\" + value + "\\root\\cimv2"; } }

        public WMIConnector(string machineIP, string username, string password)
        {
            ScopePath = machineIP;
            Username = username;
            Password = password;
        }

        public void StopExchangeService()
        {
            var serviceStopped = !getExchangeTransportServiceState();

            while (!serviceStopped)
            {
                var queryResults = InvokeWMIQuery("SELECT * FROM Win32_Service WHERE Name = 'MSExchangeTransport'");

                foreach (ManagementObject resultObject in queryResults)
                {
                    resultObject.InvokeMethod("StopService", null);
                }

                Thread.Sleep(30000);

                serviceStopped = !getExchangeTransportServiceState();
            }

        }

        public void StartExchangeService()
        {
            var serviceRunning = getExchangeTransportServiceState();

            while (!serviceRunning)
            {
                var queryResults = InvokeWMIQuery("SELECT * FROM Win32_Service WHERE Name = 'MSExchangeTransport'");

                foreach (ManagementObject resultObject in queryResults)
                {
                    resultObject.InvokeMethod("StartService", null);
                }

                Thread.Sleep(30000);

                serviceRunning = getExchangeTransportServiceState();
            }
        }

        private bool getExchangeTransportServiceState()
        {
            var serviceIsRunning = false;

            var queryResults = InvokeWMIQuery("SELECT * FROM Win32_Service WHERE Name = 'MSExchangeTransport'");

            foreach (ManagementObject resultObject in queryResults)
            {
                if (resultObject["State"].ToString() == "Running")
                {
                    serviceIsRunning = true;
                }
            }

            return serviceIsRunning;
        }

        public String getExchangeServicePath()
        {
            var exchangePath = "";
            var queryResults = InvokeWMIQuery("SELECT * FROM Win32_Service WHERE Name = 'MSExchangeTransport'");

            foreach (ManagementObject resultObject in queryResults)
            {
                exchangePath = resultObject["PathName"].ToString();
            }

            return exchangePath;
        }

        public ManagementObjectCollection InvokeWMIQuery(string wmiQuery)
        {
            ConnectionOptions options = new ConnectionOptions();

            options.Username = username;
            options.Password = password;
            options.Authority = authority;

            ManagementScope scope = new ManagementScope(scopePath, options);

            scope.Connect();

            ObjectQuery query = new ObjectQuery(wmiQuery);

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            ManagementObjectCollection queryCollection = searcher.Get();

            return queryCollection;
        }


    }
}
