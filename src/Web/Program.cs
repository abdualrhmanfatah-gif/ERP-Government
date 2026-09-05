using ERP_Government.Infrastructure.Data;
using ERP_Government.Web.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

// Register authorization startup validator
builder.Services.AddSingleton<AuthorizationStartupValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(static builder => 
    builder.AllowAnyMethod()
        .AllowAnyHeader()
        .AllowAnyOrigin());

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ERP_Government.Web.Middleware.RecordSessionOnLoginMiddleware>();

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });


app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

// Validate authorization coverage at startup
var authValidator = app.Services.GetRequiredService<AuthorizationStartupValidator>();
authValidator.Validate(app);

app.MapFallbackToFile("index.html");

app.Run();
