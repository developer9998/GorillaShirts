using GorillaShirts.Behaviours.UI;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Loading(Stand stand) : Menu_StateBase(stand)
    {
        private static readonly string[] messages =
        [
            "GorillaShirts started development in 2022, only to be released later in 2023.",
            "You can wear multiple shirts at the same time under a set of conditions.",
            "Silly and Steady canonically run their shirt store from their stump home.",
            "The shirt stand has locations in most maps featured in Gorilla World.",
            "If you run into any issues with the mod, find or create an issue about it on GitHub.",
            "You can obtain dozens of additional shirts in Dev's modding server: discord.gg/dev9998",
            "Use your mods wisely, and don't bother intentionally bother others.",
            "Access the information screen to learn all about how to use GorillaShirts.",
            "Even when Silly and Steady can be uncooperative and all, don't resort to punching them.",
            "You can see someone else's shirt if you have that same shirt installed.",
            "Snap a clear photo of Silly or Steady using the capture button.",
            "Try to be reasonable with the tag offset controls while they're still avaliable.",
            "Silly and Steady are well accustomed to dressup.",
            "Respect other players choices of shirts, they look good!",
            "GorillaShirts is a progressive mod, and will continue to recieve updates in the future."
        ];

        public override void Enter()
        {
            base.Enter();
            stand.loadMenuRoot.SetActive(true);

            stand.didYouKnowText.text = messages[Random.Range(0, messages.Length)];
        }

        public override void Exit()
        {
            base.Exit();
            stand.loadMenuRoot.SetActive(false);
        }

        public void SetLoadAppearance(int assetsLoaded, int assetCount, int errorCount)
        {
            float loadAmount = Mathf.Approximately(assetsLoaded, 0) ? 0 : Mathf.Clamp01(assetsLoaded / (float)assetCount);
            stand.loadRadial.fillAmount = loadAmount;
            stand.loadPercent.text = $"{Mathf.RoundToInt(loadAmount * 100f)}%";

            if (errorCount != 0 && stand.greenFlag.activeSelf)
            {
                stand.greenFlag.SetActive(false);
                stand.redFlag.SetActive(true);
            }
            stand.flagText.text = errorCount.ToString();
        }
    }
}
