using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Sensel;

namespace Klak.Sensel
{
    //
    // Sensel master class provides a singleton-like interface for inputs
    // from a Selsel device.
    //
    static class SenselMaster
    {
        #region Public properties and methods

        public static bool IsAvailable {
            get { return _thread != null; }
        }

        public static Vector2Int SensorResolution {
            get {
                if (IsAvailable)
                {
                    var info = _thread.SensorInfo;
                    return new Vector2Int(info.num_cols, info.num_rows);
                }
                else
                {
                    ShowNotConnectedWarning();
                    return new Vector2Int(8, 8);
                }
            }
        }

        public static void Update()
        {
            if (!IsAvailable)
            {
                ShowNotConnectedWarning();
                return;
            }

            // Check if it has been already called in the current frame.
            var now = Time.frameCount;
            if (now == _lastUpdate) return;

            _thread.Update();

            _lastUpdate = now;
        }

        unsafe public static void LoadForceIntoTexture(Texture2D texture)
        {
            if (!IsAvailable)
            {
                ShowNotConnectedWarning();
                return;
            }

            var frame = _thread.Frame;
            if (frame == null) return;

            var ptr = UnsafeUtility.AddressOf(ref frame.force_array[0]);
            var size = frame.force_array.Length * sizeof(float);
            texture.LoadRawTextureData((System.IntPtr)ptr, size);
        }

        #endregion

        #region Private members

        static SenselThread _thread;
        static int _lastUpdate;
        static bool _warned;

        static SenselMaster()
        {
            if (SenselDevice.GetDeviceList().num_devices > 0)
                _thread = new SenselThread();

        #if UNITY_EDITOR
            // To release the internal objects on script recompilation.
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseResources;
        #endif
        }

        #if UNITY_EDITOR

        static void ReleaseResources()
        {
            if (_thread != null)
            {
                _thread.Dispose();
                _thread = null;
            }
        }

        #endif

        static void ShowNotConnectedWarning()
        {
            if (!_warned)
            {
                Debug.LogWarning("Klak.Sensel: No Sensel device is found.");
                _warned = true;
            }
        }

        #endregion
    }
}
