using UnityEngine;
using UnityEngine.UI;

#if PLUGIN
using System;
using GorillaShirts.Models.UI;
#endif

namespace GorillaShirts.Behaviours.Appearance
{
    public class StandCharacterHumanoid : ShirtHumanoid
    {
        public Text characterNameTag;

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

        public void SetAppearence(ECharacterPreference preference)
        {
            Preference = preference;

            switch (preference)
            {
                case ECharacterPreference.Masculine:
                    mascAccessory.enabled = true;
                    femAccessory.enabled = false;
                    characterNameTag.text = mascIdentity;
                    MainSkin.material.color = mascColour;
                    break;
                case ECharacterPreference.Feminine:
                    mascAccessory.enabled = false;
                    femAccessory.enabled = true;
                    characterNameTag.text = femIdentity;
                    MainSkin.material.color = femColour;
                    break;
            }
        }

        public override void MoveNameTag()
        {
            MoveNameTagTransform(characterNameTag.transform, NameTagOffset);
        }
#endif
    }
}
