using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace FalxRotBonusDamage
{
    [HarmonyPatchCategory("falxrotbonusdamage")]
    internal static class FalxRustCreatureDamagePatch
    {
        internal static bool UseVanillaDamagePatch { get; set; } = true;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityBehaviorHealth), "OnEntityReceiveDamage")]
        private static void ApplyFalxBonusDamage(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
            if (!UseVanillaDamagePatch)
            {
                return;
            }

            if (damage <= 0f || damageSource == null)
            {
                return;
            }

            var target = __instance?.entity;
            if (target == null || !target.Alive || !target.HasTags("rust-creature"))
            {
                return;
            }

            if (damageSource.GetCauseEntity() is not EntityAgent attacker)
            {
                return;
            }

            if (!FalxWeaponHelper.TryGetFalxWeapon(attacker, out _))
            {
                return;
            }

            var multiplier = FalxRotBonusDamageModSystem.Config.RustCreatureDamageMultiplier;
            if (multiplier <= 1f)
            {
                return;
            }

            damage *= multiplier;
        }

    }
}
