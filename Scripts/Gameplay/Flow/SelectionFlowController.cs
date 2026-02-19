using UnityEngine;
using Systems.Services;
using Gameplay.Boss.Data;
using Systems.MenuManaging;
using Gameplay.StarterDecks.Data;

namespace Gameplay.Flow
{
    /// <summary>
    /// Controls the flow of boss and starter deck selection menus based on the current game context.
    /// </summary>
    public class SelectionFlowController : MonoBehaviour
    {
        private void Start()
        {
            if (!ServiceLocator.TryGet(out GameContextService gameContextService))
                return;

            gameContextService.RaiseCachedSelectionsIfAny();

            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.TryGetMenu(EMenuIdentifier.BossSelection, out Menu bossSelectionMenu))
                return;

            if (!menuManager.TryGetMenu(EMenuIdentifier.StarterDeckSelection, out Menu starterDeckSelectionMenu))
                return;

            if (gameContextService.SelectedBoss == null)
            {
                bossSelectionMenu.Open();

                if (starterDeckSelectionMenu.IsOpen)
                    starterDeckSelectionMenu.Close();

                return;
            }

            if (gameContextService.SelectedStarterDeck == null)
            {
                if (bossSelectionMenu.IsOpen)
                    bossSelectionMenu.Close();

                starterDeckSelectionMenu.Open();

                return;
            }

            if (bossSelectionMenu.IsOpen)
                bossSelectionMenu.Close();

            if (starterDeckSelectionMenu.IsOpen)
                starterDeckSelectionMenu.Close();
        }

        private void OnEnable()
        {
            GameContextService.OnBossChosen += HandleBossChosen;
            GameContextService.OnStarterDeckChosen += HandleDeckChosen;
        }

        private void OnDisable()
        {
            GameContextService.OnBossChosen -= HandleBossChosen;
            GameContextService.OnStarterDeckChosen -= HandleDeckChosen;
        }

        private static void HandleBossChosen(BossData _)
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.TryGetMenu(EMenuIdentifier.StarterDeckSelection, out Menu starterDeckSelectionMenu))
                return;

            starterDeckSelectionMenu.Open();
        }

        private static void HandleDeckChosen(StarterDeckDefinition _)
        {
            if (!ServiceLocator.TryGet(out MenuManager menuManager))
                return;

            if (!menuManager.TryGetMenu(EMenuIdentifier.StarterDeckSelection, out Menu starterDeckSelectionMenu))
                return;

            if (!menuManager.TryGetMenu(EMenuIdentifier.BossSelection, out Menu bossSelectionMenu))
                return;

            if (starterDeckSelectionMenu.IsOpen)
                starterDeckSelectionMenu.Close();

            if (bossSelectionMenu.IsOpen)
                bossSelectionMenu.Close();
        }
    }
}