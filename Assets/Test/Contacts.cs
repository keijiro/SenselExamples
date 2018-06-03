using UnityEngine;
using Klak.Sensel;

public class Contacts : MonoBehaviour
{
    enum Mode { Single, Dual, Triple, Sixteen }

    [SerializeField] Mode _mode = Mode.Sixteen;
    [SerializeField] GameObject _indicator;

    GameObject [] _indicators;

    Contact _contact1;
    Contact _contact2;
    Contact _contact3;

    static void UpdateIndicator(GameObject indicator, Contact contact)
    {
        var transform = indicator.transform;
        var pos = new Vector2(contact.X, contact.Y);
        transform.position = (pos * 2 - Vector2.one) * new Vector2(1, 9.0f / 16);
        transform.localScale = Vector3.one * (contact.Force * 2);

        var color = Color.HSVToRGB((contact.ID * 0.1f) % 1.0f, 1, 1);
        indicator.GetComponent<Renderer>().material.color = color;
    }

    void Start()
    {
        _indicators = new GameObject[16];
        _indicators[0] = _indicator;
        for (var i = 1; i < _indicators.Length; i++)
            _indicators[i] = Instantiate(_indicator);
    }

    void Update()
    {
        if (_mode == Mode.Sixteen)
        {
            // Sixteen mode: Show all the contacts.
            var contacts = Contact.All;

            for (var i = 0; i < contacts.Length; i++)
            {
                _indicators[i].SetActive(true);
                UpdateIndicator(_indicators[i], contacts[i]);
            }

            for (var i = contacts.Length; i < _indicators.Length; i++)
                _indicators[i].SetActive(false);
        }
        else
        {
            if (_mode == Mode.Single)
            {
                // Single mode: Trace only a single contact.
                _contact1 = Contact.GetLatest(_contact1.ID);

                if (!_contact1.IsValid) _contact1 = Contact.Head;

                _contact2 = default(Contact);
                _contact3 = default(Contact);
            }
            else if (_mode == Mode.Dual)
            {
                // Dual mode: Trace two contacts.
                _contact1 = Contact.GetLatest(_contact1.ID);
                _contact2 = Contact.GetLatest(_contact2.ID);

                if (!_contact1.IsValid) _contact1 = Contact.GetAnother(_contact2.ID);
                if (!_contact2.IsValid) _contact2 = Contact.GetAnother(_contact1.ID);

                _contact3 = default(Contact);
            }
            else // _mode == Mode.Triple
            {
                // Triple mode: Trace three contacts.
                _contact1 = Contact.GetLatest(_contact1.ID);
                _contact2 = Contact.GetLatest(_contact2.ID);
                _contact3 = Contact.GetLatest(_contact3.ID);

                if (!_contact1.IsValid) _contact1 = Contact.GetAnother(_contact2.ID, _contact3.ID);
                if (!_contact2.IsValid) _contact2 = Contact.GetAnother(_contact1.ID, _contact3.ID);
                if (!_contact3.IsValid) _contact3 = Contact.GetAnother(_contact1.ID, _contact2.ID);
            }

            if (_contact1.IsValid) UpdateIndicator(_indicators[0], _contact1);
            if (_contact2.IsValid) UpdateIndicator(_indicators[1], _contact2);
            if (_contact3.IsValid) UpdateIndicator(_indicators[2], _contact3);

            _indicators[0].SetActive(_contact1.IsValid);
            _indicators[1].SetActive(_contact2.IsValid);
            _indicators[2].SetActive(_contact3.IsValid);

            for (var i = 3; i < _indicators.Length; i++)
                _indicators[i].SetActive(false);
        }
    }
}
