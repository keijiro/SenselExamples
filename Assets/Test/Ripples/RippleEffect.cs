using UnityEngine;
using Klak.Sensel;

namespace Cortina
{
    public class RippleEffect : MonoBehaviour
    {
        [SerializeField] float _speed = 1.3f;

        [SerializeField, HideInInspector] Shader _shader;
        Material _material;

        const int kMaxRipples = 12; // Also defined in .shader
        Vector4[] _ripples;
        int _rippleCount;

        void Start()
        {
            _ripples = new Vector4[kMaxRipples];
            _material = new Material(_shader);

            for (var i = 0; i < _ripples.Length; i++)
                _ripples[i] = new Vector4(0, 0, 0, -1e5f);
        }

        void OnDestroy()
        {
            Destroy(_material);
        }

        void Update()
        {
            var contacts = TouchInput.NewContacts;
            for (var i = 0; i < contacts.Length; i++)
            {
                var contact = contacts[i];
                _ripples[_rippleCount] = new Vector4(
                    contact.X, contact.Y, contact.Force, Time.time
                );
                _rippleCount = (_rippleCount + 1) % kMaxRipples;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _material.SetFloat("_Speed", _speed);
            _material.SetVectorArray("_Ripples", _ripples);
            Graphics.Blit(source, destination, _material, 0);
        }
    }
} 
