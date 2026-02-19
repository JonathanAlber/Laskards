#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Boss.Data.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="BossData"/> that supports multi-object editing
    /// and provides Add/Remove buttons for the global <see cref="BossDataCollection"/>.
    /// </summary>
    [CustomEditor(typeof(BossData))]
    [CanEditMultipleObjects]
    public class BossDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(10);

            EditorGUILayout.LabelField(
                new GUIContent(
                    "Boss Data Collection",
                    $"Manage inclusion in the global {nameof(BossDataCollection)}. " +
                    "Only bosses included here will be selectable in-game."
                ),
                EditorStyles.boldLabel
            );

            BossDataCollection collection = BossDataCollectionEditorUtils.FindCollection();
            if (collection == null)
            {
                EditorGUILayout.HelpBox(
                    $"No {nameof(BossDataCollection)} found. Please create one to manage available bosses.",
                    MessageType.Warning
                );
                return;
            }

            // Convert all selected objects to BossData
            BossData[] selectedBosses = targets.OfType<BossData>().Where(b => b != null).ToArray();
            if (selectedBosses.Length == 0)
                return;

            bool anyIn = selectedBosses.Any(b => collection.BossData.Contains(b));
            bool anyOut = selectedBosses.Any(b => !collection.BossData.Contains(b));

            string selectionLabel = selectedBosses.Length == 1
                ? selectedBosses[0].name
                : $"{selectedBosses.Length} selected";

            EditorGUILayout.LabelField("Selection", selectionLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Add: enabled if at least one is not in the collection
                using (new EditorGUI.DisabledScope(!anyOut))
                {
                    if (GUILayout.Button(selectedBosses.Length == 1 ? "Add" : "Add All"))
                    {
                        Undo.RecordObject(collection, "Add BossData to Collection");

                        foreach (BossData b in selectedBosses)
                            BossDataCollectionEditorUtils.AddToCollection(b, collection);

                        // Utils already SaveAssets/SetDirty, but undo likes explicit dirty too.
                        EditorUtility.SetDirty(collection);
                    }
                }

                // Remove: enabled if at least one is in the collection
                using (new EditorGUI.DisabledScope(!anyIn))
                {
                    if (GUILayout.Button(selectedBosses.Length == 1 ? "Remove" : "Remove All"))
                    {
                        Undo.RecordObject(collection, "Remove BossData from Collection");

                        foreach (BossData b in selectedBosses)
                            BossDataCollectionEditorUtils.RemoveFromCollection(b, collection);

                        EditorUtility.SetDirty(collection);
                    }
                }
            }
        }
    }
}
#endif