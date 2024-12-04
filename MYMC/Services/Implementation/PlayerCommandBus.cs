using MYMC.Models;
using MYMC.Services.Interface;

namespace MYMC.Services.Implementation;

public class PlayerCommandBus : IPlayerCommandBus
{
    public event EventHandler<PlayerCommandMessage>? PlayerCommandReceived;
    
    public void SendPlayerCommand(PlayerCommandMessage command)
    {
        PlayerCommandReceived?.Invoke(this, command);
    }
}