using UnityEngine;
using Unity.Collections;
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
            get {
                // Don't use CheckReady. This should be done without warning.
                return _thread != null;
            }
        }

        public static Vector2Int SensorResolution {
            get {
                if (!CheckReady()) return Vector2Int.zero;
                var info = _thread.SensorInfo;
                return new Vector2Int(info.num_cols, info.num_rows);
            }
        }

        public static NativeArray<float> ForceArray {
            get {
                if (!CheckReady()) return default(NativeArray<float>);
                return _thread.ForceArray;
            }
        }

        public static void Update()
        {
            if (!CheckReady()) return;

            // Check if it has been already called in the current frame.
            var now = Time.frameCount;
            if (now == _lastUpdate) return;

            _thread.Update();
            _lastUpdate = now;
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

        static bool CheckReady()
        {
            if (IsAvailable) return true;

            if (!_warned)
            {
                Debug.LogWarning("Klak.Sensel: No Sensel device is found.");
                _warned = true;
            }

            return false;
        }

        #endregion
    }
}
