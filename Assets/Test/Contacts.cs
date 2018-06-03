using UnityEngine;
using Klak.Sensel;

public class Contacts : MonoBehaviour
{
    #region Editable property

    [SerializeField] GameObject _indicatorTemplate;

    #endregion

    #region Contacts and indicators

    const int kMaxContacts = 16;
    Contact [] _contacts = new Contact[kMaxContacts];
    GameObject [] _indicators = new GameObject[kMaxContacts];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _indicators[0] = _indicatorTemplate;
        for (var i = 1; i < kMaxContacts; i++)
            _indicators[i] = Instantiate(_indicatorTemplate);
    }

    void Update()
    {
        // Update the existing contacts.
        for (var i = 0; i < kMaxContacts; i++)
        {
            // If the contact is alive, try retrieving the latest state.
            if (_contacts[i].IsValid)
                _contacts[i] = Contact.GetLatest(_contacts[i].ID);

            UpdateIndicator(_indicators[i], _contacts[i]);
            SwitchParticle(_indicators[i], _contacts[i].IsValid);
        }

        // Add new entries to the contact array.
        var newEntries = Contact.NewEntries;
        for (var i1 = 0; i1 < newEntries.Length; i1++)
        {
            // Find an unsed contact.
            for (var i2 = 0; i2 < _contacts.Length; i2++)
            {
                if (!_contacts[i2].IsValid)
                {
                    // Start using this one. Don't enable the particle system
                    // at this point to avoid particle emission by jump.
                    _contacts[i2] = newEntries[i1];
                    UpdateIndicator(_indicators[i2], _contacts[i2]);
                    break;
                }
            }
        }
    }

    #endregion

    #region Private methods

    static void UpdateIndicator(GameObject indicator, Contact contact)
    {
        var transform = indicator.transform;

        if (contact.IsValid)
        {
            var pos = new Vector2(contact.X, contact.Y);
            transform.position = (pos * 2 - Vector2.one) * new Vector2(1, 9.0f / 16);

            var color = Color.HSVToRGB((contact.ID * 0.1f) % 1.0f, 1, 1);
            indicator.GetComponent<Renderer>().material.color = color;
            indicator.GetComponentInChildren<ParticleSystemRenderer>().material.color = color;
        }

        transform.localScale = Vector3.one * (contact.Force * 20);
    }

    static void SwitchParticle(GameObject indicator, bool enable)
    {
        var emission = indicator.GetComponentInChildren<ParticleSystem>().emission;
        emission.enabled = enable;
    }

    #endregion
}
