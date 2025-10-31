using HarmonyLib;
using Vintagestory.API.Common;

namespace FalxRotBonusDamage
{
    public class FalxRotBonusDamageModSystem : ModSystem
    {
        private static readonly object PatchLock = new();
        private static bool patched;

        internal static FalxRotBonusDamageConfig Config { get; private set; } = new();
        internal static ILogger? Logger { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Logger = api.Logger;
            LoadConfig(api);
            EnsurePatched();
        }

        private void LoadConfig(ICoreAPI api)
        {
            var loaded = api.LoadModConfig<FalxRotBonusDamageConfig>("FalxRotBonusDamage.json");

            if (loaded == null)
            {
                loaded = new FalxRotBonusDamageConfig();

                if (api.Side == EnumAppSide.Server)
                {
                    api.StoreModConfig(loaded, "FalxRotBonusDamage.json");
                }
            }

            Config = loaded.EnsureValid();
        }

        private void EnsurePatched()
        {
            if (patched) return;

            lock (PatchLock)
            {
                if (patched) return;

                var harmony = new Harmony("falxrotbonusdamage.bonuses");
                harmony.PatchAll();

                patched = true;

                Mod.Logger.Event("Falx rot bonus damage patch active. Multiplier={0}", Config.RustCreatureDamageMultiplier);
            }
        }
    }
}