using GorillaLocomotion;
using GorillaShirts.Behaviours;
using GorillaShirts.Tools;
using UnityEngine;
using Zenject;

namespace GorillaShirts
{
    public class MainInstaller : Installer
    {
        private GameObject GSObject;

        public GameObject Player(InjectContext ctx)
        {
            if (GSObject == null)
            {
                GSObject = new GameObject("GorillaShirts Handler");
                GSObject.transform.SetParent(Object.FindObjectOfType<Player>().transform);
            }
            return GSObject;
        }

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Main>().FromNewComponentOn(Player).AsSingle();
            Container.BindInterfacesAndSelfTo<Networking>().FromNewComponentOn(Player).AsSingle();
            Container.BindInterfacesAndSelfTo<Installation>().AsSingle();
            Container.BindInterfacesAndSelfTo<Configuration>().AsSingle();

            Container.Bind<AssetLoader>().AsSingle();
        }
    }
}
