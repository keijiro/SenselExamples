using UnityEngine;
using Sensel;

public class Test : MonoBehaviour
{
    SenselDevice _device;
    SenselSensorInfo _sensorInfo;
    Texture2D _texture;
    Color32 [] _pixels;

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
        if (_texture == null)
        {
            _texture = new Texture2D(_sensorInfo.num_cols, _sensorInfo.num_rows);
            GetComponent<Renderer>().material.mainTexture = _texture;

            _pixels = new Color32 [_sensorInfo.num_cols * _sensorInfo.num_rows];
        }

        _device.ReadSensor();

        var count = _device.GetNumAvailableFrames();
        if (count == 0) return;

        for (var i = 0; i < count - 1; i++) _device.GetFrame();
        var frame = _device.GetFrame();

        var offs = 0;
        for (var y = 0; y < _sensorInfo.num_rows; y++)
        {
            for (var x = 0; x < _sensorInfo.num_cols; x++)
            {
                var c_in = (float)_pixels[offs].r;
                c_in += frame.force_array[offs] * 2 - Time.deltaTime * 200;
                var c = (System.Byte)Mathf.Clamp(c_in, 0, 255);
                _pixels[offs++] = new Color32(c, c, c, 0xff);
            }
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply();
    }

    void OnDisable()
    {
        if (_device != null) _device.StopScanning();
    }

    void OnDestroy()
    {
        if (_device != null)
        {
            _device.Close();
            _device = null;
        }

        if (_texture != null) Destroy(_texture);
    }
}
