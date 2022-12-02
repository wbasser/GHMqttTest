using System;
using System.Diagnostics;
using System.Text;
using GHIElectronics.TinyCLR.Networking.Mqtt;

namespace MqttDemo
{
  public class MqttHandler
  {
    // delegate/event variables
    public delegate void MqttConnectionDelegate(object sender);
    public event MqttConnectionDelegate OnMqttConnectionDelegate;

    // mqtt variables
    private Mqtt lclClient;
    private MqttClientSetting mqttClientSetting;
    private MqttConnectionSetting mqttConnectionSetting;
    private ushort intPacketId;

    // class initialization
    public MqttHandler(string broker, int port, string username, string userpass)
    {
      var mqttBroker = broker;
      var mqttPort = port;
      var mqttUserName = username;
      var mqttUserPass = userpass;

      try
      {
        // set the packet ID to 0
        intPacketId = 1;

        // create the client settings
        mqttClientSetting = new MqttClientSetting
        {
          BrokerName = mqttBroker,
          BrokerPort = mqttPort,
          ClientCertificate = null,
          CaCertificate = null,
          SslProtocol = System.Security.Authentication.SslProtocols.None
        };

        // crate the connection settings
        mqttConnectionSetting = new MqttConnectionSetting
        {
          ClientId = mqttUserName,
          Password = mqttUserPass
        };

        // create the connection
        lclClient = new Mqtt(mqttClientSetting);

        // set the delegates
        lclClient.ConnectedChanged += ConnectionChanged;

        // connect to the network
        lclClient.Connect(mqttConnectionSetting);
      }
      catch (Exception e)
      {
        Debug.WriteLine("Error on MQTT connection!");
      }
    }

    // get the connection status
    public bool IsConnected()
    {
      return (lclClient.IsConnected);
    }

    public void PublishToTopic(string strTopic, string strData)
    {
      lclClient.Publish(strTopic, Encoding.UTF8.GetBytes(strData), QoSLevel.MostOnce, false, intPacketId++);
    }

    // connection changed handler
    private void ConnectionChanged(object sender)
     {
      // call the delegate
      OnMqttConnectionDelegate?.Invoke(sender);
    }

  }
}
