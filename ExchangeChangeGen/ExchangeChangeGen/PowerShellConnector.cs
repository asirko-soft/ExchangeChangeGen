using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeChangeGenerator
{
    class PowerShellConnector
    {
        private string kerberosUri;
        private const string schemaUri = "http://schemas.microsoft.com/powershell/Microsoft.Exchange";
        private PSCredential credentials;
        private string exchangePath;
        private string computerIP;

        public PowerShellConnector(string machineIP, string username, string password)
        {
            WMIConnector wmiConnector = new WMIConnector(machineIP, username, password);

            var queryResults = wmiConnector.InvokeWMIQuery("SELECT * FROM Win32_ComputerSystem");

            foreach (ManagementObject resultObject in queryResults)
            {
                kerberosUri = "http://" + resultObject["DNSHostname"].ToString() + "/PowerShell";
            }

            computerIP = machineIP;

            System.Security.SecureString PSpassword = new System.Security.SecureString();

            foreach (char c in password)
            {
                PSpassword.AppendChar(c);
            }

            credentials = new PSCredential(username, PSpassword);

            exchangePath = wmiConnector.getExchangeServicePath().Remove(0, 1);
        }

        public delegate void CommandForRunspace(Runspace runspace);

        public void PerformCommandInRunspace(CommandForRunspace commandToPerform, PSInstanceToConnect psInstance)
        {
            WSManConnectionInfo connectionInfo = new WSManConnectionInfo();
            connectionInfo.Credential = credentials;
            switch (psInstance)
            {
                case PSInstanceToConnect.Default:
                    connectionInfo.ComputerName = computerIP;
                    break;

                case PSInstanceToConnect.Exchange:
                    connectionInfo.ConnectionUri = new Uri(kerberosUri);
                    connectionInfo.ShellUri = schemaUri;
                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Kerberos;
                    break;

                default:
                    break;
            }

            using (Runspace runspace = RunspaceFactory.CreateRunspace(connectionInfo))
            {
                commandToPerform(runspace);
            }

        }
        public void MountMailbox(Runspace runspace)
        {
            //int retryCount = 5;
            //while (true)
            //{
            //    try
            //    {
                    string script = @"Get-MailboxDatabase | Mount-Database";

                    Pipeline pipeline = runspace.CreatePipeline();

                    runspace.Open();

                    pipeline.Commands.AddScript(script);

                    pipeline.Invoke();
                    //return;
                //}
                //catch ()
                //{

                //}
               
            //}
            
        }

        public void DeleteAndRecreateMailQue(Runspace runspace)
        {
            var indexForPathTrim = exchangePath.IndexOf("\\bin", StringComparison.CurrentCultureIgnoreCase);

            var shortEPath = exchangePath.Remove(indexForPathTrim);

            var mailQueuePath = shortEPath + "\\TransportRoles\\data\\Queue\\mail.que";

            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddCommand("Remove-Item");
                powershell.AddParameter("-Path", mailQueuePath);

                runspace.Open();

                powershell.Runspace = runspace;

                powershell.Invoke();

                powershell.AddCommand("New-Item");
                powershell.AddParameter("-Path", mailQueuePath);
                powershell.AddParameter("-Type", "file");

                powershell.Invoke();
            }

        } 
    }

    enum PSInstanceToConnect
    {
        Default,
        Exchange
    }
}
