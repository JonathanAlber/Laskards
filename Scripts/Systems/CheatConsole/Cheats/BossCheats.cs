using Gameplay.Boss;
using Systems.Services;
using UnityEngine;
using Utility.Logging;

// ReSharper disable UnusedMember.Local

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Cheat commands for manipulating the boss's state.
    /// </summary>
    public class BossCheats : MonoBehaviour
    {
        [CheatCommand("win", Description = "Defeats the boss instantly.", Usage = "win")]
        private void DefeatBoss()
        {
            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            boss.ApplyDamage(boss.GetHealth());
            boss.ApplyDamage(boss.GetHealth());
        }

        [CheatCommand("boss_heal_full", Description = "Heals the boss to its maximum health.",
            Usage = "boss_heal_full")]
        private void HealBossFull()
        {
            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            if (boss.Model == null)
            {
                CustomLogger.LogWarning("Boss model is null, cannot heal to full.", null);
                return;
            }

            int maxHealth = boss.Model.MaxHealth;
            boss.SetHealth(maxHealth);
        }

        [CheatCommand("boss_heal", Description = "Heals the boss by the given amount.", Usage = "boss_heal <amount>")]
        private void HealBoss(int amount)
        {
            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            if (amount < 0)
                amount = 0;

            int current = boss.GetHealth();
            boss.SetHealth(current + amount);
        }

        [CheatCommand("boss_damage", Description = "Damages the boss by the given amount.",
            Usage = "boss_damage <amount>")]
        private void DamageBoss(int amount)
        {
            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            boss.ApplyDamage(amount);
        }

        [CheatCommand("boss_set_hp", Description = "Sets the boss's current HP.", Usage = "boss_set_hp <amount>")]
        private void SetBossHealth(int amount)
        {
            if (!ServiceLocator.TryGet(out BossController boss))
                return;

            boss.SetHealth(amount);
        }
    }
}