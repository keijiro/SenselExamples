using UnityEngine;

namespace Cortina
{
    [ExecuteInEditMode]
    sealed public class RainRenderer : MonoBehaviour
    {
        public float Throttle {
            get { return _throttle; }
            set { _throttle = value; }
        }

        public float LengthAmplitude {
            get { return _lengthAmplitude; }
            set { _lengthAmplitude = value; }
        }

        [SerializeField] int _lineCount = 1000;
        [SerializeField] float _speed = 10;
        [SerializeField, Range(0, 1)] float _speedRandomness = 0.5f;
        [SerializeField] float _length = 1;
        [SerializeField, Range(0, 1)] float _lengthRandomness = 0.5f;
        [SerializeField] Vector3 _extent = Vector3.one * 10;
        [SerializeField, ColorUsage(false)] Color _color = Color.white;

        [SerializeField, HideInInspector] Shader _shader;

        float _throttle = 1;
        float _lengthAmplitude = 1;
        Material _material;

        void OnValidate()
        {
            _lineCount = Mathf.Max(_lineCount, 0);
            _speed = Mathf.Max(_speed, 0);
            _length = Mathf.Max(_length, 0);
            _extent = Vector3.Max(_extent, Vector3.zero);
        }

        void OnDestroy()
        {
            if (_material != null)
            {
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
            }
        }

        void Update()
        {
            if (_material == null)
            {
                _material = new Material(_shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            var nspeed = new Vector2(1 - _speedRandomness, 1);
            nspeed *= _speed / (_extent.z * 2);

            var length = new Vector2(1 - _lengthRandomness, 1) * _length;

            _material.SetVector("_NSpeed", nspeed);
            _material.SetVector("_Length", length * _lengthAmplitude);
            _material.SetVector("_Extent", _extent);
            _material.SetColor("_Color", _color);
            _material.SetMatrix("_ObjectMatrix", transform.localToWorldMatrix);

            if (Application.isPlaying)
                _material.SetFloat("_LocalTime", 10 + Time.time);
            else
                _material.SetFloat("_LocalTime", 10);
        }

        void OnRenderObject()
        {
            if (_material == null) return;
            _material.SetPass(0);
            var count = (int)(_lineCount * _throttle) * 2;
            Graphics.DrawProcedural(MeshTopology.Lines, count, 1);
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, _extent * 2);
        }
    }
}
