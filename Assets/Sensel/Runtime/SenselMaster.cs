using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Sensel;

namespace Klak.Sensel
{
    static class SenselMaster
    {
        #region Public properties and methods

        public static bool IsAvailable {
            get { return SenselDevice.GetDeviceList().num_devices > 0; }
        }

        public static Vector2Int SensorResolution {
            get {
                var info = _thread.SensorInfo;
                return new Vector2Int(info.num_cols, info.num_rows);
            }
        }

        public static void Update()
        {
            if (_thread == null) return;
            var now = Time.frameCount;
            if (now == _lastUpdateFrame) return;
            _thread.Update();
            _lastUpdateFrame = now;
        }

        unsafe public static void LoadForceIntoTexture(Texture2D texture)
        {
            if (_thread == null) return;
            var frame = _thread.Frame;
            if (frame == null) return;
            var ptr = UnsafeUtility.AddressOf(ref frame.force_array[0]);
            var size = frame.force_array.Length * sizeof(float);
            texture.LoadRawTextureData((System.IntPtr)ptr, size);
        }

        #endregion

        #region Private members

        static SenselMaster()
        {
            if (IsAvailable) _thread = new SenselThread();

        #if UNITY_EDITOR
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseResources;
        #endif
        }

        static void ReleaseResources()
        {
            if (_thread != null)
            {
                _thread.Dispose();
                _thread = null;
            }
        }

        static SenselThread _thread;
        static int _lastUpdateFrame;

        #endregion
    }
}
