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

// QuestPDF requires an explicitly selected license before generating PDFs.
// The license is configured only when present so unrelated startup tasks,
// such as OpenAPI generation, do not depend on PDF export configuration.
var questPdfLicenseName = builder.Configuration["QuestPdf:License"];
if (!string.IsNullOrWhiteSpace(questPdfLicenseName))
{
    ERP_Government.Infrastructure.Services.PdfReportExporter.Configure(questPdfLicenseName);
}

// Official report print template branding (header logo, entity names).
var brandingSection = builder.Configuration.GetSection("ReportBranding");
var brandingLogoPath = brandingSection["LogoPath"];
var brandingLogoAbsolute = string.IsNullOrWhiteSpace(brandingLogoPath)
    ? null
    : Path.Combine(builder.Environment.ContentRootPath, brandingLogoPath);

var republicHeaderPath = brandingSection["RepublicHeaderPath"];
var republicHeaderAbsolute = string.IsNullOrWhiteSpace(republicHeaderPath)
    ? null
    : Path.Combine(builder.Environment.ContentRootPath, republicHeaderPath);

var ministryHeaderPath = brandingSection["MinistryHeaderPath"];
var ministryHeaderAbsolute = string.IsNullOrWhiteSpace(ministryHeaderPath)
    ? null
    : Path.Combine(builder.Environment.ContentRootPath, ministryHeaderPath);

ERP_Government.Infrastructure.Services.ReportBranding.Configure(
    brandingSection["GovernmentLine"] ?? "الجمهورية اليمنية",
    brandingSection["OrganizationName"] ?? string.Empty,
    brandingSection["DepartmentName"],
    brandingLogoAbsolute,
    republicHeaderAbsolute,
    ministryHeaderAbsolute);

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

app.UseExceptionHandler(options => { });

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ERP_Government.Web.Middleware.RecordSessionOnLoginMiddleware>();

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();


app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

// Validate authorization coverage at startup
var authValidator = app.Services.GetRequiredService<AuthorizationStartupValidator>();
authValidator.Validate(app);

app.MapFallbackToFile("index.html");

app.Run();
