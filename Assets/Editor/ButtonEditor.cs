using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BookLib
{
    [CustomEditor(typeof(Button)), CanEditMultipleObjects]
    public class ButtonEditor : Editor
    {
        public SerializedProperty
            ButtonType_p,
            SizeValue_p,
            ColorValue_p,
            AlphaValue_p,
            Controller_p,
            Img_p;

        void OnEnable()
        {
            ButtonType_p = serializedObject.FindProperty("buttonType");
            SizeValue_p = serializedObject.FindProperty("sizeValue");
            ColorValue_p = serializedObject.FindProperty("colorValue");
            AlphaValue_p = serializedObject.FindProperty("alphaValue");
            Controller_p = serializedObject.FindProperty("control");
            Img_p = serializedObject.FindProperty("img");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(ButtonType_p);

            Button.ButtonType t = (Button.ButtonType)ButtonType_p.enumValueIndex;

            switch (t)
            {
                case Button.ButtonType.Color:
                    EditorGUILayout.PropertyField(ColorValue_p, new GUIContent("Value"));
                    break;
                case Button.ButtonType.Size:
                    EditorGUILayout.PropertyField(SizeValue_p, new GUIContent("Value"));
                    break;
                case Button.ButtonType.Alpha:
                    EditorGUILayout.PropertyField(AlphaValue_p, new GUIContent("Value"));
                    break;
            }
            EditorGUILayout.PropertyField(Controller_p, new GUIContent("Player Controller"));
            EditorGUILayout.PropertyField(Img_p, new GUIContent("Image Tools"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}