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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace Sensel
{
    public enum SenselStatus
    {
        SENSEL_OK = 0,
        SENSEL_ERROR = -1
    }

    public enum SenselScanMode
    {
        SCAN_MODE_DISABLE,
        SCAN_MODE_SYNC,
        SCAN_MODE_ASYNC
    }

    public enum SenselScanDetail
    {
        SCAN_DETAIL_HIGH = 0,       // Scan at full resolution
        SCAN_DETAIL_MEDIUM = 1,     // Scan at half resolution
        SCAN_DETAIL_LOW = 2 ,       // Scan at quarter resolution
        SCAN_DETAIL_UNKNOWN = 3
    }

    public enum SenselContactState
    {
        CONTACT_INVALID = 0,        // Contact is invalid
        CONTACT_START = 1,          // Contact has started
        CONTACT_MOVE = 2,           // Contact has moved
        CONTACT_END = 3             // Contact has ended
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselSensorInfo
    {
        public byte max_contacts;
        public UInt16 num_rows;
        public UInt16 num_cols;
        public float width;
        public float height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselFirmwareInfo
    {
        public byte     fw_protocol_version;
        public byte     fw_version_major;
        public byte     fw_version_minor;
        public UInt16   fw_version_build;
        public byte     fw_version_release;
        public UInt16   device_id;
        public byte     device_revision;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselContact
    {
        public byte content_bit_mask;
        public byte id;
        public Int32 state;
        public float x_pos;
        public float y_pos;
        public float total_force;
        public float area;
        public float orientation;
        public float major_axis;
        public float minor_axis;
        public float delta_x;
        public float delta_y;
        public float delta_force;
        public float delta_area;
        public float min_x;
        public float min_y;
        public float max_x;
        public float max_y;
        public float peak_x;
        public float peak_y;
        public float peak_force;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselAccelData
    {
        public UInt32 x;
        public UInt32 y;
        public UInt32 z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class SenselFrame
    {
        public byte content_bit_mask;
        public int lost_frame_count;
        public byte n_contacts;
        public List<SenselContact> contacts;
        public float[] force_array;
        public byte[] labels_array;
        public SenselAccelData accel_data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselDeviceID
    {
        public byte idx;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] serial_num;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] com_port;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SenselDeviceList
    {
        public byte num_devices;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public SenselDeviceID[] devices;
    }

    public class SenselDevice
    {
        public const byte FRAME_CONTENT_PRESSURE_MASK   = 0x01;  // Mask indicating that the frame includes pressure data
        public const byte FRAME_CONTENT_LABELS_MASK     = 0x02;  // Mask indicating that the frame includes labels data
        public const byte FRAME_CONTENT_CONTACTS_MASK   = 0x04;  // Mask indicating that the frame includes contacts data
        public const byte FRAME_CONTENT_ACCEL_MASK      = 0x08;  // Mask indicating that the frame includes acceleromter data

        public const byte CONTACT_MASK_ELLIPSE          = 0x01;  // Mask indicating that the contact data contains ellipse info
        public const byte CONTACT_MASK_DELTAS           = 0x02;  // Mask indicating that the contact data contains deltas info
        public const byte CONTACT_MASK_BOUNDING_BOX     = 0x04;  // Mask indicating that the contact data contains bound box info
        public const byte CONTACT_MASK_PEAK             = 0x08;  // Mask indicating that the contact data contains peak info

        protected IntPtr handle;
        protected SenselSensorInfo sensor_info;
        protected SenselFirmwareInfo fw_info;
        protected SenselFrame frame;
        internal SenselFrameData frame_data;

        public static SenselDeviceList GetDeviceList()
        {
            SenselDeviceList list = new SenselDeviceList();
            list.num_devices = 0;
            SenselLib.senselGetDeviceList(ref list);
            return list;
        }

        public SenselDevice()
        {
            handle = new IntPtr(0);
            frame = new SenselFrame();
            frame_data = new SenselFrameData();
            sensor_info = new SenselSensorInfo();
            fw_info = new SenselFirmwareInfo();
        }

        public void OpenDeviceByID(byte idx)
        {
            if(SenselLib.senselOpenDeviceByID(ref handle, idx) != SenselStatus.SENSEL_OK)
                throw SenselException();
            GetSensorInfo();
            AllocateFrameData();
        }

        public void Close()
        {
            if (SenselLib.senselClose(handle) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public void SoftReset()
        {
            if (SenselLib.senselSoftReset(handle) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public SenselSensorInfo GetSensorInfo()
        {
            if (SenselLib.senselGetSensorInfo(handle, ref sensor_info) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return sensor_info;
        }
        
        private void AllocateFrameData()
        {
            frame.contacts = new List<SenselContact>();
            frame.force_array = new float[sensor_info.num_rows * sensor_info.num_cols];
            frame.labels_array = new byte[sensor_info.num_rows * sensor_info.num_cols];
            frame.accel_data = new SenselAccelData();
            if (SenselLib.senselAllocateFrameData(handle, ref frame_data) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public void SetScanDetail(SenselScanDetail detail)
        {
            if (SenselLib.senselSetScanDetail(handle, detail) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public SenselScanDetail GetScanDetail()
        {
            SenselScanDetail detail = 0;
            if (SenselLib.senselGetScanDetail(handle, ref detail) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return detail;
        }

        public byte GetSupportedFrameContent()
        {
            byte content = 0;
            if (SenselLib.senselGetSupportedFrameContent(handle, ref content) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return content;
        }

        public void SetFrameContent(byte content)
        {
            if (SenselLib.senselSetFrameContent(handle, content) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public byte GetFrameContent()
        {
            byte content = 0;
            if (SenselLib.senselGetFrameContent(handle, ref content) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return content;
        }

        public void StartScanning()
        {
            if (SenselLib.senselStartScanning(handle) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public void StopScanning()
        {
            if (SenselLib.senselStopScanning(handle) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public void ReadSensor()
        {
            if (SenselLib.senselReadSensor(handle) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public int GetNumAvailableFrames()
        {
            Int32 num_frames = 0;
            if (SenselLib.senselGetNumAvailableFrames(handle, ref num_frames) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return (int)num_frames;
        }

        public SenselFrame GetFrame()
        {
            if (SenselLib.senselGetFrame(handle, frame_data) != SenselStatus.SENSEL_OK)
                throw SenselException();
            CopyFrameData();
            return frame;
        }

        private void CopyFrameData()
        {
            frame.content_bit_mask = frame_data.content_bit_mask;
            frame.lost_frame_count = frame_data.lost_frame_count;
            frame.n_contacts = 0;
            if ((frame.content_bit_mask & FRAME_CONTENT_CONTACTS_MASK) > 0)
            {
                frame.n_contacts = frame_data.n_contacts;
                frame.contacts.Clear();
                // Use UnsafeUtility to avoid GC memory allocation.
                unsafe {
                    for (int i = 0; i < frame.n_contacts; i++)
                        frame.contacts.Add(Unity.Collections.LowLevel.Unsafe.UnsafeUtility.ReadArrayElement<SenselContact>((void*)frame_data.contacts, i));
                }
                /*
                long ptr_index = (frame_data.contacts).ToInt64();
                for (int i = 0; i < frame.n_contacts; i++)
                {
                    IntPtr c_ptr = new IntPtr(ptr_index);
                    frame.contacts.Add((SenselContact)Marshal.PtrToStructure(c_ptr, typeof(SenselContact)));
                    ptr_index += Marshal.SizeOf(typeof(SenselContact));
                }
                */
            }
            if ((frame.content_bit_mask & FRAME_CONTENT_PRESSURE_MASK) > 0)
                Marshal.Copy(frame_data.force_array, frame.force_array, 0, frame.force_array.Length);
            if ((frame.content_bit_mask & FRAME_CONTENT_LABELS_MASK) > 0)
                Marshal.Copy(frame_data.labels_array, frame.labels_array, 0, frame.labels_array.Length);
            if ((frame.content_bit_mask & FRAME_CONTENT_ACCEL_MASK) > 0)
                frame.accel_data = (SenselAccelData)Marshal.PtrToStructure(frame_data.accel_data, typeof(SenselAccelData));
        }

        public byte GetNumAvailableLEDs()
        {
            byte num_leds = 0;
            if (SenselLib.senselGetNumAvailableLEDs(handle, ref num_leds) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return num_leds;
        }

        public byte GetMaxLEDBrightness()
        {
            byte max_brightness = 0;
            if (SenselLib.senselGetMaxLEDBrightness(handle, ref max_brightness) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return max_brightness;
        }

        public void SetLEDBrightness(byte led_id, UInt16 brightness)
        {
            if (SenselLib.senselSetLEDBrightness(handle, led_id, brightness) != SenselStatus.SENSEL_OK)
                throw SenselException();
        }

        public UInt16 GetLEDBrightness(byte led_id)
        {
            UInt16 brightness = 0;
            if (SenselLib.senselGetLEDBrightness(handle, led_id, ref brightness) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return brightness;
        }

        protected Exception SenselException()
        {
            string description = "Sensel Exception";
            return new Exception(description);
        }

        public SenselFirmwareInfo GetFirmwareInfo()
        {
            if (SenselLib.senselGetFirmwareInfo(handle, ref fw_info) != SenselStatus.SENSEL_OK)
                throw SenselException();
            return fw_info;
        }
    }
}
