using UnityEngine;
using Klak.Sensel;

public class Theremin : MonoBehaviour
{
    class Oscillator
    {
        public Contact Contact { get; set; }

        public float Frequency {
            set { _targetDelta = value / 48000; }
        }

        public float Amplitude { get; set; }
        public float Modulation { get; set; }

        float _targetDelta;
        float _delta;
        float _phase;

        public float Tick()
        {
            _delta = Mathf.Lerp(_delta, _targetDelta, 0.05f);
            _phase = (_phase + _delta) % 1;
            var p = Mathf.PI * 2 * _phase;
            return Mathf.Sin(p + Modulation * Mathf.Sin(p * 7)) * Amplitude;
        }
    }

    Oscillator[] _oscillators;
    AudioClip _clip;

    void Start()
    {
        _oscillators = new Oscillator[16];

        for (var i = 0; i < _oscillators.Length; i++)
            _oscillators[i] = new Oscillator();

        _clip = AudioClip.Create("Test", 0x7fffffff, 1, 48000, true, OnPcmRead);

        var source = GetComponent<AudioSource>();
        source.clip = _clip;
        source.Play();
    }

    void Update()
    {
        foreach (var osc in _oscillators)
            osc.Contact = TouchInput.GetContact(osc.Contact.ID);

        foreach (var newContact in TouchInput.NewContacts)
        {
            for (var i = 0; i < _oscillators.Length; i++)
            {
                if (!_oscillators[i].Contact.IsValid)
                {
                    _oscillators[i].Contact = newContact;
                    break;
                }
            }
        }

        foreach (var osc in _oscillators)
        {
            var c = osc.Contact;
            if (c.IsValid)
            {
                osc.Frequency = 55.0f * Mathf.Pow(2, c.X * 4);
                osc.Amplitude = Mathf.Clamp01(c.Force * 5);
                osc.Modulation = c.Y;
            }
            else
            {
                osc.Amplitude = 0;
            }
        }
    }

    void OnPcmRead(float[] data)
    {
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (var i = 0; i < data.Length; i++)
        {
            var v = 0.0f;
            foreach (var osc in _oscillators) v += osc.Tick();
            data[i] = v;
        }
    }
}
