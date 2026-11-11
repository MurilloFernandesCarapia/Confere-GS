using System.Text.Encodings.Web;
using Confere.Radar.Api.Endpoints;
using Confere.Radar.Api.Extensions;
using Confere.Radar.Application;
using Confere.Radar.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetailsConfere();
builder.Services.AddJwtConfere();
builder.Services.AddRateLimitingConfere(builder.Configuration);
builder.Services.AddSwaggerConfere();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwaggerConfere();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapAuthEndpoints(app.Environment);
app.MapRadarEndpoints();
app.MapDenunciaEndpoints();
app.MapAlertaRegionalEndpoints();
app.MapGestaoEndpoints();
app.MapBeneficioEndpoints();
app.MapPerfilSocialEndpoints();
app.MapElegibilidadeEndpoints();
app.MapHealthCheckEndpoints();

app.Run();

public partial class Program
{
}
