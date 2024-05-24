using Unity.Entities;

namespace OfflineRaidGuard.Models;

public class Player
{
    public ulong PlatformId { get; set; }
    public string? CharacterName { get; set; }

    public bool IsConnected { get; set; }

    public Entity UserEntity { get; set; }

    public Entity CharEntity { get; set; }


    public Player() { }

    public Player(ulong PlatformId, string? CharacterName, bool isConnected, Entity userEntity, Entity charEntity)
    {
        this.PlatformId = PlatformId;
        this.CharacterName = CharacterName;
        this.IsConnected = isConnected;
        this.UserEntity = userEntity;
        this.CharEntity = charEntity;
    }
}
