using System;
using System.Diagnostics;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Network;


namespace MqttDemo
{
  internal class Program
  {
    // declare the variables
    private static NetworkWifi lclNetWifi;
    private static MqttHandler lclMqttHandler;
    private static bool flagMqttConnected = false;

    public static string  mqttUrl = "killerbunnies.strongarmtech.io";
    public static int mqttPort = 1883;
    public static string mqttName = "bbasser";
    public static string mqttPass = "Triangle12";

    static void Main()
    {
      // create the the WIFI network
      lclNetWifi = new NetworkWifi();
      lclNetWifi.OnNetworkWifiConnectionDelegate += NetworkWifiConnectionHandler;
      lclNetWifi.OnNetworkWifiAddressChangedDelegate += NetworkWifiAddressHandler;

      var mqttTopic = "SATTEST";
      var mqttData = "TestData";

      // while loop
      while (true)
      {
        Thread.Sleep(1000);

        if (flagMqttConnected)
        { 
          if (lclMqttHandler.IsConnected())
          {
            lclMqttHandler.PublishToTopic(mqttTopic, mqttData);
            Debug.WriteLine("Publish");
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
      }
      else
      {
        Debug.WriteLine("WIFI Disconnected!");
      }
    }

    // address changed handler
    public static void NetworkWifiAddressHandler(NetworkController sender, NetworkAddressChangedEventArgs e)
    {
      // get the IP properties
      var ipProperties = sender.GetIPProperties();
      var address = ipProperties.Address.GetAddressBytes();
      Debug.WriteLine("WIFIADDR:" + address[0] + "." + address[1] + "." + address[2] + "." + address[3]);

      // select the default network
      if (BitConverter.ToUInt32(address, 0) != 0)
      {
        Debug.WriteLine("Connecting to the MQTT server...");

        // create the mqtt handler
        lclMqttHandler = new MqttHandler(mqttUrl, mqttPort, mqttName, mqttPass);
        lclMqttHandler.OnMqttConnectionDelegate += MqttConnectionHandler;
        flagMqttConnected = true;
      }
    }

    // mqtt connection changed handler
    public static void MqttConnectionHandler(object sender)
    {
      // print status
      Debug.WriteLine("MQTT Connection changed!");
    }
  }
}
