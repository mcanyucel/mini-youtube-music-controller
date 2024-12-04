using MYMC.Models;

namespace MYMC.Services.Interface;

public interface IPlayerCommandBus
{
    event EventHandler<PlayerCommandMessage> PlayerCommandReceived;
    void SendPlayerCommand(PlayerCommandMessage command);
}