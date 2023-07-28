using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;
using System.Security.Cryptography.X509Certificates;

namespace MqttDemo
{
  internal class Program
  {
    // declare the variables
    private static NetworkWifi lclNetWifi;
    private static MqttHandler lclMqttHandler;
    private static bool flagMqttConnected = false;
    private static bool flagNetConnected = false;

    public static string  mqttUrl = "test.mosquitto.org";
    public static int mqttPort = 1883;
    public static string mqttName = "";
    public static string mqttPass = "";
    public static string mqttClientId = "MqttTest";
    public static X509Certificate mqttCert;

    static void Main()
    {
      // create the the WIFI network
      lclNetWifi = new NetworkWifi();
      lclNetWifi.OnNetworkWifiConnectionDelegate += NetworkWifiConnectionHandler;
      lclNetWifi.OnNetworkWifiAddressChangedDelegate += NetworkWifiAddressHandler;
      
      // create the status led
      var led = GpioController.GetDefault().OpenPin(FEZFeather.GpioPin.Led);
      led.SetDriveMode(GpioPinDriveMode.Output);

      // get the certificate
      mqttCert = new X509Certificate(MqttDemo.Resources.GetBytes(MqttDemo.Resources.BinaryResources.__adafruit));
  
      // while loop
      while (true)
      {
        led.Write(GpioPinValue.High);
        Thread.Sleep(500);
        led.Write(GpioPinValue.Low);
        Thread.Sleep(500);

        if (flagMqttConnected)
        { 
          if (lclMqttHandler.IsConnected())
          {
          }
        }
      };
    }

    // connection handler
    public static void NetworkWifiConnectionHandler(NetworkController sender, NetworkLinkConnectedChangedEventArgs e)
    {
      if (sender.GetLinkConnected())
      {
        Debug.WriteLine("WIFI Connected!");

        // set the flag
        flagNetConnected = true;
      }
      else
      {
        Debug.WriteLine("WIFI Disconnected!");

        // clear the flag
        flagNetConnected = false;
      }
    }

    // address changed handler
    public static void NetworkWifiAddressHandler(NetworkController sender, NetworkAddressChangedEventArgs e)
    {
      if (flagNetConnected)
      {
        // get the IP properties
        var ipProperties = sender.GetIPProperties();
        var address = ipProperties.Address.GetAddressBytes();
        Debug.WriteLine("WIFIADDR:" + address[0] + "." + address[1] + "." + address[2] + "." + address[3]);

        // list the DNS servers
        var dnsServers = ipProperties.DnsAddresses;
        foreach (IPAddress dns in dnsServers)
        {
          string dnsaddr = dns.ToString();
          Debug.WriteLine("WIFIDNS:" + dnsaddr);
        }

        // select the default network
        if (BitConverter.ToUInt32(address, 0) != 0)
        {
          IPHostEntry mqttserver = Dns.GetHostEntry(mqttUrl);
          Debug.WriteLine("MQTT Server:" + mqttserver.AddressList[0].ToString());
          Debug.WriteLine("Connecting to the MQTT server...");

          // create the mqtt handler
          lclMqttHandler = new MqttHandler(mqttClientId, mqttUrl, mqttPort, mqttName, mqttPass, mqttCert);
          lclMqttHandler.OnMqttConnectionDelegate += MqttConnectionHandler;
        }
      }
    }

    // mqtt connection changed handler
    public static void MqttConnectionHandler(object sender)
    {
      // print status
      Debug.WriteLine("MQTT Connection changed!");

      flagMqttConnected = true;
    }
  }
}
