//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

#if WINDOWS
using System.IO.Ports;
#endif //WINDOWS
#if ANDROID
using Gurux.Serial.Enums;
#endif //ANDROID
namespace Gurux.Serial.Views;

public partial class GXSettings : ContentPage
{
    private string? _settings;
#if ANDROID
    private List<GXPort> _ports = new List<GXPort>();
#endif //ANDROID
#if WINDOWS
    private List<string> _ports = new List<string>();
#endif //WINDOWS
    private List<int> _baudRates = new List<int>();
    private List<int> _dataBits = new List<int>();
    private List<Parity> _parity = new List<Parity>();
    private List<StopBits> _stopBits = new List<StopBits>();

    private readonly GXSerial _serial;

    /// <summary>
    /// If the dialog is shown as a modal dialog.
    /// </summary>
    private readonly bool _modal;

    /// <summary>
    /// Has user accept the changes.
    /// </summary>
    public bool Accept
    {
        get;
        private set;
    }
    /// <summary>
    /// User has close the dialog.
    /// </summary>
    public readonly EventWaitHandle Closing = new EventWaitHandle(false, EventResetMode.AutoReset);

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="serial"></param>
    public GXSettings(GXSerial serial, bool modal)
    {
        _serial = serial;
        _settings = serial.Settings;
        _modal = modal;
        InitializeComponent();
        if (!_modal)
        {
            OkBtn.IsVisible = false;
            CancenBtn.IsVisible = false;
        }
#if ANDROID
        _ports.AddRange(serial.GetPorts());
#endif //ANDROID
#if WINDOWS
        _ports.AddRange(GXSerial.GetPortNames());            
#endif //WINDOWS
#if ANDROID
        foreach (var it in _ports)
        {
            SerialPorts.Items.Add(it.Port);
        }
#endif //ANDROID
#if WINDOWS
        foreach (var it in _ports)
        {
            SerialPorts.Items.Add(it);
        }
#endif //WINDOWS
        //Select the serial port.
#if ANDROID
        //Select default port.
        SerialPorts.SelectedItem = serial.Port?.Port;
#endif //ANDROID
#if WINDOWS
        SerialPorts.SelectedItem = serial.PortName;
#endif //WINDOWS
        SerialPorts.SelectedIndexChanged += SelectedSerialPortChanged;

        _baudRates.AddRange(serial.GetAvailableBaudRates());
        foreach (var it in _baudRates)
        {
            BaudRates.Items.Add(it.ToString());
        }
        BaudRates.SelectedItem = serial.BaudRate.ToString();
        BaudRates.SelectedIndexChanged += SelectedBaudRateChanged;

        _dataBits.AddRange([7, 8]);
        foreach (var it in _dataBits)
        {
            DataBitList.Items.Add(it.ToString());
        }
        DataBitList.SelectedItem = serial.DataBits.ToString();
        DataBitList.SelectedIndexChanged += SelectedDataBitsChanged;

        _parity.AddRange([Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space]);
        foreach (var it in _parity)
        {
            ParityList.Items.Add(it.ToString());
        }
        ParityList.SelectedItem = serial.Parity.ToString();
        ParityList.SelectedIndexChanged += SelectedParityChanged;

        _stopBits.AddRange([StopBits.None, StopBits.One, StopBits.Two]);
        foreach (var it in _stopBits)
        {
            StopBitList.Items.Add(it.ToString());
        }
        StopBitList.SelectedItem = serial.StopBits.ToString();
        StopBitList.SelectedIndexChanged += SelectedStopBitChanged;
    }

    /// <summary>
    /// User has select a new serial port.
    /// </summary>
    private void SelectedSerialPortChanged(object? sender, EventArgs e)
    {
        int selectedIndex = SerialPorts.SelectedIndex;
        if (selectedIndex != -1)
        {
#if ANDROID
            _serial.Port = _ports[selectedIndex];
#endif //ANDROID
#if WINDOWS
            _serial.PortName = _ports[selectedIndex];
#endif //WINDOWS
        }
    }


    /// <summary>
    /// User has select a new baud rate.
    /// </summary>
    private void SelectedBaudRateChanged(object? sender, EventArgs e)
    {
#if ANDROID || WINDOWS
        _serial.BaudRate = Convert.ToInt32(BaudRates.SelectedItem);
#endif //ANDROID || WINDOWS
    }

    /// <summary>
    /// User has select a new data bits.
    /// </summary>
    private void SelectedDataBitsChanged(object? sender, EventArgs e)
    {
#if ANDROID || WINDOWS
        _serial.DataBits = Convert.ToInt32(DataBitList.SelectedItem);
#endif //ANDROID || WINDOWS
    }

    /// <summary>
    /// User has select a new parity.
    /// </summary>
    private void SelectedParityChanged(object? sender, EventArgs e)
    {
#if ANDROID || WINDOWS
        if (ParityList.SelectedItem is string str)
        {
            _serial.Parity = Enum.Parse<Parity>(str);
        }
#endif //ANDROID || WINDOWS
    }

    /// <summary>
    /// User has select a new stop bits.
    /// </summary>
    private void SelectedStopBitChanged(object? sender, EventArgs e)
    {
#if ANDROID || WINDOWS
        if (StopBitList.SelectedItem is string str)
        {
            _serial.StopBits = Enum.Parse<StopBits>(str);
        }
#endif //ANDROID || WINDOWS
    }

    /// <summary>
    /// Close settings dialog.
    /// </summary>
    private void OnOk(object sender, EventArgs e)
    {
        try
        {
            Accept = true;
            Closing.Set();
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "Close");
        }
    }
    /// <summary>
    /// Close settings dialog.
    /// </summary>
    private void OnCancel(object sender, EventArgs e)
    {
        try
        {
            _serial.Settings = _settings;

            Closing.Set();
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "Close");
        }
    }

}