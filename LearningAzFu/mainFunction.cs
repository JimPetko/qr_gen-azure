using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;

namespace LearningAzFu
{
    public class mainFunction
    {
        private readonly ILogger<mainFunction> _logger;

        public mainFunction(ILogger<mainFunction> logger)
        {
            _logger = logger;
        }

        [Function("GenQrCode")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            if (req != null)
            {
                string itemcode = req.Query["itemcode"];
                string requestbody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<dynamic>(requestbody);
                itemcode = itemcode ?? data?.itemcode;
                return itemcode != null ? (ActionResult)new OkObjectResult($"{GenQRBase64(itemcode)}") : new BadRequestObjectResult("Its all Goofed up dude.");
            }
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        private object GenQRBase64(string itemcode)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(itemcode, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(5, "#000000", "#FFFFFF");

            // Convert to Base64 String
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                string base64String = Convert.ToBase64String(byteImage);
                return base64String;
            }
        }
    }
}
