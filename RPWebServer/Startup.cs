using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using RPWebServer.AutoMapping;
using RPWebServer.Configuration;
using RPWebServer.Services;
using RPWebServer.Services.ReverseProxy;
using Yarp.ReverseProxy.Configuration;

namespace RPWebServer;

[ExcludeFromCodeCoverage]
public class Startup
{
    public WebApplicationBuilder Builder { get; }
    public WebApplication? App { get; set; }
    
    public Startup(string[] args)
    {
        Builder = WebApplication.CreateBuilder(args);
    }

    public void Init()
    {
        SetupServices();
        CreateApp();
        UpdateComponentsMapping();
    }
    
    
    public void Run()
    {
        App?.Run();
    }


    private void SetupServices()
    {
        Builder.Services.AddTransient<IFileSystem, FileSystem>();
        Builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(RouteConfigProfile)));
        Builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RouteConfigProfile).Assembly));
        Builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        Builder.Services.AddEndpointsApiExplorer();
        Builder.Services.AddSwaggerGen();
        Builder.Services
            .AddSingleton<ICustomMemoryConfigFactory, CustomMemoryConfigFactory>()
            .AddSingleton<IComponentProviderFactory, ComponentProviderFactory>()
            .AddSingleton<IComponentsMappingService, ComponentsMappingService>()
            .AddSingleton<IReverseProxyChangesMonitor, ReverseProxyChangesMonitor>()
            .AddSingleton<IRoutesConfigProvider, RoutesConfigProvider>()
            .AddSingleton<IClustersConfigProvider, ClustersConfigProvider>()
            .AddSingleton<IProxyConfigProvider, CustomProxyConfigProvider>()
            .AddReverseProxy();

        Builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("*");
            });
        });

        Builder.Services.AddOptions<WebComponentsServerOptions>()
            .Bind(Builder.Configuration.GetSection("WebComponentsServer"));

        Builder.Logging.AddConsole();
    }

    private void CreateApp()
    {
        App = Builder.Build();

        App.MapReverseProxy();

        // Configure the HTTP request pipeline.
        if (App.Environment.IsDevelopment())
        {
            App.UseSwagger();
            App.UseSwaggerUI();
        }

        App.UseHttpsRedirection();

        App.UseAuthorization();
        App.UseCors();

        App.MapControllers();
    }

    private void UpdateComponentsMapping()
    {
        var componentsMappingService = App?.Services.GetService<IComponentsMappingService>();
        componentsMappingService?.UpdateMapping();
    }

}