using Bloodstone.API;
using HarmonyLib;
using OfflineRaidGuard.Utils;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using System;
using System.IO;
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

        if (!statChange.HasFlag(StatChangeFlag.AnnounceCastleAttack))
        {
            return;
        }

        EntityComponentDumper("statChange.json", statChange.Entity);

        if (!VWorld.Server.EntityManager.HasComponent<CastleHeartConnection>(statChange.Entity))
        {
            Plugin.Logger.LogWarning($"No Castle Heart Connection Found");
            return;
        }
        
        var heartEntity = VWorld.Server.EntityManager.GetComponentData<CastleHeartConnection>(statChange.Entity).CastleHeartEntity._Entity;

        if (!VWorld.Server.EntityManager.HasComponent<CastleHeart>(heartEntity))
        {
            Plugin.Logger.LogWarning($"No Castle Heart Found");
            return;
        } 
        var castleHeart = VWorld.Server.EntityManager.GetComponentData<CastleHeart>(heartEntity);

        if (!castleHeart.State.HasFlag(CastleHeartState.IsProcessing))
        {
            Plugin.Logger.LogWarning($"Castle State ({castleHeart.State}) != IsProcessing; Castle must be decaying");
            return;
        }

        if (!Cache.PlyonOwnerCache.TryGetValue(heartEntity, out Entity userEntity))
        {
            Plugin.Logger.LogWarning($"PlyonOwnerCache did not contain this heart. Finding Owner");
            userEntity = VWorld.Server.EntityManager.GetComponentData<UserOwner>(heartEntity).Owner._Entity;
        }

        if (Cache.PlayerCache.TryGetValue(userEntity, out var playerData))
        {
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
                statChange.OriginalChange = 0;
            }
        }
        else
        {
            Plugin.Logger.LogWarning("Owner could not be found, damage will apply.");
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

    public static void EntityComponentDumper(string filePath, Entity entity)
    {
        File.AppendAllText(filePath, $"--------------------------------------------------" + Environment.NewLine);
        File.AppendAllText(filePath, $"Dumping components of {entity.ToString()}:" + Environment.NewLine);

        foreach (var componentType in VWorld.Server.EntityManager.GetComponentTypes(entity))
        { File.AppendAllText(filePath, $"{componentType.ToString()}" + Environment.NewLine); }

        File.AppendAllText(filePath, $"--------------------------------------------------" + Environment.NewLine);

        File.AppendAllText(filePath, DumpEntity(entity));
    }

    private static string DumpEntity(Entity entity, bool fullDump = true)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        ProjectM.EntityDebuggingUtility.DumpEntity(VWorld.Server, entity, fullDump, sb);
        return sb.ToString();
    }
}
