using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Data;
using WebServer.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition");
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PDFConverterController>();

builder.WebHost.UseKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(1000); // Set the desired timeout
});

var app = builder.Build();

// Custom Middleware to store the last request and log response status
app.Use(async (context, next) =>
{
    if (context.Request.Path.HasValue == true && context.Request.Path.Value == "/PDFConverter")
    {
        string parentFolderPath = app.Environment.ContentRootPath;
        string fileName = context.Request.Form.Files[0].FileName;
        string filePath = parentFolderPath + "/Files/" + fileName;

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            context.Request.Form.Files[0].CopyTo(stream);
        }
    }


    await next();


    int statusCode = context.Response.StatusCode;
    if (context.Request.Path.HasValue == true && context.Request.Path.Value == "/PDFConverter")
    {
        string fileName = context.Request.Form.Files[0].FileName;
        string filePath = app.Environment.ContentRootPath + "\\Files\\" + fileName;
        try
        {

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
        }
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Services.GetRequiredService<IHostApplicationLifetime>()
    .ApplicationStarted.Register(() =>
    {
        PDFConverterController PDFConverter = app.Services.GetRequiredService<PDFConverterController>();
        PDFConverter.ConvertOnRestart();
    });
app.Run();
