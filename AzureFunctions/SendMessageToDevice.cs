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
        //Ist�llet f�r hardcode s� s�tter vi str�ngen till en json fil och h�mtar d�rifr�n ist�llet med IotHubConnection som namn
        private static readonly ServiceClient serviceClient =
            ServiceClient.CreateFromConnectionString(Environment.GetEnvironmentVariable("IotHubConnection"));


        [FunctionName("SendMessageToDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            
            //QueryString = localhost:7071/api/sendmessagetodevice?targetdeviceid=consoleapp&message=dettaarmeddelandet
            //g�r en request eller h�mtar fr�n query
            string targetDeviceId = req.Query["targetdeviceid"];
            string message = req.Query["message"];

            //Http body = { "targetdeviceid": "consoleapp", "message": "detta �r ett meddelandde" }
             string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            



            var data = JsonConvert.DeserializeObject<BodyMessageModel>(requestBody);
            // Om inte �r null, anv�nd det, annars ?? anv�nd vad som kommer efter som kan vara nullable
            targetDeviceId = targetDeviceId ?? data?.TargetDeviceId;
            message = message ?? data?.Message;
           //skickar till devicen med v�rden som finns i de 3 parametrarna
            await DeviceService.SendMessageToDeviceAsync(serviceClient, targetDeviceId, message);


            return new OkResult();
        }
    }
}
