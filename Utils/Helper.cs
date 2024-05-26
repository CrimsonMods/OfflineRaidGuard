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
