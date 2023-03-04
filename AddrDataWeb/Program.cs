using MongoDB.Bson;
using MongoDB.Driver;

namespace AddrDataWeb
{
    public class PacketData
    {
        public ObjectId Id { get; set; }
        public string IP { get; set; }
        public string Sender { get; set; }
        public int Count { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();

            var connectionString = "mongodb+srv://test:test@cluster0.tsde1.mongodb.net/?retryWrites=true&w=majority";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("AddrData");
            var collection = database.GetCollection<PacketData>("data");
            builder.Services.AddSingleton(collection);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });



            app.Run();
        }
    }
}