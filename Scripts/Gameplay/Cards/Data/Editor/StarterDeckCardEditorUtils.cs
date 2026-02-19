#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Gameplay.StarterDecks.Data;
using Gameplay.StarterDecks.Data.Editor;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Cards.Data.Editor
{
    /// <summary>
    /// Provides a shared editor UI for adding/removing cards from starter decks.
    /// </summary>
    public static class StarterDeckCardEditorUtils
    {
        public static void DrawStarterDeckSection(CardDefinition card)
        {
            List<StarterDeckDefinition> decks = StarterDeckEditorUtils.FindAllStarterDecks();

            if (decks.Count == 0)
            {
                EditorGUILayout.HelpBox($"No {nameof(StarterDeckDefinition)} assets found in project.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(new GUIContent("Starter Decks",
                "Manage how many copies of this card are in each starter deck."), EditorStyles.boldLabel);

            foreach (StarterDeckDefinition deck in decks)
            {
                int count = deck.Cards.Count(c => c == card);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(deck.DisplayName, GUILayout.Width(160));
                    int newCount = EditorGUILayout.IntField(count, GUILayout.Width(40));

                    if (newCount != count)
                    {
                        Undo.RecordObject(deck, "Modify Starter Deck Card Count");
                        if (newCount < count)
                        {
                            for (int i = 0; i < count - newCount; i++)
                                deck.Cards.Remove(card);
                        }
                        else
                        {
                            for (int i = 0; i < newCount - count; i++)
                                deck.Cards.Add(card);
                        }
                        EditorUtility.SetDirty(deck);
                    }

                    if (GUILayout.Button("+", GUILayout.Width(24)))
                    {
                        Undo.RecordObject(deck, "Add Card to Deck");
                        deck.Cards.Add(card);
                        EditorUtility.SetDirty(deck);
                    }

                    using (new EditorGUI.DisabledScope(count == 0))
                    {
                        if (!GUILayout.Button("-", GUILayout.Width(24)))
                            continue;

                        Undo.RecordObject(deck, "Remove Card from Deck");
                        deck.Cards.Remove(card);
                        EditorUtility.SetDirty(deck);
                    }
                }
            }
        }
    }
}
#endif