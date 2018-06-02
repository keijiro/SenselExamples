using UnityEngine;
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

        SenselDevice _device;
        SenselSensorInfo _sensorInfo;
        SenselFrame _frame;

        Thread _thread;
        AutoResetEvent _sync;
        bool _stop;

        #endregion

        #region Public properties

        public SenselSensorInfo SensorInfo { get { return _sensorInfo; } }
        public SenselFrame Frame { get { return _frame; } }

        #endregion

        #region Public methods

        public SenselThread()
        {
            var deviceList = SenselDevice.GetDeviceList();
            if (deviceList.num_devices == 0)
                throw new System.IO.IOException("Sensel device not found.");

            _device = new SenselDevice();
            _device.OpenDeviceByID(deviceList.devices[0].idx);
            _device.SetFrameContent(SenselDevice.FRAME_CONTENT_PRESSURE_MASK);

            _sensorInfo = _device.GetSensorInfo();

            _thread = new Thread(ReceiverThread);
            _sync = new AutoResetEvent(false);

            _thread.Start();
        }

        public void Update()
        {
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
                    _stop = true;
                    _sync.Set();
                    _thread.Join();
                    _thread = null;
                }
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
                _sync.WaitOne();
                if (_stop) break;
                _device.ReadSensor();
                _frame = _device.GetFrame();
            }

            _device.StopScanning();
        }

        #endregion
    }
}
