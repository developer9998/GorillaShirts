using GorillaShirts.Behaviours.UI;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Loading(Stand stand) : Menu_StateBase(stand)
    {
        public override void Enter()
        {
            base.Enter();
            stand.loadMenuRoot.SetActive(true);
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
