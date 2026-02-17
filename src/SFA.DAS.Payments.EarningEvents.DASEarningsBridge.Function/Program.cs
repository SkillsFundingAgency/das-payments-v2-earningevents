using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Infrastructure.Configuration;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

builder.Configuration.AddEnvironmentVariables();

builder.Services
    .AddOptions<EarningsBridgeConfiguration>()
    .Bind(builder.Configuration.GetSection("Values"))
    .ValidateOnStart();

builder.Services.AddSingleton<IEarningsBridgeConfiguration>(sp =>
    sp.GetRequiredService<IOptions<EarningsBridgeConfiguration>>().Value);

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddDbContext<IEarningsDataContext, EarningsDataContext>((sp, options) =>
{
    var config = sp.GetService<IEarningsBridgeConfiguration>();
    options.UseSqlServer(config.PaymentsConnectionString);
});

builder.Services.AddScoped<IGSLEarningsMapper, GSLEarningsMapper>();
builder.Services.AddScoped<IGSLCalculatePaymentsHandler, GSLCalculatePaymentsHandler>();
builder.Services.AddScoped<ICalculateGSLPaymentsValidator, CalculateGSLPaymentsValidator>();
builder.Services.AddScoped<IEarningsRepository, EarningsRepository>();


builder.Services.AddHttpClient<CollectionPeriodApiClient>((sp, client) =>
{
    var config = sp.GetService<IEarningsBridgeConfiguration>();
    client.BaseAddress = new Uri(config.CollectionPeriodApiBaseAddress);
});
builder.Services.AddScoped<ICollectionPeriodApiClient, CollectionPeriodApiClient>();

builder.Services.AddScoped<IPaymentsServiceBusPublisher, PaymentsServiceBusPublisher>((sp) =>
{
    var config = sp.GetService<IEarningsBridgeConfiguration>();
    return new PaymentsServiceBusPublisher(config.PaymentsServiceBusConnectionString);
});

builder.Build().Run();
