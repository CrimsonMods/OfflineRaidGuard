using Bloodstone.API;
using HarmonyLib;
using OfflineRaidGuard.Utils;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using System;
using Unity.Collections;
using Unity.Entities;

namespace OfflineRaidGuard.Hooks;

[HarmonyPatch]
internal class StatChange
{
    [HarmonyPatch(typeof(DealDamageSystem), nameof(DealDamageSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(DealDamageSystem __instance)
    {
        NativeArray<Entity> entities = __instance._Query.ToEntityArray(Allocator.TempJob);
        try
        {
            foreach (Entity entity in entities)
            {
                if (!Plugin.EnableMod.Value) continue;

                DealDamageEvent dealDamageEvent = entity.Read<DealDamageEvent>();
                if (dealDamageEvent.MainType != MainDamageType.Physical && dealDamageEvent.MainType != MainDamageType.Spell) continue;

                if (!dealDamageEvent.Target.TryGetComponent(out CastleHeartConnection heartConn)) continue;
                if (!heartConn.CastleHeartEntity._Entity.TryGetComponent(out CastleHeart heart)) continue;
                if (!heart.State.HasFlag(CastleHeartState.IsProcessing)) continue;
                if (!heartConn.CastleHeartEntity._Entity.TryGetComponent(out UserOwner owner)) continue;
                if (!owner.Owner._Entity.TryGetComponent(out User ownerUser)) continue;

                Entity clanEntity = VWorld.Server.EntityManager.Exists(ownerUser.ClanEntity._Entity) ? ownerUser.ClanEntity._Entity : Entity.Null;

                if (Plugin.FactorAllies.Value && !clanEntity.Equals(Entity.Null))
                {
                    var userBufffer = ownerUser.ClanEntity._Entity.ReadBuffer<SyncToUserBuffer>();

                    foreach (var item in userBufffer)
                    {
                        Entity clanUser = item.UserEntity;
                        if (clanUser.TryGetComponent(out User clanPlayer))
                        {
                            if (clanPlayer.IsConnected) continue;

                            if (clanPlayer.TimeLastConnected < Plugin.MaxAllyCacheAge.Value) continue;
                        }
                        else continue;
                    }
                }

                if (ownerUser.IsConnected) continue;
                if (ownerUser.TimeLastConnected < Plugin.MaxAllyCacheAge.Value) continue;

                VWorld.Server.EntityManager.DestroyEntity(entity);
                continue;
            }
        }
        catch (Exception ex)
        {
            
        }
        finally
        {
            entities.Dispose();
        }
    }

    private static PlayerGroup GetAllies(Entity characterEntity)
    {
        if (Cache.AlliesCache.TryGetValue(characterEntity, out var playerGroup))
        {
            TimeSpan CacheAge = DateTime.Now - playerGroup.TimeStamp;
            if (CacheAge.TotalSeconds > Plugin.MaxAllyCacheAge.Value) goto UpdateCache;
            goto ReturnResult;
        }

    UpdateCache:
        int allyCount = Helper.GetAllies(characterEntity, out var Group);
        playerGroup = new PlayerGroup()
        {
            AllyCount = allyCount,
            Allies = Group,
            TimeStamp = DateTime.Now
        };
        Cache.AlliesCache[characterEntity] = playerGroup;

    ReturnResult:
        return playerGroup;
    }
}
