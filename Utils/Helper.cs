using OfflineRaidGuard.Models;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using Stunlock.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace OfflineRaidGuard.Utils;

public static class Helper
{
    public static ServerGameManager SGM = default;

    public static bool GetServerGameManager(out ServerGameManager sgm)
    {
        sgm = Plugin.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
        return true;
    }

    public static int GetAllies(Entity CharacterEntity, out List<Entity> Group)
    {
        Group = new();

        var users = Plugin.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);

        foreach (var user in users)
        {
            if (user.Equals(CharacterEntity)) continue;

            if (SGM.IsAllies(CharacterEntity, user))
            {
                Group.Add(user);
            }
        }

        return Group.Count;
    }

    public static void CreatePlayerCache()
    {
        Cache.PlayerCache.Clear();
        var userEntities = Plugin.Server.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>()).ToEntityArray(Allocator.Temp);
        foreach (var userEntity in userEntities)
        {
            var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);
            Player player = new Player(userData.PlatformId, userData.CharacterName.ToString(), userData.IsConnected, userEntity, userData.LocalCharacter._Entity);
            Cache.PlayerCache.Add(userEntity, player);
        }
    }

    public static void UserConnected(ServerBootstrapSystem sender, NetConnectionId netConnectionId)
    {
        try
        {
            var userIndex = sender._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = sender._ApprovedUsersLookup[userIndex];

            UpdatePlayerCache(serverClient.UserEntity);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Error while updating user (Connected) to cache {e.Message}");
        }
    }

    public static void UserDisconnected(ServerBootstrapSystem sender, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
    {
        try
        {
            var userIndex = sender._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = sender._ApprovedUsersLookup[userIndex];
            var userEntity = serverClient.UserEntity;

            if (Cache.PlayerCache.ContainsKey(userEntity))
            {
                UpdatePlayerCache(userEntity, true);
            }
            else
            {
                Plugin.Logger.LogWarning($"Wasn't able to find user in cache.");
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Error while updating user (Disconnected) to cache {e.Message}");
        }
    }

    public static void UpdatePlayerCache(Entity userEntity, bool forceOffline = false)
    {
        var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);
        if (userData.CharacterName.IsEmpty) return;
        
        if (forceOffline)
        {
            userData.IsConnected = false;
        }

        Player player = new Player(userData.PlatformId, userData.CharacterName.ToString(), userData.IsConnected, userEntity, userData.LocalCharacter._Entity);
        Cache.PlayerCache[userEntity] = player;
    }

    public static PrefabGUID GetPrefabGUID(Entity entity)
    {
        var entityManager = Plugin.Server.EntityManager;
        PrefabGUID guid;
        try
        {
            guid = entityManager.GetComponentData<PrefabGUID>(entity);
        }
        catch
        {
            return new PrefabGUID(0);
        }
        return guid;
    }

    public static string GetPrefabName(PrefabGUID hashCode)
    {
        var s = Plugin.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
        string name = "Nonexistent";
        if (hashCode.GuidHash == 0)
        {
            return name;
        }
        try
        {
            name = s.PrefabLookupMap[hashCode].ToString();
        }
        catch
        {
            name = "NoPrefabName";
        }
        return name;
    }
}
