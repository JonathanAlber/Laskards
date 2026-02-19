using UnityEditor;
#if !UNITY_EDITOR
using UnityEngine;
#endif

namespace UI.Confirmation
{
    /// <summary>
    /// Closes the game or stops the editor when clicked.
    /// </summary>
    public class ConfirmedQuitButton : BaseConfirmationButton
    {
        protected override void OnClick() => ShowConfirmationBox();
        protected override void OnConfirm() => QuitApplication();

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}