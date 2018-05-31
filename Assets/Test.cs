using UnityEngine;
using Sensel;

public class Test : MonoBehaviour
{
    [SerializeField] Shader _shader;

    ComputeBuffer _buffer;
    RenderTexture _texture1;
    RenderTexture _texture2;
    Material _material;

    SenselDevice _device;
    SenselSensorInfo _sensorInfo;

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
            _buffer = new ComputeBuffer(_sensorInfo.num_cols * _sensorInfo.num_rows, sizeof(float));

        if (_texture1 == null)
            _texture1 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);

        if (_texture2 == null)
            _texture2 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);

        if (_material == null)
            _material = new Material(_shader);

        _device.ReadSensor();

        var frame = _device.GetFrame();

        _buffer.SetData(frame.force_array);
        _material.SetBuffer("_ForceBuffer", _buffer);
        _material.SetVector("_BufferDims", new Vector2(_sensorInfo.num_cols, _sensorInfo.num_rows));
        Graphics.Blit(_texture1, _texture2, _material);

        var temp = _texture1;
        _texture1 = _texture2;
        _texture2 = temp;
    }

    void OnDisable()
    {
        if (_device != null) _device.StopScanning();

        if (_buffer != null)
        {
            _buffer.Dispose();
            _buffer = null;
        }
    }

    void OnDestroy()
    {
        if (_device != null)
        {
            _device.Close();
            _device = null;
        }

        if (_texture1 != null) Destroy(_texture1);
        if (_texture2 != null) Destroy(_texture2);
        if (_material != null) Destroy(_material);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_texture1, destination);
    }
}
