using HarmonyLib;
using OfflineRaidGuard.Utils;
using ProjectM;
using Stunlock.Network;
using System;

namespace OfflineRaidGuard.Hooks;

[HarmonyPatch]
public static class Bootstrap
{
    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    [HarmonyPostfix]
    public static void Start()
    {
        Plugin.Initialize();
    }

    private static bool isInitialized = false;

    [HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    [HarmonyPostfix]
    public static void VerifyServerGameSettings()
    {
        if (isInitialized == false)
        {
            Plugin.Initialize();
            isInitialized = true;
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    [HarmonyPostfix]
    public static void OnUserConnected(ServerBootstrapSystem __instance, NetConnectionId netConnectionId) 
    {
        try
        {
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];

            Helper.UpdatePlayerCache(serverClient.UserEntity);
        }
        catch (Exception e) 
        {
            Plugin.Logger.LogWarning($"Error while updating user (Connected) to cache {e.Message}");
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
    [HarmonyPrefix]
    public static void OnUserDisconnected(ServerBootstrapSystem __instance, NetConnectionId netConnection, ConnectionStatusChangeReason reason, string extraData)
    {
        try
        {
            var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnection];
            var serverClient = __instance._ApprovedUsersLookup[userIndex];

            Helper.UpdatePlayerCache(serverClient.UserEntity, true);
        }
        catch (Exception e) 
        {
            Plugin.Logger.LogWarning($"Error while updating user (Disconnected) to cache {e.Message}");
        }
    }
}
