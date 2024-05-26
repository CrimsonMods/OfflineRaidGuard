using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Unity.Entities;
using Bloodstone.API;
using OfflineRaidGuard.Utils;
using Bloody.Core;
using Bloody.Core.API.v1;

namespace OfflineRaidGuard;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.Bloodstone")]
[BepInDependency("trodi.Bloody.Core")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin, IRunOnInitialized
{
    private Harmony harmony;

    public static ConfigEntry<bool> EnableMod;
    public static ConfigEntry<bool> FactorAllies;
    public static ConfigEntry<int> MaxAllyCacheAge;

    public static bool isInitialized = false;

    public static ManualLogSource Logger;

    private static World _serverWorld;

    public static SystemsCore SystemsCore;

    public static World Server
    {
        get
        {
            if (_serverWorld != null) return _serverWorld;

            _serverWorld = GetWorld("Server")
                ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
            return _serverWorld;
        }
    }

    public static bool IsServer => VWorld.IsServer;

    private static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            if (world.Name == name)
            {
                return world;
            }
        }

        return null;
    }

    public void InitConfig()
    {
        EnableMod = Config.Bind("Config", "Enable Mod", true, "Enable/disable the mod.");
        FactorAllies = Config.Bind("Config", "Factor in Ally Status", true, "Include the besieged player allies online status before blocking siege.");
        MaxAllyCacheAge = Config.Bind("Config", "Max Ally Cache Age", 300, "Max age of the besieged player allies cache in seconds.\n" +
            "If the cache age is older than specified, the cache will be renewed.\n" +
            "Don't set this too short as allies gathering process can slightly impact your server performance.\n" +
            "This cache is only for allies gathering, their online/offline status is updated instantly.");
    }

    public override void Load()
    {
        InitConfig();
        Logger = Log;
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        EventsHandlerSystem.OnInitialize += GameDataOnInitialize;
        EventsHandlerSystem.OnDestroy += GameDataOnDestroy;

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }

    public override bool Unload()
    {
        EventsHandlerSystem.OnInitialize -= GameDataOnInitialize;
        EventsHandlerSystem.OnDestroy -= GameDataOnDestroy;

        Config.Clear();
        harmony?.UnpatchSelf();
        return true;
    }

    public void OnGameInitialized()
    {
        if (!VWorld.IsServer) return;
    }

    private static void GameDataOnInitialize(World world)
    {
        SystemsCore = Core.SystemsCore;

        Helper.GetServerGameManager(out Helper.SGM);

        isInitialized = true;
    }

    private static void GameDataOnDestroy()
    { 
    
    }
}
