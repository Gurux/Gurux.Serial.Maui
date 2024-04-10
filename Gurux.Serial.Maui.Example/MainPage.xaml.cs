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

using Gurux.Common;
using Gurux.Common.Enums;
using System.Text;

namespace Gurux.Serial.Example
{
    public partial class MainPage : ContentPage
    {
        GXSerial _serial;
#if ANDROID
        private List<GXPort> _ports = new List<GXPort>();
#endif //ANDROID
#if WINDOWS
        private List<string> _ports = new List<string>();
#endif //WINDOWS

        public MainPage()
        {
            InitializeComponent();
#if ANDROID
            _serial = new GXSerial(Platform.CurrentActivity);
            _ports.AddRange(_serial.GetPorts());
#endif //ANDROID
#if WINDOWS
            _serial = new GXSerial();
            _ports.AddRange(GXSerial.GetPortNames());
#endif //WINDOWS
#if __IOS__
            throw new Exception("IOS is not supported at the moment.");
#endif //__IOS__            
            _serial.OnError += OnSerialPortError;
            _serial.OnMediaStateChange += SerialPortStateChange;
            _serial.OnReceived += OnSerialPortReceived;
            SerialPorts.SelectedIndexChanged += SelectedSerialPortChanged;
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
#if ANDROID || WINDOWS
            //Select the first serial port.
            if (_ports.Any())
            {
                SerialPorts.SelectedIndex = 0;
            }
#endif //ANDROID || WINDOWS
        }

        private void OnSerialPortReceived(object sender, ReceiveEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsHex.IsChecked)
                {
                    ReceivedData.Text += GXCommon.ToHex((byte[])e.Data);
                }
                else
                {
                    ReceivedData.Text += Encoding.ASCII.GetString((byte[])e.Data);
                }
            });
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
            SerialPortStateChange(this, new MediaStateEventArgs()
            {
                State = MediaState.Closed
            });
        }

        private void SerialPortStateChange(object sender, MediaStateEventArgs e)
        {
            try
            {
                bool bOpen = e.State == MediaState.Open;
                OpenBtn.IsVisible = !bOpen;
                CloseBtn.IsVisible = bOpen;
                Properties.IsEnabled = !bOpen;
                SendData.IsEnabled = bOpen;
                SendBtn.IsEnabled = bOpen;
                ReceivedData.IsEnabled = bOpen;
                SerialPorts.IsEnabled = !bOpen;
            }
            catch (Exception ex)
            {
                DisplayAlert("Alert", ex.Message, "Close");
            }
        }

        /// <summary>
        /// Show serial port error.
        /// </summary>
        private void OnSerialPortError(object sender, Exception ex)
        {
            try
            {
                _serial.Close();
                DisplayAlert("Alert", ex.Message, "Close");
            }
            catch (Exception Ex)
            {
                DisplayAlert("Alert", Ex.Message, "Close");
            }
        }

        /// <summary>
        /// Send data to the serial port.
        /// </summary>
        private void OnSend(object sender, EventArgs e)
        {
            try
            {
                byte[] data;
                if (IsHex.IsChecked)
                {
                    data = GXCommon.HexToBytes(SendData.Text);
                }
                else
                {
                    data = ASCIIEncoding.ASCII.GetBytes(SendData.Text);
                }
                _serial.Send(data);
            }
            catch (Exception Ex)
            {
                DisplayAlert("Send failed", Ex.Message, "Close");
            }
        }

        /// <summary>
        /// Open selected serial port.
        /// </summary>
        private void OnOpen(object sender, EventArgs e)
        {
            try
            {
                _serial.Open();
            }
            catch (Exception ex)
            {
                DisplayAlert("Alert", ex.Message, "Close");
            }
        }

        /// <summary>
        /// Serial port properties are shown.
        /// </summary>
        private async void OnPropertiesAsync(object sender, EventArgs e)
        {
            try
            {
                await _serial.PropertiesAsync(Navigation);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Alert", ex.Message, "Close");
            }
        }

        /// <summary>
        /// Close selected serial port.
        /// </summary>
        private void OnClose(object sender, EventArgs e)
        {
            try
            {
                _serial.Close();
            }
            catch (Exception ex)
            {
                DisplayAlert("Alert", ex.Message, "Close");
            }
        }
    }

}
