using HarmonyLib;
using OfflineRaidGuard.Utils;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using System;
using Unity.Entities;

namespace OfflineRaidGuard.Hooks;

[HarmonyPatch]
internal class StatChange
{
    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.ApplyHealthChangeToEntity))]
    [HarmonyPrefix]
    private static void ApplyHealthChangeToEntity(StatChangeSystem __instance, ref StatChangeEvent statChange)
    {
        if (!Plugin.EnableMod.Value) return;

        if (!__instance.EntityManager.HasComponent<CastleHeartConnection>(statChange.Entity)) return;
        var heartEntity = __instance.EntityManager.GetComponentData<CastleHeartConnection>(statChange.Entity).CastleHeartEntity._Entity;

        if (!__instance.EntityManager.HasComponent<CastleHeart>(heartEntity)) return;
        var castleHeart = __instance.EntityManager.GetComponentData<CastleHeart>(heartEntity);

        if (castleHeart.State != CastleHeartState.IsProcessing) return;

        if (!Cache.PlyonOwnerCache.TryGetValue(heartEntity, out Entity userEntity))
        {
            userEntity = __instance.EntityManager.GetComponentData<UserOwner>(heartEntity).Owner._Entity;
        }

        Cache.PlayerCache.TryGetValue(userEntity, out var playerData);

        if (playerData.IsConnected == false)
        {
            if (Plugin.FactorAllies.Value)
            {
                var playerAllies = GetAllies(playerData.CharEntity);
                if (playerAllies.AllyCount > 0)
                {
                    foreach (var ally in playerAllies.Allies)
                    {
                        Cache.PlayerCache.TryGetValue(ally, out var allyData);
                        if (allyData.IsConnected) return;
                    }
                }
            }

            statChange.Change = 0;
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
