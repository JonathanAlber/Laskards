#if UNITY_EDITOR
using Gameplay.Board.Data;
using UnityEditor;

namespace Gameplay.CardExecution.Data.Editor
{
    /// <summary>
    /// Custom editor for <see cref="DeploymentZonesConfig"/> that validates
    /// the configured row limits against the project's <see cref="BoardConfiguration"/>.
    /// </summary>
    [CustomEditor(typeof(DeploymentZonesConfig))]
    public class DeploymentZonesConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DeploymentZonesConfig zones = (DeploymentZonesConfig)target;

            string[] guids = AssetDatabase.FindAssets("t:BoardConfiguration");

            if (guids.Length == 0)
            {
                EditorGUILayout.HelpBox($"No {nameof(BoardConfiguration)} asset found. " +
                                        "Create one to validate row limits.", MessageType.Info);
                return;
            }

            if (guids.Length > 1)
            {
                EditorGUILayout.HelpBox($"Multiple {nameof(BoardConfiguration)} assets found. " +
                                        "Validation is ambiguous. Keep only one in the project, or " +
                                        "temporarily move extras out to validate.", MessageType.Info);
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            BoardConfiguration boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfiguration>(path);

            if (boardConfig == null)
            {
                EditorGUILayout.HelpBox($"Failed to load {nameof(BoardConfiguration)} for" +
                                        " validation.", MessageType.Warning);
                return;
            }

            int totalRows = boardConfig.Rows;
            if (totalRows <= 0)
            {
                EditorGUILayout.HelpBox($"{nameof(BoardConfiguration)} '{boardConfig.name}' " +
                                        $"has Rows={totalRows}. Validation skipped.", MessageType.Warning);
                return;
            }


            if (zones.PlayerRowsFromBottom > totalRows)
            {
                EditorGUILayout.HelpBox($"PlayerRowsFromBottom ({zones.PlayerRowsFromBottom}) " +
                                        $"exceeds board rows ({totalRows}) in '{boardConfig.name}'. " +
                                        "Player deployment would be invalid.", MessageType.Warning);
            }

            if (zones.BossRowsFromTop > totalRows)
            {
                EditorGUILayout.HelpBox($"BossRowsFromTop ({zones.BossRowsFromTop}) exceeds board " +
                                        $"rows ({totalRows}) in '{boardConfig.name}'. Boss deployment" +
                                        $" would be invalid.", MessageType.Warning);
            }
        }
    }
}
#endif