using System.Collections.Generic;
using Unity.Entities;
using OfflineRaidGuard.Models;

namespace OfflineRaidGuard.Utils;

public static class Cache
{
    public static Dictionary<Entity, Entity> PlyonOwnerCache = new();
    public static Dictionary<Entity, Player> PlayerCache = new();
    public static Dictionary<Entity, PlayerGroup> AlliesCache = new();
}
