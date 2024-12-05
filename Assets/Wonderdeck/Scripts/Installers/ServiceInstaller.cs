using UnityEngine;
using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<IPlayersService>().To<PlayerService>().AsSingle();
        Container.Bind<IBlackjackService>().To<BlackjackService>().AsSingle();
        Container.Bind<INetworkingService>().To<NetworkingService>().AsSingle();
    }
}