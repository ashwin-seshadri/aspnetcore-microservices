using Mango.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthentication();
if (builder.Environment.EnvironmentName.ToLower().Equals("production"))
{
    builder.Configuration.AddJsonFile("ocelot.Production.json", optional: false, reloadOnChange: true);
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
}

builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot().Wait();
app.Run();
