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
    /// Custom property drawer for selecting boss unit card definitions that
    /// automatically filters options based on their team affiliation.
    /// </summary>
    [CustomPropertyDrawer(typeof(BossUnitCardAttribute))]
    public class BossUnitCardDefinitionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BossCardDefinitionCollection bossCollection = BossEditorUtils.FindBossCollection();
            if (bossCollection == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            List<UnitCardDefinition> options = bossCollection.Cards
                .OfType<UnitCardDefinition>()
                .Where(c => c.TeamAffiliation is ECardTeamAffiliation.Boss or ECardTeamAffiliation.Both)
                .ToList();

            PropertyDrawerUtility.DrawObjectPopup(position, property, label, options);
        }
    }
}
#endif