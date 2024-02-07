using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using HrappRepositories.DBContext;
using HrappServices.Interfaces;
using HrappServices;
using HrappRepositories.Interfaces;
using HrappRepositories;
using Microsoft.Azure.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using HrappModels;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<EmployeeDbContext>(options => options.UseSqlServer(context.Configuration["AzureDBCString"]));
        services.AddDbContext<JobStatusDbContext>(options => options.UseSqlServer(context.Configuration["AzureDBCString"]));
        services.AddScoped<IExtractDataService, ExtractDataService>();
        services.AddScoped<ITransformDataService, TransformDataService>();
        services.AddScoped<IRepository<JobStatusModel>>((sp) => new Repository<JobStatusModel>(sp.GetService<JobStatusDbContext>()));
        services.AddScoped<IJobStatusService, JobStatusService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEmployeesService, EmployeesService>();
        services.AddScoped<IEmployeesRepository, EmployeesRepository>();
        services.AddScoped<BlobContainerClient>((sp) => new BlobContainerClient(context.Configuration["BlobStorageConnectionString"], context.Configuration["BlobContainerName"]));
                                                             
    })
    .Build();


host.Run();
