## NOTICE - THIS IS THE FINAL UPDATE FOR OFFLINE RAID GUARD. THIS FUNCTIONALITY WILL BE PART OF CrimsonRaidGuard WHEN IT RELEASES.

# OfflineRaidGuard
### Server Only Mod
Protect offline players from being sieged.\
When any of the castle owner allies is online, the siege can progress as usual,\
Siege will also progress as usual if the castle is already being sieged/attacked when all the castle owner & allies goes offline.

## Installation
- Install [Bloodstone](https://thunderstore.io/c/v-rising/p/deca/Bloodstone/)
- Install [BloodyCore](https://thunderstore.io/c/v-rising/p/Trodi/BloodyCore/)
- Copy & paste the `OfflineRaidMod.dll` to `\Server\BepInEx\plugins\` folder.

## Config
<details>
<summary>Config</summary>

- `Enable Mod` [default `true`]\
Enable/disable the mod.
- `Factor in Ally Status` [default `true`]\
Include the player allies online status before blocking siege.
- `Max Ally Cache Age` [default `300`]\
Max age of the player allies cache in seconds.\
If the cache age is older than specified, the cache will be renewed.\
Don't set this too short as allies gathering process can slightly impact your server performance.\
This cache is only for allies gathering, their online/offline status is updated instantly.

</details>

## Support

Want to support my V Rising Mod development? 

Donations Accepted
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/skytech6)

Or buy/play my games! 

[Train Your Minibot](https://store.steampowered.com/app/713740/Train_Your_Minibot/) 

[Boring Movies](https://store.steampowered.com/app/1792500/Boring_Movies/)

**This mod was a paid creation. If you are looking to hire someone to make a mod for any Unity game reach out to me on Discord! (skytech6)**

## Credits
This is ported and modified from [CasualSiege](https://github.com/Kaltharos/VRising-CasualSiege)