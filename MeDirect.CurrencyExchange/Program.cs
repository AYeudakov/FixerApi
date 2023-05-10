using System.Reflection;
using MeDirect.CurrencyExchange.Application.Common.Interfaces;
using MeDirect.CurrencyExchange.Application.Interfaces;
using MeDirect.CurrencyExchange.Application.Services;
using MeDirect.CurrencyExchange.Extensions;
using MeDirect.CurrencyExchange.Infrastructure;
using MeDirect.CurrencyExchange.Infrastructure.Persistence;
using MeDirect.CurrencyExchange.Middlewares;
using MeDirect.CurrencyExchange.Options;

var builder = WebApplication.CreateBuilder(args);
var logger = GetLogger(builder);

builder.Services.AddControllers();
// can be setted on base of appsettings info, for now just 1 min
builder.Services.AddMemoryCache(action => action.ExpirationScanFrequency = TimeSpan.FromMinutes(1));

builder.Services.AddJwtBarrier(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddHttpClientForProvider(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(Program)));

builder.Services.AddOptions<JwtOptions>(nameof(JwtOptions));

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExchangeCurrencyService, ExchangeCurrencyService>();
builder.Services.AddScoped<IFixerApiRequester, FixerApiRequester>();

builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

logger.LogInformation("Building app..");
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

try
{
    logger.LogInformation("Running service...");
    app.Run();
}
catch (Exception e)
{
    logger.LogError("{serviceName} service failed unexpectedly with exception: {exception}",
        nameof(MeDirect.CurrencyExchange), e.Message);
}


ILogger GetLogger(WebApplicationBuilder webApplicationBuilder)
{
    var programLogger = LoggerFactory.Create(config =>
    {
        config.AddConsole();
        config.AddConfiguration(webApplicationBuilder.Configuration.GetSection("Logging"));
    }).CreateLogger(nameof(Program));
    
    return programLogger;
}