using System.Text;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace FalxRotBonusDamage
{
    [HarmonyPatchCategory("falxrotbonusdamage")]
    [HarmonyPatch(typeof(CollectibleObject), nameof(CollectibleObject.GetHeldItemInfo))]
    internal static class FalxDescriptionPatch
    {
        private static void Postfix(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            var stack = inSlot?.Itemstack;
            if (!FalxWeaponHelper.IsFalxBlade(stack))
            {
                return;
            }

            var multiplier = FalxRotBonusDamageModSystem.Config.RustCreatureDamageMultiplier;
            if (multiplier <= 1f)
            {
                return;
            }
            multiplier -= 1;
            multiplier *= 100;
            var formattedMultiplier = multiplier.ToString("0", GlobalConstants.DefaultCultureInfo);
            var description = Lang.Get("falxrotbonusdamage:iteminfo-falxrustmultiplier", formattedMultiplier);

            if (dsc.Length > 0 && dsc[dsc.Length - 1] != '\n')
            {
                dsc.AppendLine();
            }

            dsc.AppendLine(description);
        }
    }
}
