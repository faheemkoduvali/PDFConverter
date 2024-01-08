using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileProviders;
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

builder.WebHost.UseKestrel(options =>
{
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(1000); // Set the desired timeout
});

var app = builder.Build();

// Custom Middleware to store the last request and log response status
app.Use(async (context, next) =>
{
    if (context.Request.Path.HasValue == true && context.Request.Path.Value == "/PDFConverter/PDFConverterOnFailure")
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
    if (context.Request.Path.HasValue == true && context.Request.Path.Value == "/PDFConverter/PDFConverterOnFailure")
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

// Handle graceful shutdown
//app.Services.GetRequiredService<IHostApplicationLifetime>()
//    .ApplicationStopping.Register(() =>
//    {
//        //string folderPath = app.Environment.ContentRootPath + "\\Files";
//        //string[] files = Directory.GetFiles(folderPath);
//        //foreach (string filePath in files)
//        //{
//        //    PDFConverterController PDFConverter = new PDFConverterController(app.Environment);

//        //    IFileProvider fileProvider = new PhysicalFileProvider(Path.GetDirectoryName(folderPath));

//        //    // Get the file information
//        //    IFileInfo fileInfo = fileProvider.GetFileInfo(filePath);

//        //    // Create an IFormFile from the file information
//        //    IFormFile formFile = new FormFile(fileInfo.CreateReadStream(), 0, fileInfo.Length, null, fileInfo.Name);

//        //    //PDFConverter.PDFConverterOnFailure(formFile);
//        //}
//        //PDFConverter.
//    });

app.Run();
