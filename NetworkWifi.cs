using System;
using System.Diagnostics;
using System.Net;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;

namespace MqttDemo
{
  public class NetworkWifi
  {
    // delegate/event variables
    public delegate void NetworkWifiConnectionDelegate(NetworkController sender, NetworkLinkConnectedChangedEventArgs e);
    public delegate void NetworkWifiAddressChangedDelegate(NetworkController sender, NetworkAddressChangedEventArgs e);
    public event NetworkWifiConnectionDelegate OnNetworkWifiConnectionDelegate;
    public event NetworkWifiAddressChangedDelegate OnNetworkWifiAddressChangedDelegate;

    // main class
    public NetworkWifi(String strSSID, String strPassword, String strSpiName, String strGpioName, String strNetName, int iEnablePin, int iChipSelPin, int iIrqPin, int iResetPin)
    {
      // create the enable pin
      var enablePin = GpioController.GetDefault().OpenPin(iEnablePin);
      enablePin.SetDriveMode(GpioPinDriveMode.Output);
      enablePin.Write(GpioPinValue.High);

      // create the chip select
      var chipselect = GpioController.GetDefault().OpenPin(iChipSelPin);

      // set up the connection setting
      SpiNetworkCommunicationInterfaceSettings netInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings();
      var settings = new SpiConnectionSettings()
      {
        ChipSelectLine = chipselect,
        ClockFrequency = 4000000,
        Mode = SpiMode.Mode0,
        ChipSelectType = SpiChipSelectType.Gpio,
        ChipSelectHoldTime = TimeSpan.FromTicks(10),
        ChipSelectSetupTime = TimeSpan.FromTicks(10)
      };

      // netInterfaceSettings
      netInterfaceSettings.SpiApiName = strSpiName;
      netInterfaceSettings.SpiSettings = settings;
      netInterfaceSettings.GpioApiName = strGpioName;
      netInterfaceSettings.InterruptPin = GpioController.GetDefault().OpenPin(iIrqPin);
      netInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
      netInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
      netInterfaceSettings.ResetPin = GpioController.GetDefault().OpenPin(iResetPin);
      netInterfaceSettings.ResetActiveState = GpioPinValue.Low;

      // Wifi setting
      var wifiSettings = new WiFiNetworkInterfaceSettings()
      {
        Ssid = strSSID,
        Password = strPassword,
      };
      wifiSettings.DhcpEnable = true;
      wifiSettings.DynamicDnsEnable = true;
      wifiSettings.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 8, 8, 8, 8 }) };

      var networkController = NetworkController.FromName(strNetName);
      networkController.SetInterfaceSettings(wifiSettings);
      networkController.SetCommunicationInterfaceSettings(netInterfaceSettings);
      networkController.SetAsDefaultController();
      networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;
      networkController.NetworkLinkConnectedChanged += NetworkController_NetworkLinkConnectedChanged;
      networkController.Enable();

      // get the mac address
      byte[] macAddress = Winc15x0Interface.GetMacAddress();
      Debug.WriteLine("WIFIMAC: " + macAddress[0].ToString("X") + "." + macAddress[1].ToString("X") + "." + macAddress[2].ToString("X") + "." + macAddress[3].ToString("X") + "." + macAddress[4].ToString("X") + "." + macAddress[5].ToString("X"));
    }

    private void NetworkController_NetworkLinkConnectedChanged(NetworkController sender, NetworkLinkConnectedChangedEventArgs e)
    {
      // invoke the delegate
      OnNetworkWifiConnectionDelegate?.Invoke(sender, e);
    }

    private void NetworkController_NetworkAddressChanged(NetworkController sender, NetworkAddressChangedEventArgs e)
    {
      // call the delegate
      OnNetworkWifiAddressChangedDelegate?.Invoke(sender, e);
    }
  }
}
