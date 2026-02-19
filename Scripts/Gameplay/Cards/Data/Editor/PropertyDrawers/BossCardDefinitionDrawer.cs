#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Gameplay.Cards.Data.Properties;
using UnityEditor;
using UnityEngine;
using Utility.Editor;

namespace Gameplay.Cards.Data.Editor.PropertyDrawers
{
    /// <summary>
    /// Custom property drawer for selecting boss card definitions that automatically filters options
    /// based on their team affiliation.
    /// </summary>
    [CustomPropertyDrawer(typeof(BossCardAttribute), true)]
    public class BossCardDefinitionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BossCardDefinitionCollection bossCollection = BossEditorUtils.FindBossCollection();
            if (bossCollection == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            List<CardDefinition> options = bossCollection.Cards
                .Where(c => c && c.TeamAffiliation is ECardTeamAffiliation.Boss or ECardTeamAffiliation.Both)
                .ToList();

            PropertyDrawerUtility.DrawObjectPopup(position, property, label, options);
        }
    }
}
#endif