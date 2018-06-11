/******************************************************************************************
* MIT License
*
* Copyright (c) 2013-2017 Sensel, Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
******************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Sensel
{

    [StructLayout(LayoutKind.Sequential)]
    internal class SenselFrameData
    {
        public byte content_bit_mask;
        public Int32 lost_frame_count;
        public byte n_contacts;
        public IntPtr contacts;
        public IntPtr force_array;
        public IntPtr labels_array;
        public IntPtr accel_data;
    }

    public static class SenselLib
    {
        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetDeviceList(ref SenselDeviceList device_list);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselOpenDeviceByID(ref IntPtr handle, byte idx);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselClose(IntPtr handle);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselSoftReset(IntPtr handle);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetSensorInfo(IntPtr handle, ref SenselSensorInfo info);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselAllocateFrameData(IntPtr handle, ref SenselFrameData data);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselFreeFrameData(IntPtr handle, SenselFrameData data);
        
        [DllImport("LibSensel")]
        internal extern static SenselStatus senselSetScanDetail(IntPtr handle, SenselScanDetail detail);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetScanDetail(IntPtr handle, ref SenselScanDetail detail);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetSupportedFrameContent(IntPtr handle, ref byte content);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselSetFrameContent(IntPtr handle, byte content);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetFrameContent(IntPtr handle, ref byte content);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselStartScanning(IntPtr handle);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselStopScanning(IntPtr handle);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselReadSensor(IntPtr handle);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetNumAvailableFrames(IntPtr handle, ref Int32 num_avail_frames);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetFrame(IntPtr handle, SenselFrameData data);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselSetDynamicBaselineEnabled(IntPtr handle, byte val);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetDynamicBaselineEnabled(IntPtr handle, ref byte val);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetNumAvailableLEDs(IntPtr handle, ref byte num_leds);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetMaxLEDBrightness(IntPtr handle, ref byte max_brightness);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselSetLEDBrightness(IntPtr handle, byte led_id, UInt16 brightness);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetLEDBrightness(IntPtr handle, byte led_id, ref UInt16 brightness);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetPowerButtonPressed(IntPtr handle, ref byte num_leds);

        [DllImport("LibSensel")]
        internal extern static SenselStatus senselGetFirmwareInfo(IntPtr handle, ref SenselFirmwareInfo fw_info);
    }
}
