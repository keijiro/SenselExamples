using Unity.Collections;
using Sensel;

namespace Klak.Sensel
{
    //
    // ContactHandler provides a storage for contact points and basic filtering
    // functionalities. This is only used from the Contact struct.
    //
    static class ContactHandler
    {
        #region Contact point variables

        public static NativeArray<Contact> _contactArray;
        public static int _contactCount;
        public static int _newContactCount;

        #endregion

        #region Query methods

        public static Contact Find(int id)
        {
            for (var i = 0; i < _contactCount; i++)
                if (_contactArray[i]._id == id) return _contactArray[i];
            return default(Contact);
        }

        public static Contact FindNot(int id)
        {
            for (var i = 0; i < _contactCount; i++)
                if (_contactArray[i]._id != id) return _contactArray[i];
            return default(Contact);
        }

        public static Contact FindNot(int id1, int id2)
        {
            for (var i = 0; i < _contactCount; i++)
            {
                var id = _contactArray[i]._id;
                if (id != id1 && id != id2) return _contactArray[i];
            }
            return default(Contact);
        }

        public static Contact FindNot(int id1, int id2, int id3)
        {
            for (var i = 0; i < _contactCount; i++)
            {
                var id = _contactArray[i]._id;
                if (id != id1 && id != id2 && id != id3) return _contactArray[i];
            }
            return default(Contact);
        }

        #endregion

        #region Update method

        public static void Update()
        {
            if (!SenselMaster.Update()) return;

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
        }

        #endregion

        #region Private members

        static SenselSensorInfo _sensorInfo;

        static ContactHandler()
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

        #endregion
    }
}
