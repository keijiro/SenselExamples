using UnityEngine;

public class Visualizer : MonoBehaviour
{
    [SerializeField] bool _debug;

    [SerializeField, HideInInspector] Shader _shader;
    [SerializeField, HideInInspector] ComputeShader _updateCompute;

    Klak.Sensel.ForceMap _forceMap;
    Material _material;
    ComputeBuffer _state;
    RenderTexture _rt1;
    RenderTexture _rt2;

    void Start()
    {
        _forceMap = new Klak.Sensel.ForceMap();
        _material = new Material(_shader);
        _state = new ComputeBuffer(2, sizeof(float));
        _rt1 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);
        _rt2 = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGBHalf);
    }

    void OnDestroy()
    {
        if (_forceMap != null) _forceMap.Dispose();
        Destroy(_material);
        _state.Dispose();
        Destroy(_rt1);
        Destroy(_rt2);
    }

    void Update()
    {
        if (_forceMap != null) _forceMap.Update();

        _updateCompute.SetFloat("DeltaTime", Time.deltaTime);
        _updateCompute.SetTexture(0, "TotalTexture", _forceMap.TotalInputTexture);
        _updateCompute.SetBuffer(0, "StateBuffer", _state);
        _updateCompute.Dispatch(0, 1, 1, 1);

        _material.SetTexture("_InputTex", _forceMap.FilteredInputTexture);
        _material.SetBuffer("_StateBuffer", _state);
        Graphics.Blit(_rt1, _rt2, _material);

        var temp = _rt1;
        _rt1 = _rt2;
        _rt2 = temp;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
       Graphics.Blit(_debug ? _forceMap.FilteredInputTexture : _rt2, destination);
    }
}
