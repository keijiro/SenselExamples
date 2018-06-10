using UnityEngine;
using Klak.Sensel;

public class RainController : MonoBehaviour
{
    Cortina.RainRenderer _renderer;

    Contact _contact1;
    Contact _contact2;

    void Start()
    {
        _renderer = GetComponent<Cortina.RainRenderer>();
    }

    void Update()
    {
        if (_contact1.IsValid)
            _contact1 = TouchInput.GetContact(_contact1.ID);
        else
            _contact1 = TouchInput.GetContactExclude(_contact2.ID);

        if (_contact2.IsValid)
            _contact2 = TouchInput.GetContact(_contact2.ID);
        else
            _contact2 = TouchInput.GetContactExclude(_contact1.ID);

        var input = Vector2.Lerp(
            new Vector2(_contact1.X, _contact1.Y),
            new Vector2(_contact2.X, _contact2.Y),
            _contact2.Force / (_contact1.Force + _contact2.Force + 0.00001f)
        );

        var tan = Mathf.Atan2(
            _contact2.Y - _contact1.Y, _contact2.X - _contact1.X
        );

        transform.rotation = Quaternion.Euler(new Vector3(
            90 - input.y * 180, input.x * 180 - 90, tan * Mathf.Rad2Deg
        ));

        var force = _contact1.Force + _contact2.Force;
        _renderer.Throttle = Mathf.Clamp01(force * 10);
        _renderer.LengthAmplitude = 1 + 2 * Mathf.Clamp01(force * 10 - 2);
    }
}
