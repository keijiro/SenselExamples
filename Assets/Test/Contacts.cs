using UnityEngine;
using Klak.Sensel;

public class Contacts : MonoBehaviour
{
    [SerializeField] Transform _transform1;
    [SerializeField] Transform _transform2;

    Contact _contact1;
    Contact _contact2;

    Vector2 ConvertPosition(Contact c)
    {
        var p = new Vector2(c.X, c.Y);
        return (p * 2 - Vector2.one) * new Vector2(1, 9.0f / 16);
    }

    void Update()
    {
        _contact1 = Contact.GetLatest(_contact1.ID);
        _contact2 = Contact.GetLatest(_contact2.ID);

        if (!_contact1.IsValid) _contact1 = Contact.GetAnother(_contact2.ID);
        if (!_contact2.IsValid) _contact2 = Contact.GetAnother(_contact1.ID);

        _transform1.localPosition = ConvertPosition(_contact1);
        _transform2.localPosition = ConvertPosition(_contact2);

        _transform1.localScale = Vector3.one * _contact1.Force;
        _transform2.localScale = Vector3.one * _contact2.Force;
    }
}
