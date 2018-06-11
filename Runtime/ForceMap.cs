using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;

namespace Klak.Sensel
{
    //
    // ForceMap visualizes force inputs on a Sensel device surface. It applies
    // a low pass filter to give expressiveness to visual inputs.
    //
    public sealed class ForceMap : System.IDisposable
    {
        #region Compile time constants

        // Force map dimensions -- hardcoded values here!
        // These numbers are the nearest PoT values from the resolution of the
        // Morph force sensor matrix (185x105).
        const int kMapWidth = 128;
        const int kMapHeight = 72;

        // Count of image pyramid levels
        const int kLevelCount = 5;

        #endregion

        #region Private variables

        Material _filter;
        Texture2D _rawInput;
        RenderTexture _filteredInput;
        RenderTexture _totalInput;
        RenderTexture[] _pyramid = new RenderTexture[kLevelCount];

        #endregion

        #region Public properties

        public Texture RawInputTexture {
            get { return _rawInput; }
        }

        public Texture FilteredInputTexture {
            get { return _filteredInput; }
        }

        public Texture TotalInputTexture {
            get { return _totalInput; }
        }

        #endregion

        #region Public methods

        public ForceMap()
        {
            _filter = new Material(Shader.Find("Hidden/Sensel/Filters"));

            // Raw input texture
            var info = SenselMaster.SensorInfo;
            _rawInput = new Texture2D(info.num_cols, info.num_rows, TextureFormat.RFloat, false);
            _rawInput.wrapMode = TextureWrapMode.Clamp;

            // Filtered input texture
            _filteredInput = new RenderTexture(kMapWidth, kMapHeight, 0, RenderTextureFormat.RHalf);
            _filteredInput.wrapMode = TextureWrapMode.Clamp;

            // Image pyramid
            var tw = kMapWidth;
            var th = kMapHeight;

            for (var i = 0; i < kLevelCount; i++)
            {
                tw /= 2;
                th /= 2;
                _pyramid[i] = new RenderTexture(tw, th, 0, RenderTextureFormat.RHalf);
                _pyramid[i].wrapMode = TextureWrapMode.Clamp;
            }

            // Single pixel texture used to store the total of input force
            _totalInput = new RenderTexture(1, 1, 0, RenderTextureFormat.RHalf);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Update()
        {
            SenselMaster.Update();

            // Transfer the force array to the raw input texture.
            unsafe {
                var input = SenselMaster.ForceArray;
                if (input.IsCreated)
                {
                    _rawInput.LoadRawTextureData(
                        (System.IntPtr)input.GetUnsafePtr(),
                        sizeof(float) * input.Length
                    );
                    _rawInput.Apply();
                }
            }

            // Apply the prefilter (vertical flip).
            Graphics.Blit(_rawInput, _filteredInput, _filter, 0);

            // Apply the gaussian blur filter.
            ApplyBlurFilter(_filteredInput);
        }

        #endregion

        #region Private methods

        ~ForceMap()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_rawInput != null)
                    UnityEngine.Object.Destroy(_rawInput);

                if (_filteredInput != null)
                    UnityEngine.Object.Destroy(_filteredInput);

                if (_filter != null)
                    UnityEngine.Object.Destroy(_filter);

                if (_totalInput != null)
                    UnityEngine.Object.Destroy(_totalInput);

                for (var i = 0; i < kLevelCount; i++)
                    if (_pyramid[i] != null) UnityEngine.Object.Destroy(_pyramid[i]);
            }
        }

        void ApplyBlurFilter(RenderTexture source)
        {
            // Downsampling chain
            Graphics.Blit(source, _pyramid[0], _filter, 1);
            for (var i = 0; i < kLevelCount - 1; i++)
                Graphics.Blit(_pyramid[i], _pyramid[i + 1], _filter, 1);

            // Upsampling chain
            _filter.SetFloat("_Alpha", 1);
            for (var i = kLevelCount - 1; i > 0; i--)
                Graphics.Blit(_pyramid[i], _pyramid[i - 1], _filter, 2);

            // Final upsampling and normalization
            _filter.SetFloat("_Alpha", 1.0f / (1 + kLevelCount));
            Graphics.Blit(_pyramid[0], source, _filter, 2);

            // Calculate the total force from the lowest layer.
            Graphics.Blit(_pyramid[kLevelCount - 1], _totalInput, _filter, 3);
        }

        #endregion
    }
}
