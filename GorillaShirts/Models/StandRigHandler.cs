using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShirts.Models
{
    public class StandRigHandler : BaseRigHandler
    {
        public Color SteadyColour = new(0.8f, 0.2f, 0.2f);
        public MeshRenderer SteadyHat;

        public Color SillyColour = new(1f, 0.5f, 0.9f);
        public MeshRenderer SillyHat;

        public Text StandNameTag;

#if PLUGIN

        public event Action<Configuration.PreviewGorilla> OnAppearanceChange;

        private IEnumerator cycle_routine;

        public void SetAppearance(bool isSilly)
        {
            SteadyHat.enabled = !isSilly;
            SillyHat.enabled = isSilly;
            MainSkin.material.color = isSilly ? SillyColour : SteadyColour;
            StandNameTag.text = isSilly ? "SILLY" : "STEADY";

            OnAppearanceChange?.Invoke(isSilly ? Configuration.PreviewGorilla.Silly : Configuration.PreviewGorilla.Steady);

            var shirtGameObjectArrayILoveYouSoMuchKaylieColonThree = ShirtGameObjectArray;
            for (int i = 0; i < shirtGameObjectArrayILoveYouSoMuchKaylieColonThree.Length; i++)
            {
                if (shirtGameObjectArrayILoveYouSoMuchKaylieColonThree[i].TryGetComponent(out ShirtVisual component))
                {
                    component.SetColour(MainSkin.material.color);
                }
            }
        }

        public override void MoveNameTag()
        {
            int offset = NameTagOffset;
            MoveNameTagTransform(StandNameTag.transform, offset);
        }

        public IEnumerator Cycle(List<IShirtAsset> shirts)
        {
            List<int> indicies = [.. Enumerable.Range(0, shirts.Count)];

            while (true)
            {
                if (indicies.Count == 0)
                    indicies = [.. Enumerable.Range(0, shirts.Count)];

                int index = indicies[UnityEngine.Random.Range(0, indicies.Count)];
                Shirts = [shirts[index]];

                yield return new WaitForSeconds(1f);
                indicies.Remove(index);
            }
        }

        public void StartCycle(List<IShirtAsset> shirts)
        {
            StopCycle();

            cycle_routine = Cycle(shirts);
            Main.Instance.StartCoroutine(cycle_routine);
        }

        public void StopCycle()
        {
            if (cycle_routine != null)
            {
                Main.Instance.StopCoroutine(cycle_routine);
                cycle_routine = null;
            }
        }

#endif
    }
}