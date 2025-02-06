
using Domain.Abstractions;
using Domain.Services;
using Infrastructure;
using Serilog;
namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //builder.Logging.AddSerilog();
            
            AddDependencies(builder.Services);

            builder.Host.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.WriteTo.Console();
                loggerConfiguration.WriteTo.File("logs.log");
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseSerilogRequestLogging();

            app.UseAuthorization();

            app.UseWebSockets();

            app.MapControllers();

            app.Run();

        }

        public static IServiceCollection AddDependencies(IServiceCollection services)
        {
            services.AddScoped<IDataStore, DataStore>();
            services.AddScoped<IExchangeService, ExchangeService>();
            services.AddSingleton<WebSocketConnectionManager>();

            return services;
        }
    }
}
