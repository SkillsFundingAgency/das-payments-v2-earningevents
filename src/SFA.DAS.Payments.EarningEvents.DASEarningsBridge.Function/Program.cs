using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.EarningEvents.Data;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Handlers;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Repositories;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Services;
using SFA.DAS.Payments.EarningEvents.EarningsBridge.Application.Validators;


var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddDbContext<IEarningsDataContext, EarningsDataContext>(options =>
{
    options.UseSqlServer(Environment.GetEnvironmentVariable("PaymentsConnectionString"));
});

builder.Services.AddScoped<IGSLearningsMapper, GSLEarningsMapper>();
builder.Services.AddScoped<IGSLCalculatePaymentsHandler, GSLCalculatePaymentsHandler>();
builder.Services.AddScoped<ICalculateGSLPaymentsValidator, CalculateGSLPaymentsValidator>();
builder.Services.AddScoped<IEarningsRepository, EarningsRepository>();


builder.Services.AddHttpClient<CollectionPeriodAPI>(client =>
{
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("CollectionPeriodAPIEndpoint"));
});

builder.Build().Run();