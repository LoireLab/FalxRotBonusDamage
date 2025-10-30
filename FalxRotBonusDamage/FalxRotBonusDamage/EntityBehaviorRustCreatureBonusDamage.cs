using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace FalxRotBonusDamage
{
    public class EntityBehaviorRustCreatureBonusDamage : EntityBehavior
    {
        private float damageMultiplier = 1.25f;

        public EntityBehaviorRustCreatureBonusDamage(Entity entity) : base(entity)
        {
        }

        public override string PropertyName() => "falxrotbonusdamage:rustcreaturebonus";

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            if (attributes? ["damageMultiplier"].Exists == true)
            {
                damageMultiplier = Math.Max(1f, (float)attributes["damageMultiplier"].AsDouble(damageMultiplier));
            }
        }

        public void Configure(FalxRotBonusDamageConfig cfg)
        {
            if (cfg == null) return;
            damageMultiplier = Math.Max(1f, cfg.FalxRustCreatureDamageMultiplier);
        }

        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            if (damage <= 0 || entity.World.Side != EnumAppSide.Server) return;

            Entity? causeEntity = damageSource?.GetCauseEntity() ?? damageSource?.SourceEntity;
            if (causeEntity is not EntityAgent attacker) return;

            var weaponSlot = attacker.RightHandItemSlot;
            if (weaponSlot?.Itemstack == null) return;

            var collectible = weaponSlot.Itemstack.Collectible;
            if (collectible == null) return;

            if (!IsFalxBlade(collectible)) return;

            damage *= damageMultiplier;
        }

        private static bool IsFalxBlade(CollectibleObject collectible)
        {
            if (collectible.Code == null) return false;

            if (!string.Equals(collectible.FirstCodePart(), "blade", StringComparison.Ordinal)) return false;

            string secondPart;
            try
            {
                secondPart = collectible.FirstCodePart(1);
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }

            if (!string.Equals(secondPart, "falx", StringComparison.Ordinal)) return false;

            return true;
        }
    }
}
