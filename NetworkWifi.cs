using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
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

    // local variables
    private static NetworkController networkController = null;

    // main class
    public NetworkWifi(String strSSID, String strPassword, String strSpiName, String strGpioName, String strNetName, int iEnablePin, int iChipSelPin, int iIrqPin, int iResetPin, bool bSetAsDefault)
    {
      // get the local GPIO controller
      GpioController lclGpio = GpioController.GetDefault();
      // create the enable pin
      GpioPin pinEnable = lclGpio.OpenPin(iEnablePin);
      pinEnable.SetDriveMode(GpioPinDriveMode.Output);
      pinEnable.Write(GpioPinValue.High);

      // create the chip select
      GpioPin pinChipSelect = lclGpio.OpenPin(iChipSelPin);

      // set up the connection setting
      var settings = new SpiConnectionSettings()
      {
        ChipSelectLine = pinChipSelect,
        ClockFrequency = 4000000,
        Mode = SpiMode.Mode0,
        ChipSelectType = SpiChipSelectType.Gpio,
        ChipSelectHoldTime = TimeSpan.FromTicks(10),
        ChipSelectSetupTime = TimeSpan.FromTicks(10)
      };

      // netInterfaceSettings
      SpiNetworkCommunicationInterfaceSettings netInterfaceSettings = new SpiNetworkCommunicationInterfaceSettings();
      netInterfaceSettings.SpiApiName = strSpiName;
      netInterfaceSettings.SpiSettings = settings;
      netInterfaceSettings.GpioApiName = strGpioName;
      netInterfaceSettings.InterruptPin = lclGpio.OpenPin(iIrqPin);
      netInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
      netInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
      netInterfaceSettings.ResetPin = lclGpio.OpenPin(iResetPin);
      netInterfaceSettings.ResetActiveState = GpioPinValue.Low;

      // Wifi setting
      var wifiSettings = new WiFiNetworkInterfaceSettings();
      wifiSettings.Ssid = strSSID;
      wifiSettings.Password = strPassword;
      wifiSettings.DhcpEnable = true;
      wifiSettings.DynamicDnsEnable = true;
      wifiSettings.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 8, 8, 8, 8 }) };

      // set up the network controller
      networkController = NetworkController.FromName(strNetName);
      networkController.SetInterfaceSettings(wifiSettings);
      networkController.SetCommunicationInterfaceSettings(netInterfaceSettings);
      networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;
      networkController.NetworkLinkConnectedChanged += NetworkController_NetworkLinkConnectedChanged;
      networkController.Enable();

      // check for default
      if (bSetAsDefault)
      {
        // set as default
        networkController.SetAsDefaultController();
      }
    }

    public void SetDefaultNetworkState(bool bState)
    {
      if (bState)
      {
        networkController.SetAsDefaultController();
      }
    }

    public int GetSignalStrength()
    {
      int iRssi = Winc15x0Interface.GetRssi();

      // return the signal strenth
      return(iRssi);
    }

    public byte[] GetMacAddress()
    {
      // get the mac address
      byte[] mac = Winc15x0Interface.GetMacAddress();

      // return it
      return mac;
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
