using Microsoft.Azure.Devices.Client;
using SharedLibraries.Models;
using SharedLibraries.Services;
using System;

namespace Uppgift3
{
    class Program
    {
        private static readonly string _conn = "HostName=ec-win20-iothub-mw.azure-devices.net;DeviceId=consoleapp;SharedAccessKey=zHGjUjbxSgfEQnyNW/HuJc5mpBSErc2vvapFN2nmp6Q=";

        private static readonly DeviceClient deviceClient = 
            //uppkoppling till enheten som använder mqtt protokollen som är som http fast en mer slimmad version anpassad för skicka meddelande
            DeviceClient.CreateFromConnectionString(_conn, TransportType.Mqtt);
        static void Main(string[] args)
        {
            //GetAwaiter körs det på async och man slipper skriva static async void Main och sätta dit await i kodblocket 
            DeviceService.SendMessageAsync(deviceClient).GetAwaiter();
            DeviceService.ReceiveMessageAsync(deviceClient).GetAwaiter();

            Console.ReadKey();
        }
    }
}
