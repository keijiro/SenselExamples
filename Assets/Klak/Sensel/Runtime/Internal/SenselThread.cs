using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;
using Sensel;

namespace Klak.Sensel
{
    //
    // Sensel thread class manages the lifecycle of the device object and the
    // receiver thread.
    //
    sealed internal class SenselThread : System.IDisposable
    {
        #region Private variables

        // Device information
        SenselDevice _device;
        SenselSensorInfo _sensorInfo;

        // Received frame data
        SenselFrame _frame;
        NativeArray<float> _forceArray;

        // Thread control
        Thread _thread;
        AutoResetEvent _sync;
        bool _stop;

        #endregion

        #region Public properties

        public SenselSensorInfo SensorInfo { get { return _sensorInfo; } }
        public NativeArray<float> ForceArray { get { return _forceArray; } }

        #endregion

        #region Public methods

        public SenselThread()
        {
            // Check if a device is available.
            var deviceList = SenselDevice.GetDeviceList();
            if (deviceList.num_devices == 0)
                throw new System.IO.IOException("Sensel device not found.");

            // Open the found device.
            _device = new SenselDevice();
            _device.OpenDeviceByID(deviceList.devices[0].idx);
            _device.SetFrameContent(SenselDevice.FRAME_CONTENT_PRESSURE_MASK);

            _sensorInfo = _device.GetSensorInfo();

            // Allocate the force array.
            _forceArray = new NativeArray<float>(
                _sensorInfo.num_cols * _sensorInfo.num_rows,
                Allocator.Persistent
            );

            // Start the receiver thread.
            _thread = new Thread(ReceiverThread);
            _sync = new AutoResetEvent(false);
            _thread.Start();
        }

        public void Update()
        {
            if (_frame != null)
            {
                // Copy the force array from the received frame. We expect that
                // the previous read operation in the receiver thread has been
                // already done so that we can safely copy the array. Although
                // it may introduce visual tearing if the operation is not
                // completed, it might be not a serious problem... or is it?
                unsafe {
                    UnsafeUtility.MemCpy(
                        _forceArray.GetUnsafePtr(),
                        UnsafeUtility.AddressOf(ref _frame.force_array[0]),
                        sizeof(float) * _forceArray.Length
                    );
                }
            }

            // Wake the receiver thread up.
            _sync.Set();
        }

        #endregion

        #region IDisposable implementation

        ~SenselThread()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_thread != null)
                {
                    // Stop and join the receiver thread.
                    _stop = true;
                    _sync.Set();
                    _thread.Join();
                    _thread = null;
                }

                if (_forceArray.IsCreated) _forceArray.Dispose();
            }

            if (_device != null)
            {
                _device.Close();
                _device = null;
            }
        }

        #endregion

        #region Receiver thread function

        void ReceiverThread()
        {
            _device.StartScanning();

            while (true)
            {
                // Wait for a kick from the main thread.
                _sync.WaitOne();

                if (_stop) break;

                // Read the sensor.
                // This may block the thread for serial communication.
                _device.ReadSensor();

                // Retrieve the received frame data.
                _frame = _device.GetFrame();
            }

            _device.StopScanning();
        }

        #endregion
    }
}
