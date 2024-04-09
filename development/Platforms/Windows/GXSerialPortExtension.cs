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

using Microsoft.Win32.SafeHandles;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Gurux.Serial
{
    internal static class GXSerialPortExtension
    {
        public static byte GetChar(this Gurux.Serial.GXSerial port, string name)
        {
            if (port == null)
            {
                throw new NullReferenceException();
            }
            if (port.BaseStream == null)
            {
                throw new InvalidOperationException("Cannot change X chars until after the port has been opened.");
            }
            try
            {
                // Get the base stream and its type which is System.IO.Ports.SerialStream
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();

                // Get the Win32 file handle for the port
                SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);

                // Get the value of the private DCB field (a value type)
                FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);

                // The type of dcb is Microsoft.Win32.UnsafeNativeMethods.DCB which is an internal type. We can only access it through reflection.
                Type dcbType = dcbValue.GetType();
                return Convert.ToByte(dcbType.GetField(name).GetValue(dcbValue));
            }
            catch (System.Security.SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception ex)
            {
                throw new ApplicationException("SetXonXoffChars has failed due to incorrect assumptions about System.IO.Ports.SerialStream which is an internal type.", ex);
            }
        }

        public static void SetChar(Gurux.Serial.GXSerial port, string name, byte value)
        {
            if (port == null)
            {
                throw new NullReferenceException();
            }
            if (port.BaseStream == null)
            {
                throw new InvalidOperationException("Cannot change X chars until after the port has been opened.");
            }

            try
            {
                // Get the base stream and its type which is System.IO.Ports.SerialStream
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();

                // Get the Win32 file handle for the port
                SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);

                // Get the value of the private DCB field (a value type)
                FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);

                // The type of dcb is Microsoft.Win32.UnsafeNativeMethods.DCB which is an internal type. We can only access it through reflection.
                Type dcbType = dcbValue.GetType();
                dcbType.GetField(name).SetValue(dcbValue, value);

                // We need to call SetCommState but because dcbValue is a private type, we don't have enough
                //  information to create a p/Invoke declaration for it. We have to do the marshalling manually.

                // Create unmanaged memory to copy DCB into
                IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(dcbValue));
                try
                {
                    // Copy their DCB value to unmanaged memory
                    Marshal.StructureToPtr(dcbValue, hGlobal, false);

                    // Call SetCommState
                    if (!SetCommState(portFileHandle, hGlobal))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Update the BaseStream.dcb field if SetCommState succeeded
                    dcbFieldInfo.SetValue(baseStream, dcbValue);
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                        Marshal.FreeHGlobal(hGlobal);
                }
            }
            catch (System.Security.SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception ex)
            {
                throw new ApplicationException("SetXonXoffChars has failed due to incorrect assumptions about System.IO.Ports.SerialStream which is an internal type.", ex);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(Microsoft.Win32.SafeHandles.SafeFileHandle hFile, IntPtr lpDCB);
    }
}
