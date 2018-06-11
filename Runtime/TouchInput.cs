using Unity.Collections;
using Sensel;

namespace Klak.Sensel
{
    //
    // Contact stores information about a single contact point.
    //
    public struct Contact
    {
        public bool IsValid { get { return _id != 0; } }
        public int ID { get { return _id; } }
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Force { get { return _force; } }

        internal int _id;
        internal float _x, _y, _force;
    }

    //
    // TouchInput is a singleton-like interface that provides information about
    // contact points on a sensor device.
    //
    static public class TouchInput
    {
        #region Contact point retrival properties

        // Get a contact point without specifying an ID.
        public static Contact Contact {
            get {
                Update();
                if (_contactCount == 0) return default(Contact);
                return _contactArray[0];
            }
        }

        // Get the array of the currently active contact points.
        public static NativeSlice<Contact> AllContacts {
            get {
                Update();
                return new NativeSlice<Contact>(_contactArray, 0, _contactCount);
            }
        }

        // Get the array of contact points that are newly activated.
        public static NativeSlice<Contact> NewContacts {
            get {
                Update();
                return new NativeSlice<Contact>(_contactArray, 0, _newContactCount);
            }
        }

        #endregion

        #region Contact point query methods

        // Get a contact point with specifying an ID.
        public static Contact GetContact(int id)
        {
            if (id == 0) return default(Contact);
            Update();
            for (var i = _newContactCount; i < _contactCount; i++)
                if (_contactArray[i]._id == id) return _contactArray[i];
            return default(Contact);
        }

        // Get a contact point that has a different ID from the given ID(s).
        public static Contact GetContactExclude(int id)
        {
            Update();
            for (var i = 0; i < _contactCount; i++)
                if (_contactArray[i]._id != id) return _contactArray[i];
            return default(Contact);
        }

        public static Contact GetContactExclude(int id1, int id2)
        {
            Update();
            for (var i = 0; i < _contactCount; i++)
            {
                var id = _contactArray[i]._id;
                if (id != id1 && id != id2) return _contactArray[i];
            }
            return default(Contact);
        }

        public static Contact GetContactExclude(int id1, int id2, int id3)
        {
            Update();
            for (var i = 0; i < _contactCount; i++)
            {
                var id = _contactArray[i]._id;
                if (id != id1 && id != id2 && id != id3) return _contactArray[i];
            }
            return default(Contact);
        }

        #endregion

        #region Private members

        static SenselSensorInfo _sensorInfo;
        static NativeArray<Contact> _contactArray;
        static int _contactCount;
        static int _newContactCount;
        static int _lastUpdate;

        static TouchInput()
        {
            _sensorInfo = SenselMaster.SensorInfo;

            _contactArray = new NativeArray<Contact>(
                _sensorInfo.max_contacts, Allocator.Persistent
            );

        #if UNITY_EDITOR
            // To release the internal objects on script recompilation.
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ReleaseResources;
        #endif
        }

        static void ReleaseResources()
        {
            _contactArray.Dispose();
        }

        static Contact ConvertContact(ref NativeSlice<SenselContact> contacts, int index)
        {
            var source = contacts[index];
            return new Contact {
                // We prefer one-based numbering, so let's add one.
                _id = source.id + 1,
                // Normalized position
                _x = source.x_pos / _sensorInfo.width,
                _y = 1 - source.y_pos / _sensorInfo.height,
                // 8192 = The maximum measurable force with the sensor
                _force = source.total_force / 8192
            };
        }

        static void Update()
        {
            SenselMaster.Update();

            // Check if it has been already called in the current frame.
            var now = UnityEngine.Time.frameCount;
            if (now == _lastUpdate) return;

            var inputs = SenselMaster.Contacts;

            // Pick the new contacts up.
            _newContactCount = 0;
            for (var i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].state == (int)SenselContactState.CONTACT_START)
                    _contactArray[_newContactCount++] = ConvertContact(ref inputs, i);
            }

            // Collect the rest of the contacts.
            _contactCount = _newContactCount;
            for (var i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].state != (int)SenselContactState.CONTACT_START)
                    _contactArray[_contactCount++] = ConvertContact(ref inputs, i);
            }

            _lastUpdate = now;
        }

        #endregion
    }
}
