using Azure.Identity;
using Azure.Storage.Blobs;
using HrappModels;
using HrappRepositories;
using HrappRepositories.DBContext;
using HrappRepositories.Interfaces;
using HrappServices;
using HrappServices.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureKeyVault(
        new Uri("https://hrappsecrets.vault.azure.net/"),
        new DefaultAzureCredential());

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<JobStatusDbContext>(options => options.UseSqlServer(builder.Configuration["AzureDBCString"]));
builder.Services.AddDbContext<TenantDbContext>(options =>    options.UseSqlServer(builder.Configuration["AzureDBCString"]));
builder.Services.AddDbContext<EmployeeDbContext>(options =>  options.UseSqlServer(builder.Configuration["AzureDBCString"]));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<TenantDbContext>()            
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWTValidAudience"],
                    ValidIssuer = builder.Configuration["JWTValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTSecret"]))
                };
            });


//Blob Storage
builder.Services.AddScoped<BlobContainerClient>((sp) => new BlobContainerClient(builder.Configuration["BlobStorageConnectionString"],builder.Configuration["BlobContainerName"]));

//Service Bus
builder.Services.AddScoped<IServiceBusService, ServiceBusService>();
builder.Services.AddScoped<IQueueClient>((sp) => new QueueClient(builder.Configuration["ServiceBusConnectionString"], builder.Configuration["ServiceBusQueueName"]));

//Custom Services
builder.Services.AddScoped<IUploadDataService, UploadDataService>();
builder.Services.AddScoped<IEmployeesRepository, EmployeesRepository>();
builder.Services.AddScoped<IEmployeesService, EmployeesService>();
builder.Services.AddScoped(typeof(IRepository<>),typeof( Repository<>));
builder.Services.AddScoped<IRepository<JobStatusModel>>((sp) => new Repository<JobStatusModel>(sp.GetService<JobStatusDbContext>()));
builder.Services.AddScoped<IJobStatusService, JobStatusService>();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
