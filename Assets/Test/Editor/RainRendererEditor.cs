using UnityEngine;
using UnityEditor;

namespace Cortina
{
    [CustomEditor(typeof(RainRenderer))]
    sealed class RainRendererEditor : Editor
    {
        SerializedProperty _lineCount;
        SerializedProperty _speed;
        SerializedProperty _speedRandomness;
        SerializedProperty _length;
        SerializedProperty _lengthRandomness;
        SerializedProperty _extent;
        SerializedProperty _color;

        static class Styles
        {
            public static readonly GUIContent Randomness = new GUIContent("Randomness");
        }

        void OnEnable()
        {
            _lineCount = serializedObject.FindProperty("_lineCount");
            _speed = serializedObject.FindProperty("_speed");
            _speedRandomness = serializedObject.FindProperty("_speedRandomness");
            _length = serializedObject.FindProperty("_length");
            _lengthRandomness = serializedObject.FindProperty("_lengthRandomness");
            _extent = serializedObject.FindProperty("_extent");
            _color = serializedObject.FindProperty("_color");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_lineCount);
            EditorGUILayout.PropertyField(_speed);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_speedRandomness, Styles.Randomness);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_length);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_lengthRandomness, Styles.Randomness);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_extent);
            EditorGUILayout.PropertyField(_color);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
