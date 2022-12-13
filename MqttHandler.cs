using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
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
    private System.Security.Authentication.SslProtocols SSLProtocol;
    private ushort intPacketId;

    // class initialization
    public MqttHandler(string clientid, string broker, int port, string username, string userpass, X509Certificate certificate)
    {
      var mqttClientId = clientid;
      var mqttBroker = broker;
      var mqttPort = port;
      var mqttUserName = username;
      var mqttUserPass = userpass;
      var mqttCertificate = certificate;
      if (mqttCertificate != null)
      {
        SSLProtocol = System.Security.Authentication.SslProtocols.Tls12;
      }
      else
      {
       SSLProtocol = System.Security.Authentication.SslProtocols.None;
      }

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
          CaCertificate = mqttCertificate,
          SslProtocol = SSLProtocol,
        };

        // crate the connection settings
        mqttConnectionSetting = new MqttConnectionSetting
        {
          ClientId = mqttClientId,
          UserName = mqttUserName,
          Password = mqttUserPass
        };

        // create the connection
        lclClient = new Mqtt(mqttClientSetting);

        // set the delegates
        lclClient.ConnectedChanged += ConnectionChanged;

        // connect to the network
        ConnectReturnCode returncode = lclClient.Connect(mqttConnectionSetting);
        Debug.WriteLine(returncode.ToString()); 
      }
      catch (Exception e)
      {
        Debug.WriteLine("Error on MQTT connection-" + e.Message);
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
