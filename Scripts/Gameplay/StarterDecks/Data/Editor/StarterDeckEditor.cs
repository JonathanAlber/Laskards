#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gameplay.StarterDecks.Data.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="StarterDeckDefinition"/>. Displays its Unique ID,
    /// validation tools, and buttons to manage its presence in the global <see cref="StarterDeckCollection"/>.
    /// </summary>
    [CustomEditor(typeof(StarterDeckDefinition))]
    public class StarterDeckDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StarterDeckDefinition deck = (StarterDeckDefinition)target;
            GUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Starter Deck Collection management
            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Starter Deck Collection", "Manage this deck's " +
                $"inclusion in the global {nameof(StarterDeckCollection)}. Only the decks included here will be " +
                "choosable for the player"), EditorStyles.boldLabel);

            StarterDeckCollection collection = StarterDeckCollectionEditorUtils.FindCollection();
            if (collection == null)
            {
                EditorGUILayout.HelpBox($"No {nameof(StarterDeckCollection)} found. Please create one to " +
                                        "manage available decks.", MessageType.Warning);
                return;
            }

            bool isInCollection = collection.Decks.Contains(deck);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isInCollection))
                {
                    if (GUILayout.Button("Add"))
                        StarterDeckCollectionEditorUtils.AddToCollection(deck, collection);
                }

                using (new EditorGUI.DisabledScope(!isInCollection))
                {
                    if (GUILayout.Button("Remove"))
                        StarterDeckCollectionEditorUtils.RemoveFromCollection(deck, collection);
                }
            }

            // Unique ID field
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Unique ID", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField(deck.UniqueId);
        }
    }
}
#endif