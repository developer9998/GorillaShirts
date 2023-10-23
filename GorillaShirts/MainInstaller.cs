using GorillaLocomotion;
using GorillaShirts.Behaviors;
using GorillaShirts.Behaviors.Tools;
using UnityEngine;
using Zenject;

namespace GorillaShirts
{
    public class MainInstaller : Installer
    {
        public GameObject Player = Object.FindObjectOfType<Player>().gameObject;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Main>().FromNewComponentOn(Player).AsSingle();
            Container.BindInterfacesAndSelfTo<Networking>().FromNewComponentOn(Player).AsSingle();

            Container.Bind<AssetLoader>().AsSingle();
            Container.Bind<Configuration>().AsSingle();
            Container.Bind<Installation>().AsSingle();
        }
    }
}
