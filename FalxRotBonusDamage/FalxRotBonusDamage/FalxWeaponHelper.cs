using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace FalxRotBonusDamage
{
    internal static class FalxWeaponHelper
    {
        internal static bool TryGetFalxWeapon(EntityAgent attacker, out ItemStack? falxStack)
        {
            falxStack = attacker.ActiveHandItemSlot?.Itemstack;
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
