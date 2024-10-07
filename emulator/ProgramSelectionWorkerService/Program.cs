//using clProgramSelection; //ProgramsSelectionEmulator;


//IHost host = Host.CreateDefaultBuilder(args)
//    .UseSystemd()
//    .ConfigureServices(services =>
//    {
//        services.AddHostedService<Worker>();
//    })
//    .Build();


//await host.RunAsync();

using clProgramSelection; //ProgramsSelectionEmulator;


IHost host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        WorkerOptions options = configuration.GetSection("INITCONF").Get<WorkerOptions>();
        services.AddSingleton(options);
        services.AddHostedService<Worker>();
    })
    .Build();


await host.RunAsync();

//using clProgramSelection;
//public class Program
//{
//    public static void Main(string[] args)
//    {
//        CreateHostBuilder(args).Build().Run();
//    }

//    public static IHostBuilder CreateHostBuilder(string[] args) =>
//        Host.CreateDefaultBuilder(args)
//            .ConfigureServices((hostContext, services) => {
//                IConfiguration configuration = hostContext.Configuration;

//                WorkerOptions options = configuration.GetSection("WCF").Get<WorkerOptions>();

//                services.AddSingleton(options);

//                services.AddHostedService<Worker>();
//            });
//}
