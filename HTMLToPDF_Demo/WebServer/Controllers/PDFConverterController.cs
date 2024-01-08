using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PuppeteerSharp;
using System.Data;
using System.Net.Mime;
using file = System.IO.File;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class PDFConverterController : ControllerBase
    {
        //private readonly ILogger<PDFConverterController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PDFConverterController(IWebHostEnvironment hostingEnvironment)
        {
            //_logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }
        //public PDFConverterController()
        //{ 
        //}

        [HttpPost("PDFConverter")]
        public async Task<IActionResult> PDFConverter(IFormFile htmlFile)
        {
            if (htmlFile == null || htmlFile.Length == 0)
            {
                return BadRequest("HTML file is not provided.");
            }

            try
            {
                using (var reader = new StreamReader(htmlFile.OpenReadStream()))
                {
                    var htmlContent = await reader.ReadToEndAsync();

                    var pdfBytes = await ConvertHtmlToPdfAsync(htmlContent);

                    return File(pdfBytes, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("PDFConverterOnFailure")]
        public async Task<IActionResult> PDFConverterOnFailure(IFormFile htmlFile)
        {
            try
            {
                string filePath = _hostingEnvironment.ContentRootPath + "Files\\" + htmlFile.FileName;

                var pdfBytes = await ConvertHtmlToPdfAsync(file.ReadAllText(filePath));

                var fileName = htmlFile.FileName.Split('.')[0] + ".pdf";
                string destFilePath = Directory.GetParent(_hostingEnvironment.ContentRootPath)?.Parent + "\\clientapp\\src\\ConvertedFiles\\" + fileName;

                using (FileStream fileStream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(pdfBytes, 0, pdfBytes.Length);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions 
                { 
                    Headless = true ,
                    ExecutablePath = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
                }))
                using (var page = await browser.NewPageAsync())
                {
                    await page.SetContentAsync(htmlContent);

                    var pdfBytes = await page.PdfDataAsync();

                    return pdfBytes;
                }
        }
    }
}