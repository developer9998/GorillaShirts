using UnityEngine;
using UnityEngine.UI;

#if PLUGIN
using System;
using System.Linq;
using GorillaShirts.Models.UI;
using GorillaShirts.Models.Cosmetic;
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

        public GameObject[] fallbackShirtObjects = new GameObject[6];

        public Transform[] fallbackNameAnchors = new Transform[2];

#if PLUGIN

        public ECharacterPreference Preference;

        public event Action<ECharacterPreference> OnPreferenceSet;

        private Transform fallbackNameAnchor;

        private bool usingSignatureFallback;

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

            if (!usingSignatureFallback)
            {
                string preferenceName = Preference.GetName();

                Array.ForEach(fallbackShirtObjects, gameObject =>
                {
                    bool active = gameObject.name.StartsWith(preferenceName);
                    if (gameObject.activeSelf != active) gameObject.SetActive(active);
                });

                Transform transform = null;
                foreach (Transform anchor in fallbackNameAnchors)
                {
                    if (anchor.transform.name.StartsWith(preferenceName))
                    {
                        transform = anchor;
                        break;
                    }
                }
                if (fallbackNameAnchor != transform)
                {
                    fallbackNameAnchor = transform;
                    MoveNameTag();
                }

                ClearShirts();
                usingSignatureFallback = true;
            }
        }

        public override void OnShirtWorn()
        {
            if (usingSignatureFallback)
            {
                Array.ForEach(Array.FindAll(fallbackShirtObjects, gameObject => gameObject.activeSelf), gameObject => gameObject.SetActive(false));
               
                if (fallbackNameAnchor is not null && characterNameTagAnchor.transform.parent == fallbackNameAnchor)
                {
                    fallbackNameAnchor = null;
                    MoveNameTag();
                }

                usingSignatureFallback = false;
            }
        }

        public override void MoveNameTag()
        {
            characterNameTagAnchor.transform.parent = NameTagAnchor is not null ? NameTagAnchor.transform : (fallbackNameAnchor is not null ? fallbackNameAnchor.transform : Body);
            characterNameTagAnchor.transform.localPosition = Vector3.zero;
            characterNameTagAnchor.transform.localRotation = Quaternion.identity;
            MoveNameTagTransform(characterNameText.transform, NameTagOffset);
        }
#endif
    }
}
