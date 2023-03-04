using System.Net;
using System.Net.Sockets;
using MongoDB.Bson;
using MongoDB.Driver;


namespace AddrData
{
    
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

        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb+srv://test:test@cluster0.tsde1.mongodb.net/?retryWrites=true&w=majority");
            var database = client.GetDatabase("AddrData");
            var collection = database.GetCollection<PacketData>("data");


            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), BitConverter.GetBytes(1));

            
            while (true)
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
                Console.WriteLine("From " + sourceAddress.ToString() + " to " + destinationAddress.ToString());
                Array.Clear(buffer, 0, buffer.Length);
            }

        }
    }
}

         