using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using GrovePi;
using GrovePi.Sensors;
using GrovePi.I2CDevices;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace UniElevatorPi
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceClient deviceClient;
        private string iotHubHostname = "{HostName}.azure-devices.net";
        private string deviceId = "ElevatorPi";
        private string deviceKey = "{Insert DeviceKey}";
        private IDHTTemperatureAndHumiditySensor tempHumSensor;
        private ILightSensor lightSensor;
        private ISoundSensor soundSensor;
        private IRgbLcdDisplay lcdDisplay;
        private IButtonSensor button;
        private bool elevatorFailed = false;
        private ILed warnLED;

        public MainPage()
        {
            this.InitializeComponent();
            deviceClient = DeviceClient.Create(iotHubHostname,
                new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            this.tempHumSensor = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin7, DHTModel.Dht11);
            this.lightSensor = DeviceFactory.Build.LightSensor(Pin.AnalogPin2);
            this.soundSensor = DeviceFactory.Build.SoundSensor(Pin.AnalogPin1);
            this.lcdDisplay = DeviceFactory.Build.RgbLcdDisplay();
            this.button = DeviceFactory.Build.ButtonSensor(Pin.DigitalPin3);
            this.warnLED = DeviceFactory.Build.Led(Pin.DigitalPin4);

            resetColor();

            SendDeviceToCloudMessageAsync();
            checkButtonAsync();
        }

        private async void checkButtonAsync()
        {
            while (true)
            {
                try
                {
                    if (elevatorFailed && button.CurrentState.Equals(SensorStatus.On))
                    {
                        repairElevator();
                    }
                }
                catch
                {
                    // *shrug*
                }
                await Task.Delay(10);
            }
        }

        private void resetColor()
        {
            this.lcdDisplay.SetBacklightRgb(255, 255, 255);
        }

        private void repairElevator()
        {
            this.elevatorFailed = false;
            this.lcdDisplay.SetBacklightRgb(10, 255, 10);
            Task.Delay(5000).Wait();
            resetColor();
        }

        private void failElevator()
        {
            this.elevatorFailed = true;
            this.lcdDisplay.SetBacklightRgb(255, 10, 10);
        }

        private async Task ReceiveCloudToDeviceMessagesAsync()
        {
            var receivedMessage = await deviceClient.ReceiveAsync();
            if(receivedMessage != null)
            {
                var messageContent = Encoding.UTF8.GetString(receivedMessage.GetBytes());

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }

        private async void SendDeviceToCloudMessageAsync()
        {
            while (true)
            {
                tempHumSensor.Measure();
                var temp = tempHumSensor.TemperatureInCelsius;
                if (double.IsNaN(temp)) continue;
                var hum = tempHumSensor.Humidity;
                if (double.IsNaN(hum)) continue;

                var light = lightSensor.SensorValue();
                if (light < 1) continue;
                
                var sound = soundSensor.SensorValue();
                if (sound < 1) continue;

                if(!elevatorFailed && (hum > 50))
                {
                    failElevator();
                }

                lcdDisplay.SetText(String.Format("Temp {0}, Hum {1}\nLgt {2}, Snd {3}", temp, hum, light, sound));

                var tempHumDataPoint = new
                {
                    id = deviceId,
                    date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    temperature = temp,
                    humidity = hum,
                    brightness = light,
                    sound = sound
                };

                var json = JsonConvert.SerializeObject(tempHumDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(json));
                try
                {
                    await deviceClient.SendEventAsync(message);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    warnLED.AnalogWrite(150);
                    Task.Delay(50).Wait();
                    warnLED.AnalogWrite(0);
                    // Screw you guys, I'm going home.
                }
                Task.Delay(1000).Wait();
            }
            

        }
    }
}
