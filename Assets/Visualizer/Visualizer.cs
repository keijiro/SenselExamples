using UnityEngine;

namespace SenselTest
{
    public class Visualizer : MonoBehaviour
    {
        [SerializeField] bool _debug;

        [SerializeField, HideInInspector] Shader _shader;
        [SerializeField, HideInInspector] ComputeShader _updateCompute;

        ForceMap _force;
        Material _material;
        ComputeBuffer _state;
        RenderTexture _rt1;
        RenderTexture _rt2;

        void Start()
        {
            if (ForceMap.IsAvailable)
            {
                _force = new ForceMap();
                _force.StartScanning();
            }

            _material = new Material(_shader);
            _state = new ComputeBuffer(2, sizeof(float));
            _rt1 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);
            _rt2 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);
        }

        void OnDestroy()
        {
            if (_force != null) _force.Dispose();
            Destroy(_material);
            _state.Dispose();
            Destroy(_rt1);
            Destroy(_rt2);
        }

        void OnEnable()
        {
            if (_force != null) _force.StartScanning();
        }

        void OnDisable()
        {
            if (_force != null) _force.StopScanning();
        }

        void Update()
        {
            if (_force != null) _force.Update();

            _updateCompute.SetFloat("DeltaTime", Time.deltaTime);
            _updateCompute.SetTexture(0, "TotalTexture", _force.TotalInputTexture);
            _updateCompute.SetBuffer(0, "StateBuffer", _state);
            _updateCompute.Dispatch(0, 1, 1, 1);

            _material.SetTexture("_InputTex", _force.FilteredInputTexture);
            _material.SetBuffer("_StateBuffer", _state);
            Graphics.Blit(_rt1, _rt2, _material);

            var temp = _rt1;
            _rt1 = _rt2;
            _rt2 = temp;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
           Graphics.Blit(_debug ? _force.FilteredInputTexture : _rt2, destination);
        }
    }
}
