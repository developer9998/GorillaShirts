using GorillaShirts.Behaviours.Appearance;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace GorillaShirts.Behaviours.UI
{
    public class Stand : MonoBehaviour
    {
        public GameObject Root;

        public AudioSource AudioDevice;

        public Camera Camera;

        public StandCharacterHumanoid Character;

        [Header("Welcome Menu")]

        public GameObject welcomeMenuRoot;

        public TMP_Text tipText;

        [Header("Shirt Process/Load Menu")]

        public GameObject loadMenuRoot;

        public Image loadRadial;

        public Text loadPercent;

        [Header("Version Notice Screen")]

        public GameObject versionMenuRoot;

        public TMP_Text versionDiffText;

        [TextArea(2, 4)]
        public string versionDiffFormat;

        [Header("Main Menu")]

        public GameObject mainMenuRoot;

        public GameObject navigationRoot;

        public GameObject mainContentRoot;

        public GameObject infoContentRoot;

        public TMP_Text navigationText;

        public TMP_Text headerText;

        [TextArea(2, 8)]
        public string headerFormat;

        public Text playerInfoText;

        [TextArea(4, 6), FormerlySerializedAs("personalDataFormat")]
        public string playerInfoFormat;

        public TMP_Text descriptionText;

        public GameObject[] featureObjects;

        public TMP_Text shirtStatusText;

        public GameObject rigButtonObject;

        public GameObject sillyHeadObject, steadyHeadObject;

        public GameObject captureButtonObject;

        public GameObject shuffleButtonObject;

        public GameObject tagOffsetControlObject;

        public Text tagOffsetText;
    }
}
