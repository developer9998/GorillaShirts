using UnityEngine;
using UnityEngine.UI;
using GorillaShirts.Models.Cosmetic;


#if PLUGIN
using System;
using System.Linq;
using GorillaShirts.Models.UI;
#endif

namespace GorillaShirts.Behaviours.Appearance
{
    public class StandCharacterHumanoid : ShirtHumanoid
    {
        public GameObject characterNameTagAnchor;

        public Text characterNameText;

        [Header("Masculine / Steady")]
        public Color mascColour;// = new Color(0.8f, 0.2f, 0.2f);
        public MeshRenderer mascAccessory;
        public string mascIdentity = "STEADY";

        [Header("Feminine / Silly")]
        public Color femColour;// = new Color(1f, 0.5f, 0.9f);
        public MeshRenderer femAccessory;
        public string femIdentity = "SILLY";

#if PLUGIN

        public ECharacterPreference Preference;

        public event Action<ECharacterPreference> OnPreferenceSet;

        public EShirtFallback PreferredFallback => Preference switch
        {
            ECharacterPreference.Masculine => EShirtFallback.SteadyHoodie,
            ECharacterPreference.Feminine => EShirtFallback.SillyCroptop,
            _ => EShirtFallback.None
        };

        public void SetAppearence(ECharacterPreference preference)
        {
            Preference = preference;

            switch (preference)
            {
                case ECharacterPreference.Masculine:
                    mascAccessory.enabled = true;
                    femAccessory.enabled = false;
                    characterNameText.text = mascIdentity;
                    MainSkin.material.color = mascColour;
                    break;
                case ECharacterPreference.Feminine:
                    mascAccessory.enabled = false;
                    femAccessory.enabled = true;
                    characterNameText.text = femIdentity;
                    MainSkin.material.color = femColour;
                    break;
            }

            GameObject[] allShirtObjects = Objects?.SelectMany(selector => selector.Value).ToArray();
            for (int i = 0; i < allShirtObjects.Length; i++)
            {
                if (allShirtObjects[i].TryGetComponent(out ShirtColourProfile colourProfile))
                {
                    colourProfile.SetColour(MainSkin.material.color);
                }
            }

            OnPreferenceSet?.Invoke(preference);
        }

        public void WearSignatureShirt()
        {
            if (Main.Instance.GetShirtFromFallback(PreferredFallback) is IGorillaShirt shirt)
            {
                SetShirt(shirt);
                return;
            }

            ClearShirts();
        }

        public override void MoveNameTag()
        {
            characterNameTagAnchor.transform.parent = NameTagAnchor is not null ? NameTagAnchor.transform : Body;
            characterNameTagAnchor.transform.localPosition = Vector3.zero;
            characterNameTagAnchor.transform.localRotation = Quaternion.identity;

            MoveNameTagTransform(characterNameText.transform, NameTagOffset);
        }
#endif
    }
}
