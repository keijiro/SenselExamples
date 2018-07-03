using UnityEngine;
using Klak.Sensel;

public class Fluid : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] int _resolution = 512;
    [SerializeField] float _viscosity = 0.01f;
    [SerializeField] float _force = 300;
    [SerializeField] float _exponent = 200;

    #endregion

    #region Internal resources

    [SerializeField, HideInInspector] ComputeShader _compute;
    [SerializeField, HideInInspector] Shader _shader;

    #endregion

    #region Buffer objects

    Material _shaderSheet;

    static class Kernels
    {
        public const int Advect = 0;
        public const int Force = 1;
        public const int PSetup = 2;
        public const int PFinish = 3;
        public const int Jacobi1 = 4;
        public const int Jacobi2 = 5;
    }

    int ThreadCountX { get { return (_resolution                                + 7) / 8; } }
    int ThreadCountY { get { return (_resolution * Screen.height / Screen.width + 7) / 8; } }

    int ResolutionX { get { return ThreadCountX * 8; } }
    int ResolutionY { get { return ThreadCountY * 8; } }

    // Vector field buffers
    static class VFB
    {
        public static RenderTexture V1;
        public static RenderTexture V2;
        public static RenderTexture V3;
        public static RenderTexture P1;
        public static RenderTexture P2;
    }

    // Color buffers (for double buffering)
    RenderTexture _colorRT1;
    RenderTexture _colorRT2;

    RenderTexture AllocateBuffer(int componentCount, int width = 0, int height = 0)
    {
        var format = RenderTextureFormat.ARGBHalf;
        if (componentCount == 1) format = RenderTextureFormat.RHalf;
        if (componentCount == 2) format = RenderTextureFormat.RGHalf;

        if (width  == 0) width  = ResolutionX;
        if (height == 0) height = ResolutionY;

        var rt = new RenderTexture(width, height, 0, format);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    #endregion

    #region Contact points

    const int kMaxContacts = 16;

    Contact [] _contacts = new Contact [kMaxContacts];
    Vector4 [] _forceOrigins = new Vector4 [kMaxContacts];
    Vector4 [] _forceVectors = new Vector4 [kMaxContacts];

    Vector2 ConvertCoord(Contact c)
    {
        return new Vector2((c.X - 0.5f) * 16 / 9, c.Y - 0.5f);
    }

    Vector4 MakeForceOrigin(Contact c)
    {
        var crd = ConvertCoord(c);
        return new Vector3(crd.x, crd.y, c.Force);
    }

    Vector4 MakeForceVector(Contact c0, Contact c1)
    {
        return ConvertCoord(c1) - ConvertCoord(c0);
    }

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
    {
        _resolution = Mathf.Max(_resolution, 8);
    }

    void Start()
    {
        _shaderSheet = new Material(_shader);

        VFB.V1 = AllocateBuffer(2);
        VFB.V2 = AllocateBuffer(2);
        VFB.V3 = AllocateBuffer(2);
        VFB.P1 = AllocateBuffer(1);
        VFB.P2 = AllocateBuffer(1);

        _colorRT1 = AllocateBuffer(1, 1920, 1080);
        _colorRT2 = AllocateBuffer(1, 1920, 1080);
    }

    void OnDestroy()
    {
        Destroy(_shaderSheet);

        Destroy(VFB.V1);
        Destroy(VFB.V2);
        Destroy(VFB.V3);
        Destroy(VFB.P1);
        Destroy(VFB.P2);

        Destroy(_colorRT1);
        Destroy(_colorRT2);
    }

    void Update()
    {
        var dt = Time.deltaTime;
        var dx = 1.0f / ResolutionY;

        // Update contact points.
        for (var i = 0; i < _contacts.Length; i++)
        {
            var updated = TouchInput.GetContact(_contacts[i].ID);
            if (_contacts[i].IsValid && updated.IsValid)
            {
                _forceOrigins[i] = MakeForceOrigin(updated);
                _forceVectors[i] = MakeForceVector(_contacts[i], updated);
            }
            else
            {
                _forceOrigins[i] = new Vector4(1e+5f, 0, 0, 0);
                _forceVectors[i] = Vector3.zero;
            }
            _contacts[i] = updated;
        }

        // Append newly entered contact points.
        foreach (var newContact in TouchInput.NewContacts)
        {
            for (var i = 0; i < _contacts.Length; i++)
            {
                if (!_contacts[i].IsValid)
                {
                    _contacts[i] = newContact;
                    break;
                }
            }
        }

        // Common variables
        _compute.SetFloat("Time", Time.time);
        _compute.SetFloat("DeltaTime", dt);

        // Advection
        _compute.SetTexture(Kernels.Advect, "U_in", VFB.V1);
        _compute.SetTexture(Kernels.Advect, "W_out", VFB.V2);
        _compute.Dispatch(Kernels.Advect, ThreadCountX, ThreadCountY, 1);

        // Diffuse setup
        var dif_alpha = dx * dx / (_viscosity * dt);
        _compute.SetFloat("Alpha", dif_alpha);
        _compute.SetFloat("Beta", 4 + dif_alpha);
        Graphics.CopyTexture(VFB.V2, VFB.V1);
        _compute.SetTexture(Kernels.Jacobi2, "B2_in", VFB.V1);

        // Jacobi iteration
        for (var i = 0; i < 20; i++)
        {
            _compute.SetTexture(Kernels.Jacobi2, "X2_in", VFB.V2);
            _compute.SetTexture(Kernels.Jacobi2, "X2_out", VFB.V3);
            _compute.Dispatch(Kernels.Jacobi2, ThreadCountX, ThreadCountY, 1);

            _compute.SetTexture(Kernels.Jacobi2, "X2_in", VFB.V3);
            _compute.SetTexture(Kernels.Jacobi2, "X2_out", VFB.V2);
            _compute.Dispatch(Kernels.Jacobi2, ThreadCountX, ThreadCountY, 1);
        }

        // Add external force
        _compute.SetVectorArray("ForceOrigins", _forceOrigins);
        _compute.SetVectorArray("ForceVectors", _forceVectors);
        _compute.SetTexture(Kernels.Force, "W_in", VFB.V2);
        _compute.SetTexture(Kernels.Force, "W_out", VFB.V3);
        _compute.Dispatch(Kernels.Force, ThreadCountX, ThreadCountY, 1);

        // Projection setup
        _compute.SetTexture(Kernels.PSetup, "W_in", VFB.V3);
        _compute.SetTexture(Kernels.PSetup, "DivW_out", VFB.V2);
        _compute.SetTexture(Kernels.PSetup, "P_out", VFB.P1);
        _compute.Dispatch(Kernels.PSetup, ThreadCountX, ThreadCountY, 1);

        // Jacobi iteration
        _compute.SetFloat("Alpha", -dx * dx);
        _compute.SetFloat("Beta", 4);
        _compute.SetTexture(Kernels.Jacobi1, "B1_in", VFB.V2);

        for (var i = 0; i < 20; i++)
        {
            _compute.SetTexture(Kernels.Jacobi1, "X1_in", VFB.P1);
            _compute.SetTexture(Kernels.Jacobi1, "X1_out", VFB.P2);
            _compute.Dispatch(Kernels.Jacobi1, ThreadCountX, ThreadCountY, 1);

            _compute.SetTexture(Kernels.Jacobi1, "X1_in", VFB.P2);
            _compute.SetTexture(Kernels.Jacobi1, "X1_out", VFB.P1);
            _compute.Dispatch(Kernels.Jacobi1, ThreadCountX, ThreadCountY, 1);
        }

        // Projection finish
        _compute.SetTexture(Kernels.PFinish, "W_in", VFB.V3);
        _compute.SetTexture(Kernels.PFinish, "P_in", VFB.P1);
        _compute.SetTexture(Kernels.PFinish, "U_out", VFB.V1);
        _compute.Dispatch(Kernels.PFinish, ThreadCountX, ThreadCountY, 1);

        // Apply the velocity field to the color buffer.
        _shaderSheet.SetVectorArray("_ForceOrigins", _forceOrigins);
        _shaderSheet.SetVectorArray("_ForceVectors", _forceVectors);
        _shaderSheet.SetTexture("_VelocityField", VFB.V1);
        Graphics.Blit(_colorRT1, _colorRT2, _shaderSheet, 0);

        // Swap the color buffers.
        var temp = _colorRT1;
        _colorRT1 = _colorRT2;
        _colorRT2 = temp;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(_colorRT1, destination, _shaderSheet, 1);
    }

    #endregion
}
