using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Network;
using GHIElectronics.TinyCLR.Devices.Spi;
using GHIElectronics.TinyCLR.Drivers.Microchip.Winc15x0;
using GHIElectronics.TinyCLR.Pins;

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
    public NetworkWifi()
    {
      // define the pins/controllers
      /// define the pins
      var enablePinNumber = SC20100.GpioPin.PA8;
      var chipSelectPinNumber = SC20100.GpioPin.PD15;
      var irqPinNumber = SC20100.GpioPin.PB12;
      var resetPinNumber = SC20100.GpioPin.PB13;
      var spiControllerName = SC20100.SpiBus.Spi3;
      var gpioControllerName = SC20100.GpioPin.Id;

      var enablePin = GpioController.GetDefault().OpenPin(enablePinNumber);
      enablePin.SetDriveMode(GpioPinDriveMode.Output);
      enablePin.Write(GpioPinValue.High);

      var chipselect = GpioController.GetDefault().OpenPin(chipSelectPinNumber);

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
      netInterfaceSettings.SpiApiName = spiControllerName;
      netInterfaceSettings.SpiSettings = settings;
      netInterfaceSettings.GpioApiName = gpioControllerName;
      netInterfaceSettings.InterruptPin = GpioController.GetDefault().OpenPin(irqPinNumber);
      netInterfaceSettings.InterruptEdge = GpioPinEdge.FallingEdge;
      netInterfaceSettings.InterruptDriveMode = GpioPinDriveMode.InputPullUp;
      netInterfaceSettings.ResetPin = GpioController.GetDefault().OpenPin(resetPinNumber);
      netInterfaceSettings.ResetActiveState = GpioPinValue.Low;

      // Wifi setting
      var wifiSettings = new WiFiNetworkInterfaceSettings()
      {
        Ssid = "guardnet5",
        Password = "H@tOne@)!^",
      };

      wifiSettings.DhcpEnable = true;
      wifiSettings.DynamicDnsEnable = true;
      wifiSettings.DnsAddresses = new IPAddress[] { new IPAddress(new byte[] { 8, 8, 8, 8 }) };

      var networkController = NetworkController.FromName(SC20100.NetworkController.ATWinc15x0);
      networkController.SetInterfaceSettings(wifiSettings);
      networkController.SetCommunicationInterfaceSettings(netInterfaceSettings);
      networkController.SetAsDefaultController();
      networkController.NetworkAddressChanged += NetworkController_NetworkAddressChanged;
      networkController.NetworkLinkConnectedChanged += NetworkController_NetworkLinkConnectedChanged;
      networkController.Enable();

      // get the mac address
      byte[] macAddress = Winc15x0Interface.GetMacAddress();
      Debug.WriteLine("WIFIMAC: " + macAddress[0].ToString("X") + "." + macAddress[1].ToString("X") + "." + macAddress[2].ToString("X") + "." + macAddress[3].ToString("X") + "." + macAddress[4].ToString("X") + "." + macAddress[5].ToString("X"));
 
      // sleep
      Thread.Sleep(Timeout.Infinite);
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
