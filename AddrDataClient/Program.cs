using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Data;
using System;

namespace AddrData
{
    //to-do: implement settings class
    public class DatabaseSettings
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public DatabaseSettings()
        {

            string path = Path.Combine(Directory.GetCurrentDirectory(), "settings.ini");
            var databaseSettings = new DatabaseSettings();

            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                var section = string.Empty;
                var settings = new Dictionary<string, string>();

                foreach (var line in lines)
                {
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Substring(1, line.Length - 2);
                    }
                    else if (section == "databaseSettings")
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            settings[parts[0]] = parts[1];
                        }
                    }
                }

                if (settings.TryGetValue("IP", out var ip))
                {
                    databaseSettings.IP = ip;
                }
                if (settings.TryGetValue("Port", out var port))
                {
                    if (int.TryParse(port, out var parsedPort))
                    {
                        databaseSettings.Port = parsedPort;
                    }
                }
                if (settings.TryGetValue("Username", out var username))
                {
                    databaseSettings.Username = username;
                }
                if (settings.TryGetValue("Password", out var password))
                {
                    databaseSettings.Password = password;
                }
            }
        }
    }

    public class PacketData
    {
        public ObjectId Id { get; set; }
        public string IP { get; set; }
        public string Sender { get; set; }
        public int Count { get; set; }
    }


    internal class Program
    {
       

        static async Task serverlistener(CancellationToken cancellationToken)
        {
            TcpListener server = null;
            try
            {
                // Avaa uusi TCPKuuntelija portille 8787 ja hyväksy kaikki IP-Osoitteet
                server = new TcpListener(IPAddress.Any, 8787);
                // Aloita kuuntelu
                server.Start();

                // Aloita loop
                while (true)
                {
                    Console.Write("Listening...");
                    // Hyväksy kaikki yhteydet
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected");

                    // Stream objekti lukemiselle
                    NetworkStream stream = client.GetStream();
                    while (true)
                    {
                        if (!stream.DataAvailable)
                        {
                            if (server.Pending())
                            {
                                break;
                            }
                        }

                        if (stream.DataAvailable)
                        {
                            //uusi puskuri ja puskurinkoko pyydetään clientiltä
                            byte[] buffer = new byte[client.ReceiveBufferSize];
                            int bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                            //Dekoodataan Bytes jotta saadaan merkkijono.
                            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            //tulostetaan merkkijono
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Received: {0}", dataReceived);
                            Console.ResetColor();
                            //add block rule in windows firewall
                            string ipAddressToBlock = dataReceived;

                            Console.WriteLine("Starting block process");
                            Process process = new Process();
                            process.StartInfo.FileName = "netsh";
                            
                            
                            process.StartInfo.Arguments = $"advfirewall firewall add rule name=\"ADDRCLIENT\" dir=in action=block remoteip={ipAddressToBlock} enable=yes"; ;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            string output = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            Console.WriteLine("IN block: " + output);

                            process.StartInfo.Arguments = $"advfirewall firewall add rule name=\"ADDRCLIENT\" dir=out action=block remoteip={ipAddressToBlock} enable=yes"; ;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            output = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            Console.WriteLine("OUT block: " + output);


                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("IP: {0} blocked", ipAddressToBlock);
                            Console.ResetColor();



                        }

                        if (server.Pending())
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Client close");
                    // Sulje yhteys
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Console.WriteLine("Closing socket listen.");
                server.Stop();
            }
        }








        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb+srv://test:test@cluster0.tsde1.mongodb.net/?retryWrites=true&w=majority");
            var database = client.GetDatabase("AddrData");
            var collection = database.GetCollection<PacketData>("data");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };


            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), BitConverter.GetBytes(1));

            var listenerTask = serverlistener(cts.Token);

            while (!cts.Token.IsCancellationRequested)
            {
                




                // Packet receive
                byte[] buffer = new byte[4096];
                try
                {
                    int bytesReceived = socket.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                // IP header Source/Dest
                byte[] sourceBytes = new byte[4];
                Array.Copy(buffer, 12, sourceBytes, 0, 4);
                IPAddress sourceAddress = new IPAddress(sourceBytes);

                byte[] destinationBytes = new byte[4];
                Array.Copy(buffer, 16, destinationBytes, 0, 4);
                IPAddress destinationAddress = new IPAddress(destinationBytes);

                //var PacketData isnt local ip




                var packetData = new PacketData
                {
                    IP = sourceAddress.ToString(),
                    Sender = Environment.MachineName
                };

                var filter = Builders<PacketData>.Filter.Where(x => x.IP == packetData.IP && x.Sender == packetData.Sender);
                var update = Builders<PacketData>.Update.Inc("Count", 1);

                var options = new FindOneAndUpdateOptions<PacketData>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                };

                var result = collection.FindOneAndUpdate(filter, update, options);

                // Info printti
                //Console.WriteLine("From " + sourceAddress.ToString() + " to " + destinationAddress.ToString());
                Array.Clear(buffer, 0, buffer.Length);
            }

            //testing purposes clear rules
            Console.WriteLine("clear firewall rules");           
            Process process = new Process();
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "advfirewall reset";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(output);
            Console.WriteLine("Closing");

            
        }
    }
}

         