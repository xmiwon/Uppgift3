using System;
using System.Text;
using System.Threading.Tasks;
using MAD = Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using SharedLibraries.Models;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace SharedLibraries.Services
{
 //Deklareras redan och skapar ett minnesplats med static
    public static class DeviceService
    {
        //En konstant värde som går aldrig att ändra på
        private static readonly Random rnd = new Random();

        //Väder API
        private static readonly string _url = "https://api.openweathermap.org/data/2.5/onecall?lat=59.2741668&lon=15.2139959&exclude=hourly,daily&units=metric&appid=7fd7db6d0ff6e8c22433efb93de3f6b6";
        private static HttpClient _client;
        private static HttpResponseMessage _response;
        
        //Device Client = Iot Device
        public static async Task SendMessageAsync(DeviceClient deviceClient)
        {
            _client = new HttpClient();

            while(true)
            {
                try
                {
                    _response = await _client.GetAsync(_url);
                    //om den får en 2xx status code
                    if (_response.IsSuccessStatusCode)
                    {
                        //Json raw meddenlanden {"bla": "bla" }
                        var result = await _response.Content.ReadAsStringAsync();
                        //konvertera till en class
                        var data = JsonConvert.DeserializeObject<TemperatureModel>(result);

                        //konvertera tillbaka till json 
                        var jsonTemp = JsonConvert.SerializeObject(data.Current.Temp);
                        var jsonHumidity = JsonConvert.SerializeObject(data.Current.Humidity);
                        //omvandlar till datorns språk (01) -> omvandlar till UTF8 (special tecken)
                        var payload = new Message(Encoding.UTF8.GetBytes($"Temperature: {jsonTemp} & Humidity: {jsonHumidity}"));
                        await deviceClient.SendEventAsync(payload);
                        


                        Console.WriteLine($"Message sent: Temperature: {jsonTemp} & Humidity: {jsonHumidity}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error:{ex.Message}");
                }
                
                    // ALTERNATIV - SLUMPMÄSSIGT TEMP OCH HUMIDITY UTAN API
                     
                     /* var data = new TemperatureModel
                            {
                                Temperature = rnd.Next(20, 30),
                                Humidity = rnd.Next(40, 60)
                            };

                            //konvertera till en json format { "temperature": 20, "humidity": 44 }
                            var json = JsonConvert.SerializeObject(data); 
           

                        //meddelande som man vill skicka kallas för payload. Message kommer från microsoft.azure.device.client. Formatera till UTF8 som inkludera öäå och omvandlar texten till bytes då datorn förstår bara 0 och 1
                        var payload = new Message(Encoding.UTF8.GetBytes(json));
                

                        await deviceClient.SendEventAsync(payload);
                    */

                
                //bromsa upp det med en delay på 30 sec istället att hela while loopen körs på 10ms i intervall
                await Task.Delay(3 * 1000);
            }





        }
        //Device Client = Iot Device
        public static async Task ReceiveMessageAsync(DeviceClient deviceClient)
        {
            while (true)
            {
                var payload = await deviceClient.ReceiveAsync();
           

                //om inte payload har fått något svar, så med continue kommer den bryta och gå tillbaka och börja om loopen
                if (payload == null)
                    continue;
                //Hämta 0 och 1 med getbytes och konvertera till en sträng med getstring som stödjer specialtecken med utf8
                Console.WriteLine($"Message received: {Encoding.UTF8.GetString(payload.GetBytes())}");

                //När meddelanden har mottagits tar den bort den från hubben
                await deviceClient.CompleteAsync(payload);
            }
        }

        //Service Client = Iot Hub
        public static async Task SendMessageToDeviceAsync(MAD.ServiceClient serviceClient, string targetDeviceId, string message)
        {
            var payload = new MAD.Message(Encoding.UTF8.GetBytes(message));
            await serviceClient.SendAsync(targetDeviceId, payload);

        }
    }
}