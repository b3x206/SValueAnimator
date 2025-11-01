#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;
using BX.Editor.Utility;
using static BX.Editor.Utility.SerializedPropertyUtility;

namespace BX.Editor
{
    [CustomPropertyDrawer(typeof(ValueAnimatorBase.Sequence), true)]
    public class ValueAnimatorSequenceEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();
        private const float ClearFramesButtonHeight = 20f;
        private const float ReverseFramesButtonHeight = 20f;
        private const float MultiObjectWarningHeight = 26f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.YMargin;

            if (!property.isExpanded)
            {
                return height;
            }
            if (property.serializedObject.isEditingMultipleObjects)
            {
                height += MultiObjectWarningHeight + mainCtx.YMargin;
                return height;
            }

            // ValueAnimatorBase.Sequence.Duration
            height += EditorGUIUtility.singleLineHeight + mainCtx.YMargin;

            // GUI.Button = ValueAnimatorBase.Sequence.Clear();
            height += ClearFramesButtonHeight + mainCtx.YMargin;
            // GUI.Button = ValueAnimatorBase.Sequence.Reverse();
            height += ReverseFramesButtonHeight + mainCtx.YMargin;
            // Space
            height += 6f;

            bool loopPropertyValue = property.FindPropertyRelative(nameof(ValueAnimatorBase.Sequence.loop)).boolValue;

            foreach (var visibleProp in GetVisibleChildren(property))
            {
                if (!loopPropertyValue && visibleProp.name == nameof(ValueAnimatorBase.Sequence.loopMode))
                {
                    continue;
                }

                height += EditorGUI.GetPropertyHeight(visibleProp) + mainCtx.YMargin;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(mainCtx.GetRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                return;
            }
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(mainCtx.GetRect(position, MultiObjectWarningHeight), "Multiple object editing not supported", MessageType.Info);
                return;
            }

            ValueAnimatorBase.Sequence targetValue = GetTarget<ValueAnimatorBase.Sequence>(property);

            EditorGUI.indentLevel++;

            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
            {
                EditorGUI.FloatField(
                    mainCtx.GetRect(indentedPosition, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Total Duration", "The length (in seconds) that this animation will take."),
                    targetValue.Duration
                );
            }

            if (GUI.Button(mainCtx.GetRect(indentedPosition, ClearFramesButtonHeight), "Clear Frames"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Clear Frames");

                targetValue.Clear();
            }
            if (GUI.Button(mainCtx.GetRect(indentedPosition, ReverseFramesButtonHeight), "Reverse Frames"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Reverse Frames");

                targetValue.Reverse();
            }

            mainCtx.GetRect(position, 6); // Push space

            bool loopPropertyValue = property.FindPropertyRelative(nameof(ValueAnimatorBase.Sequence.loop)).boolValue;

            foreach (SerializedProperty visibleProp in GetVisibleChildren(property))
            {
                if (!loopPropertyValue && visibleProp.name == nameof(ValueAnimatorBase.Sequence.loopMode))
                {
                    continue;
                }

                EditorGUI.PropertyField(mainCtx.GetRect(indentedPosition, visibleProp), visibleProp, true);
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ValueAnimatorBase), true)]
    public class ValueAnimatorEditor : UnityEditor.Editor
    {
        private readonly new List<ValueAnimatorBase> targets = new List<ValueAnimatorBase>();
        private void PopulateTargets()
        {
            targets.Clear();
            targets.AddRange(base.targets.Cast<ValueAnimatorBase>());
        }

        /// <summary>
        /// String pointing to "m_Script"
        /// </summary>
        protected const string InjectedScriptPropertyName = "m_Script";
        public override void OnInspectorGUI()
        {
            using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(InjectedScriptPropertyName));
            }

            var target = base.target as ValueAnimatorBase;
            PopulateTargets();

            // weird rules for the serializedObject iterator
            SerializedProperty iterProp = serializedObject.GetIterator();
            iterProp.Next(true);

            foreach (SerializedProperty prop in GetVisibleChildren(iterProp))
            {
                if (prop.name == InjectedScriptPropertyName)
                {
                    continue;
                }

                if (prop.name == "m_CurrentAnimIndex")
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginHorizontal();
                    bool showMixed = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = targets.Any(n => n.CurrentAnimIndex != target.CurrentAnimIndex);
                    int animIndexSet = EditorGUILayout.IntField(
                        new GUIContent("Current Animation Index", "Sets the index from 'animation' array."),
                        target.CurrentAnimIndex
                    );
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        animIndexSet++;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        animIndexSet--;
                    }
                    EditorGUI.showMixedValue = showMixed;
                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.IncrementCurrentGroup();
                        int group = Undo.GetCurrentGroup();
                        Undo.SetCurrentGroupName("set CurrentAnimIndex");
                        for (int i = 0; i < targets.Count; i++)
                        {
                            Undo.RecordObject(target, string.Empty);
                            targets[i].CurrentAnimIndex = animIndexSet;
                        }
                        Undo.CollapseUndoOperations(group);
                    }

                    continue;
                }

                EditorGUILayout.PropertyField(prop);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
