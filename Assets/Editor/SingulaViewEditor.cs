#if UNITY_EDITOR

using Construct.Views;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System;
using Construct.Model;

namespace Editor
{
    [CustomEditor(typeof(SingulaView))]
    public sealed class SingulaViewEditor : UnityEditor.Editor
    {
        private SingulaView _singulaTarget;
        private ReorderableList _pimpleList;

        private readonly float ITEM_LIST_HEIGHT = EditorGUIUtility.singleLineHeight + 5;

        private void OnEnable()
        {
            _singulaTarget = target as SingulaView;
            _pimpleList = new(
                serializedObject: serializedObject,
                elements: serializedObject.FindProperty("Pimples"),
                draggable: true,
                displayHeader: true,
                displayAddButton: true,
                displayRemoveButton: true);

            _pimpleList.drawHeaderCallback = DrawHeader;
            _pimpleList.drawElementCallback = DrawListItems;
            _pimpleList.elementHeight = ITEM_LIST_HEIGHT;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("EcsEntity"));

            EditorGUILayout.Separator();

            _pimpleList.DoLayoutList();

            EditorGUILayout.Separator();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() {
            if (_singulaTarget.Pimples is null) _singulaTarget.Pimples = Array.Empty<Pimple>();

            Handles.color = Color.red;

            for (int i = 0; i < _singulaTarget.Pimples.Length; i++) {
                EditorGUI.BeginChangeCheck();
                var pimple = _singulaTarget.Pimples[i];
                var position = _singulaTarget.transform.TransformPoint(pimple.Position);

                var newPosition = Handles.FreeMoveHandle(
                    position,
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(position) * 0.05f,
                    Vector3.zero,
                    Handles.DotHandleCap);

                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(_singulaTarget, "Change leg vertex position");
                    var value = pimple;
                    value.Position = _singulaTarget.transform.InverseTransformPoint(newPosition);
                    _singulaTarget.Pimples[i] = value;
                    serializedObject.Update();
                }
            }
        }

        private void DrawHeader(Rect rect)
        {
            const string headerName = "Pimples";
            EditorGUI.LabelField(rect, headerName);
        }

        private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _pimpleList.serializedProperty.GetArrayElementAtIndex(index);

            var labelRect = new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight);
            var propertyRect = new Rect(rect.x + labelRect.width, rect.y, rect.width - labelRect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(labelRect, "Position");
            EditorGUI.PropertyField(propertyRect, element.FindPropertyRelative("Position"), GUIContent.none);
        }
    }
}

#endif