using System;
using System.Linq;
using System.Security.Authentication;
using NDesk.Options;

namespace PingSslVersions
{
    internal class Program
    {
        private static void DisplayUsage()
        {
            Console.WriteLine("To ping ssl:");
            Console.WriteLine("pingssl -m=machinename -c=servercertificatename -p=sslprotocols");
            Environment.Exit(1);
        }

        private static void Main(string[] args)
        {
            string serverCertificateName = null;
            string machineName = null;
            var verbose = false;
            SslProtocols sslprotocols = SslProtocols.Ssl2 | SslProtocols.Ssl3;
            var p = new OptionSet() {
                { "h|?|help", v => DisplayUsage() },
                { "m|machinename=", v=>machineName=v},
                { "v|verbose", v=>verbose=true},
                { "c|servercertificatename=", v=>serverCertificateName=v},
                { "p|sslprotocols=", v=>sslprotocols=ParseSslprotocols(v)},
              };
            p.Parse(args);
            if (args == null || args.Length ==0)
            {
                DisplayUsage();
                return;
            }
            
            if (string.IsNullOrEmpty(machineName))
            {
                DisplayUsage();
                return;
            }
            if (string.IsNullOrEmpty(serverCertificateName))
            {
                serverCertificateName = machineName;
            }
            if (verbose)
            {
                Console.WriteLine("Connecting to: {0}", machineName);
                Console.WriteLine("Trying to use the following protocols: {0}", sslprotocols);
            }
            var client = new SslTcpClient(verbose);
            try
            {
                client.RunClient(machineName, serverCertificateName, sslprotocols);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
            
        }

        private static SslProtocols ParseSslprotocols(string s)
        {
            var sslprotocols = SslProtocols.None;
            var sslProtocolsRaw =
                s.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(SslProtocolsParse)
                    .ToArray();
            if (sslProtocolsRaw.Any())
            {
                sslprotocols = sslProtocolsRaw
                    .Aggregate((aggregator, next) => aggregator | next);
            }
            return sslprotocols;
        }

        private static SslProtocols SslProtocolsParse(string protocol)
        {
            SslProtocols result;
            if (!Enum.TryParse(protocol, true, out result))
            {
                throw new Exception("! "+protocol);
            }
            return result;
        }
    }
}
