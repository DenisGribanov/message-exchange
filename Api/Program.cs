
using Domain.Abstractions;
using Domain.Services;
using Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using AutoMapper;
using Domain;
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
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "MessageExchange",
                    Description = "MessageExchange API",
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            AddDependencies(builder.Services);

            builder.Host.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration.WriteTo.Console();
                loggerConfiguration.WriteTo.File("logs.log");
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSerilogRequestLogging();

            app.UseAuthorization();

            app.UseWebSockets();

            app.MapControllers();

            app.Run();

        }

        public static IServiceCollection AddDependencies(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(AppMappingProfile));

            services.AddScoped<IDataStore, DataStore>();
            services.AddScoped<IExchangeService, ExchangeService>();
            services.AddSingleton<IWsConnectionManager, WsConnectionManager>();

            return services;
        }
    }
}
