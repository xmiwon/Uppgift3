using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedLibraries.Models;
using SharedLibraries.Services;
using Microsoft.Azure.Devices;

namespace AzureFunctions
{
    public static class SendMessageToDevice
    {
        //Istället för hardcode så sätter vi strängen till en json fil och hämtar därifrån istället med IotHubConnection som namn
        private static readonly ServiceClient serviceClient =
            ServiceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("IotHubConnection"));


        [FunctionName("SendMessageToDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            //QueryString = localhost:7071/api/sendmessagetodevice?targetdeviceid=consoleapp&message=dettaarmeddelandet
            //gör en request eller hämtar från query
            string targetDeviceId = req.Query["targetdeviceid"];
            string message = req.Query["message"];

            //Http body = { "targetdeviceid": "consoleapp", "message": "detta är ett meddelandde" }
             string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            



            var data = JsonConvert.DeserializeObject<BodyMessageModel>(requestBody);
            // Om inte är null, använd det, annars ?? använd vad som kommer efter som kan vara nullable
            targetDeviceId = targetDeviceId ?? data?.TargetDeviceId;
            message = message ?? data?.Message;
           //skickar till devicen med värden som finns i de 3 parametrarna
            await DeviceService.SendMessageToDeviceAsync(serviceClient, targetDeviceId, message);


            return new OkResult();
        }
    }
}
