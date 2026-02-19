#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gameplay.Cards.Data.Editor
{
    /// <summary>
    /// Custom editor for <see cref="ActionCardDefinition"/>.
    /// Adds membership management for the <see cref="PlayerCardDefinitionCollection"/>.
    /// </summary>
    [CustomEditor(typeof(ActionCardDefinition))]
    public class ActionCardDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ActionCardDefinition card = (ActionCardDefinition)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Starter Deck Management
            StarterDeckCardEditorUtils.DrawStarterDeckSection(card);

            // Player Collection Membership
            EditorGUILayout.Space(15);

            if (card.TeamAffiliation is ECardTeamAffiliation.Player or ECardTeamAffiliation.Both)
            {
                PlayerCardDefinitionCollection collection = PlayerCardDefinitionCollectionEditorUtils.FindCollection();
                if (collection != null)
                {
                    EditorGUILayout.LabelField(new GUIContent("Player Card Collection",
                            "Adding this card makes it available in the shop / player progression."),
                        EditorStyles.boldLabel);

                    bool isInCollection = collection.Cards.Contains(card);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(isInCollection))
                        {
                            if (GUILayout.Button("Add", GUILayout.Width(64)))
                                PlayerCardDefinitionCollectionEditorUtils.AddToCollection(card, collection);
                        }

                        using (new EditorGUI.DisabledScope(!isInCollection))
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(64)))
                                PlayerCardDefinitionCollectionEditorUtils.RemoveFromCollection(card, collection);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"No {nameof(PlayerCardDefinitionCollection)} found." +
                                            " Please create one.", MessageType.Warning);
                }
            }

            // Boss Collection Membership
            EditorGUILayout.Space(10);

            if (card.TeamAffiliation is ECardTeamAffiliation.Boss or ECardTeamAffiliation.Both)
            {
                BossCardDefinitionCollection bossCollection = BossCardDefinitionCollectionEditorUtils.FindCollection();

                if (bossCollection != null)
                {
                    EditorGUILayout.LabelField(new GUIContent("Boss Card Collection",
                            "Adding this card allows the boss to use it."),
                        EditorStyles.boldLabel);

                    bool isInBossCollection = bossCollection.Cards.Contains(card);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(isInBossCollection))
                        {
                            if (GUILayout.Button("Add", GUILayout.Width(64)))
                                BossCardDefinitionCollectionEditorUtils.AddToCollection(card, bossCollection);
                        }

                        using (new EditorGUI.DisabledScope(!isInBossCollection))
                        {
                            if (GUILayout.Button("Remove", GUILayout.Width(64)))
                                BossCardDefinitionCollectionEditorUtils.RemoveFromCollection(card, bossCollection);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox($"No {nameof(BossCardDefinitionCollection)} found." +
                                            " Please create one.", MessageType.Warning);
                }
            }

            // Unique ID
            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField(new GUIContent("Unique ID", "This is an automatically generated " +
                                                        "unique identifier for this card."), EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.TextField(card.UniqueId);
        }
    }
}
#endif