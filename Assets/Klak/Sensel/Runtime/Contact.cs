using Unity.Collections;

namespace Klak.Sensel
{
    //
    // Contact stores information about a single contact point. It also
    // provides static properties and methods to retrieve contact points on a
    // Sensel device.
    //
    public struct Contact
    {
        #region Public properties

        public bool IsValid { get { return _id != 0; } }
        public int ID { get { return _id; } }
        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Force { get { return _force; } }

        #endregion

        #region Private members

        internal int _id;
        internal float _x, _y, _force;

        #endregion

        #region Public static interface

        // Get a contact point that has the same ID.
        public static Contact GetLatest(int id)
        {
            if (id == 0) return default(Contact);
            ContactHandler.Update();
            return ContactHandler.FindExcludeNewEntries(id);
        }

        // Get a contact point that has a different ID from the given ID(s).
        public static Contact GetAnother(int id)
        {
            ContactHandler.Update();
            return ContactHandler.FindNot(id);
        }

        public static Contact GetAnother(int id1, int id2)
        {
            ContactHandler.Update();
            return ContactHandler.FindNot(id1, id2);
        }

        public static Contact GetAnother(int id1, int id2, int id3)
        {
            ContactHandler.Update();
            return ContactHandler.FindNot(id1, id2, id3);
        }

        // Get a contact point without specifying an ID.
        public static Contact Head {
            get {
                ContactHandler.Update();
                if (ContactHandler._contactCount == 0) return default(Contact);
                return ContactHandler._contactArray[0];
            }
        }

        // Get the array of the currently active contact points.
        public static NativeSlice<Contact> All {
            get {
                ContactHandler.Update();
                return new NativeSlice<Contact>(
                    ContactHandler._contactArray,
                    0, ContactHandler._contactCount
                );
            }
        }

        // Get the array of contact points that are newly entered.
        public static NativeSlice<Contact> NewEntries {
            get {
                ContactHandler.Update();
                return new NativeSlice<Contact>(
                    ContactHandler._contactArray,
                    0, ContactHandler._newContactCount
                );
            }
        }

        #endregion
    }
}
