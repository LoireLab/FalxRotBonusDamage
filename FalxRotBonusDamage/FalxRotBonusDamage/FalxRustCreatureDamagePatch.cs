using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace FalxRotBonusDamage
{
    [HarmonyPatchCategory("falxrotbonusdamage")]
    internal static class FalxRustCreatureDamagePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityBehaviorHealth), "OnEntityReceiveDamage")]
        private static void ApplyFalxBonusDamage(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
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

            if (!TryGetFalxWeapon(attacker, out _))
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

        private static bool TryGetFalxWeapon(EntityAgent attacker, out ItemStack? falxStack)
        {
            falxStack = attacker.ActiveHandItemSlot?.Itemstack;
            if (IsFalxBlade(falxStack))
            {
                return true;
            }

            falxStack = attacker.LeftHandItemSlot?.Itemstack;
            if (IsFalxBlade(falxStack))
            {
                return true;
            }

            falxStack = null;
            return false;
        }

        private static bool IsFalxBlade(ItemStack? stack)
        {
            var collectible = stack?.Collectible;
            if (collectible?.Code == null)
            {
                return false;
            }

            return collectible.Code.Path.StartsWith("blade-falx-");
        }
    }
}
