using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace PingSslVersions
{
    internal class SslTcpClient
    {
        private readonly bool _verbose;

        public SslTcpClient(bool verbose)
        {
            _verbose = verbose;
        }

        // The following method is invoked by the RemoteCertificateValidationDelegate. 
        public bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                if (_verbose)
                {
                    Console.WriteLine("Certificate OK");
                }
                return true;
            }
            Console.Error.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

        public X509Certificate UserCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            if (_verbose)
            {
                Console.WriteLine("UserCertificateSelectionCallback: " + localCertificates.Count);
                foreach (X509Certificate certificate in localCertificates)
                {
                    Console.WriteLine(certificate.Subject);
                }
                Console.WriteLine("AcceptableIssuers: {0}", String.Join(", ", acceptableIssuers));
            }
            return remoteCertificate;
        }

        public void RunClient(string machineName, string serverName, SslProtocols enabledSslProtocols)
        {
            // Create a TCP/IP client socket. 
            using (var client = new TcpClient(machineName, 443))
            {
                if (_verbose)
                {
                    Console.WriteLine("Client connected.");
                }
                // Create an SSL stream that will close the client's stream.
                using (var sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate, UserCertificateSelectionCallback, EncryptionPolicy.RequireEncryption))
                {
                    try
                    {
                        sslStream.AuthenticateAsClient(serverName, new X509CertificateCollection(), enabledSslProtocols, false);
                    }
                    catch (AuthenticationException e)
                    {
                        Console.Error.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                        {
                            Console.Error.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        }
                        Console.Error.WriteLine("Authentication failed - closing the connection.");
                        client.Close();
                        return;
                    }
            
                    byte[] messsage = Encoding.UTF8.GetBytes(@"GET https://"+machineName+@"/ HTTP/1.1
Host: "+machineName+@"
Connection: keep-alive
Cache-Control: max-age=0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
User-Agent: CustomAgent
Accept-Encoding: 
Accept-Language: en-US,en;q=0.8,sv;q=0.6,nb;q=0.4
Cookie:

");
                    sslStream.Write(messsage);
                    sslStream.Flush();
                    var serverMessage = ReadHeader(sslStream);
                    if (_verbose)
                    {
                        Console.WriteLine("Server says: {0}", serverMessage);
                    }
                    Console.WriteLine("Ssl protocol: {0}", sslStream.SslProtocol);
                }
                // Close the client connection.
                client.Close();
            }
        }

        private string ReadHeader(SslStream sslStream)
        {
            var streamreader = new StreamReader(sslStream, Encoding.UTF8);
            var messageData = new StringBuilder();
            var count = 100000;
            while (true)
            {
                var readLine = streamreader.ReadLine();
                messageData.AppendLine(readLine);
                if (string.IsNullOrWhiteSpace(readLine))
                {// Just read the header
                    break;
                }
                if ((--count) <= 0)
                {
                    Console.Error.WriteLine("The server is sending to much data!");
                    Environment.Exit(1);
                    break;
                }
            }
            
            return messageData.ToString();
        }
    }
}