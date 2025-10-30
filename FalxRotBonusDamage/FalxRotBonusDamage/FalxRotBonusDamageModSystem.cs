using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace FalxRotBonusDamage
{
    public class FalxRotBonusDamageModSystem : ModSystem
    {
        private const string BehaviorCode = "falxrotbonusdamage:rustcreaturebonus";

        private FalxRotBonusDamageConfig config = new();
        private ICoreServerAPI? serverApi;

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterEntityBehaviorClass(BehaviorCode, typeof(EntityBehaviorRustCreatureBonusDamage));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);

            serverApi = api;

            LoadConfig(api);

            api.Event.PlayerJoin += EnsureBehaviorForExistingEntities;
            api.Event.OnEntityLoaded += OnEntityLoaded;
            api.Event.OnEntitySpawn += OnEntitySpawn;

            api.Event.ServerRunPhase(EnumServerRunPhase.RunGame, () =>
            {
                foreach (var entity in api.World.LoadedEntities)
                {
                    TryAttachBehavior(entity);
                }
            });
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }

        private void EnsureBehaviorForExistingEntities(IServerPlayer player)
        {
            if (player?.Entity == null) return;
            TryAttachBehavior(player.Entity);
        }

        private void OnEntityLoaded(Entity entity)
        {
            TryAttachBehavior(entity);
        }

        private void OnEntitySpawn(Entity entity)
        {
            TryAttachBehavior(entity);
        }

        private void TryAttachBehavior(Entity entity)
        {
            if (serverApi == null) return;
            if (entity == null || entity.World.Side != EnumAppSide.Server) return;
            if (!EntityHasRustCreatureTag(entity)) return;

            var existing = entity.GetBehavior<EntityBehaviorRustCreatureBonusDamage>();
            if (existing != null)
            {
                existing.Configure(config);
                return;
            }

            try
            {
                var behavior = new EntityBehaviorRustCreatureBonusDamage(entity);
                behavior.Configure(config);

                var attributes = new JsonObject(new JObject
                {
                    ["damageMultiplier"] = config.FalxRustCreatureDamageMultiplier
                });

                behavior.Initialize(entity.Properties, attributes);
                entity.AddBehavior(behavior);
            }
            catch (Exception ex)
            {
                Mod.Logger.Warning("Failed to attach falx rust creature bonus behavior to entity {0}: {1}", entity.Code, ex);
            }
        }

        private static bool EntityHasRustCreatureTag(Entity entity)
        {
            if (entity?.Properties == null) return false;

            JsonObject? tagsNode = entity.Properties.Attributes?["tags"];
            if (tagsNode == null) return false;

            string[]? tags = tagsNode.AsArray<string>();
            if (tags == null || tags.Length == 0) return false;

            return tags.Contains("rust-creature", StringComparer.OrdinalIgnoreCase);
        }

        private void LoadConfig(ICoreServerAPI api)
        {
            const string configFileName = "FalxRotBonusDamageConfig.json";

            config = api.LoadModConfig<FalxRotBonusDamageConfig>(configFileName) ?? new FalxRotBonusDamageConfig();

            if (config.FalxRustCreatureDamageMultiplier < 1f)
            {
                config.FalxRustCreatureDamageMultiplier = 1f;
            }

            api.StoreModConfig(config, configFileName);
        }
    }
}
