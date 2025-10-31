using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace FalxRotBonusDamage
{
    internal static class CombatOverhaulCompat
    {
        private const string HarmonyId = "falxrotbonusdamage.combatoverhaul";
        private const string DamageBehaviorTypeName = "CombatOverhaul.DamageSystems.EntityDamageModelBehavior";

        private static readonly FieldInfo? EntityField = AccessTools.Field(typeof(EntityBehavior), "entity");

        private static bool patched;

        internal static void TryRegister(ICoreAPI api)
        {
            if (patched)
            {
                return;
            }

            var damageBehaviorType = AccessTools.TypeByName(DamageBehaviorTypeName);
            if (damageBehaviorType == null)
            {
                return;
            }

            var damageHandler = AccessTools.Method(damageBehaviorType, "OnReceiveDamageHandler", new[] { typeof(float), typeof(DamageSource) });
            if (damageHandler == null)
            {
                FalxRotBonusDamageModSystem.Logger?.Warning("Combat Overhaul detected but damage handler method was not found; compatibility patch skipped.");
                return;
            }

            var prefix = new HarmonyMethod(typeof(CombatOverhaulCompat).GetMethod(nameof(ApplyFalxBonusDamage), BindingFlags.Static | BindingFlags.NonPublic));

            new Harmony(HarmonyId).Patch(damageHandler, prefix: prefix);

            FalxRustCreatureDamagePatch.UseVanillaDamagePatch = false;
            patched = true;

            FalxRotBonusDamageModSystem.Logger?.Event("Combat Overhaul detected; Falx rust-creature bonus routed through compatibility patch.");
        }

        private static void ApplyFalxBonusDamage(object __instance, DamageSource damageSource, ref float damage)
        {
            if (damage <= 0f || damageSource == null)
            {
                return;
            }

            var target = EntityField?.GetValue(__instance) as Entity;
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
