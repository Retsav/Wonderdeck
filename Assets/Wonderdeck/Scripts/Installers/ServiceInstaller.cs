using UnityEngine;
using Zenject;

public class ServiceInstaller : Installer<ServiceInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<IPlayersService>().To<PlayerService>().AsSingle();
        Container.Bind<IBlackjackService>().To<BlackjackService>().AsSingle();
        Container.Bind<INetworkingService>().To<NetworkingService>().AsSingle();
        Container.Bind<IInventoryService>().To<InventoryService>().AsSingle();
        Container.Bind<IAudioService>().To<AudioService>().AsSingle();
        Container.Bind<IConsequencesService>().To<ConsequencesService>().AsSingle();
        Container.Bind<IHealthService>().To<HealthService>().AsSingle();
        Container.Bind<IPostProcessingService>().To<PostProcessingService>().AsSingle();
        Container.Bind<IEnvironmentService>().To<EnvironmentService>().AsSingle();
        Container.Bind<ISelectModeService>().To<SelectModeService>().AsSingle();
    }
}