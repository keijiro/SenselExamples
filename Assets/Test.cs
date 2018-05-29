using UnityEngine;
using Sensel;

public class Test : MonoBehaviour
{
    SenselDevice _device;
    SenselSensorInfo _sensorInfo;
    ComputeBuffer _buffer;
    MaterialPropertyBlock _materialSheet;

    void OnEnable()
    {
        if (_device == null)
        {
            var deviceList = SenselDevice.GetDeviceList();
            if (deviceList.num_devices == 0) return;

            _device = new SenselDevice();
            _device.OpenDeviceByID(deviceList.devices[0].idx);
        }

        _device.SetFrameContent(SenselDevice.FRAME_CONTENT_PRESSURE_MASK);
        _device.StartScanning();

        _sensorInfo = _device.GetSensorInfo();
    }

    void Update()
    {
        if (_device == null) return; // No device is available.

        if (_buffer == null)
        {
            _buffer = new ComputeBuffer(
                _sensorInfo.num_cols * _sensorInfo.num_rows, sizeof(float)
            );

            if (_materialSheet == null) _materialSheet = new MaterialPropertyBlock();

            var renderer = GetComponent<Renderer>();
            renderer.GetPropertyBlock(_materialSheet);
            _materialSheet.SetBuffer("_ForceBuffer", _buffer);
            _materialSheet.SetVector("_BufferDims",
                new Vector2(_sensorInfo.num_cols, _sensorInfo.num_rows));
            renderer.SetPropertyBlock(_materialSheet);
        }

        _device.ReadSensor();

        var count = _device.GetNumAvailableFrames();
        if (count == 0) return;

        for (var i = 0; i < count - 1; i++) _device.GetFrame();
        var frame = _device.GetFrame();

        _buffer.SetData(frame.force_array);
    }

    void OnDisable()
    {
        if (_device != null) _device.StopScanning();
        if (_buffer != null) _buffer.Dispose();
    }

    void OnDestroy()
    {
        if (_device != null)
        {
            _device.Close();
            _device = null;
        }
    }
}
