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

        public static SenselSensorInfo SensorInfo {
            get {
                if (!CheckReady()) return default(SenselSensorInfo);
                return _thread.SensorInfo;
            }
        }

        public static NativeSlice<SenselContact> Contacts {
            get {
                if (!CheckReady()) return default(NativeSlice<SenselContact>);
                return _thread.Contacts;
            }
        }

        public static NativeArray<float> ForceArray {
            get {
                if (!CheckReady()) return default(NativeArray<float>);
                return _thread.ForceArray;
            }
        }

        public static bool Update()
        {
            if (!CheckReady()) return false;

            // Check if it has been already called in the current frame.
            var now = UnityEngine.Time.frameCount;
            if (now == _lastUpdate) return false;

            _thread.Update();
            _lastUpdate = now;

            return true;
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
                UnityEngine.Debug.LogWarning("No Sensel device is found.");
                _warned = true;
            }
            return false;
        }

        #endregion
    }
}
