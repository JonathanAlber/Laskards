#if UNITY_EDITOR
using Gameplay.Cards.Data;
using UnityEditor;
using UnityEngine;
using Utility.Editor;

namespace Gameplay.Cards.Modifier.Data.Editor
{
    /// <summary>
    /// Custom editor for <see cref="ModifierData"/> ScriptableObjects.
    /// </summary>
    [CustomEditor(typeof(ModifierData), true)]
    public class ModifierDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty durationTypeProp = CustomEditorUtility.FindProp(serializedObject,
                nameof(ModifierData.DurationType));
            SerializedProperty durationProp = CustomEditorUtility.FindProp(serializedObject,
                nameof(ModifierData.Duration));
            SerializedProperty uniqueIdProp = CustomEditorUtility.FindProp(serializedObject,
                nameof(ModifierData.UniqueId));

            // Iterate through all visible properties in order
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                // Skip the script reference
                if (prop.name == EditorConstants.ScriptPropertyName)
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.PropertyField(prop, true);
                    continue;
                }

                // Skip properties we handle manually
                if (prop.propertyPath == durationProp?.propertyPath ||
                    prop.propertyPath == uniqueIdProp?.propertyPath)
                    continue;

                if (prop.propertyPath == durationTypeProp?.propertyPath)
                {
                    // Draw DurationType
                    EditorGUILayout.PropertyField(durationTypeProp);

                    // Conditionally draw Duration
                    if (durationTypeProp != null && (EDurationType)durationTypeProp.enumValueIndex == EDurationType.Temporary)
                        EditorGUILayout.PropertyField(durationProp);

                    continue;
                }

                EditorGUILayout.PropertyField(prop, true);
            }

            serializedObject.ApplyModifiedProperties();

            // Separator
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(10);

            // Display Unique ID
            ModifierData modifierData = (ModifierData)target;
            EditorGUILayout.LabelField(new GUIContent("Unique ID",
                "Automatically generated unique identifier for this card."), EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField(modifierData.UniqueId);
        }
    }
}
#endif