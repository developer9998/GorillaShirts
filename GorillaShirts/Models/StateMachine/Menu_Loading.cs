using GorillaShirts.Behaviours.UI;
using System;
using UnityEngine;
using Random = System.Random;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Loading(Stand stand) : Menu_StateBase(stand)
    {
        private static readonly string[] messages =
        [
            "GorillaShirts started development back in December 2022, only releasing to the public by September 2023.",
            "You can wear multiple shirts at once, with the exception being for having any that overlap.",
            "Silly and Steady are the stump characters you see stood on the shirt stand.",
            "The shirt stand has locations in most maps featured in Gorilla World.",
            "If you run into any issues with the mod, find or create an issue about it on GitHub.",
            "You can discuss the mod and find shirts in the \"Gorilla Tag Modding Group\" Discord server.",
            "Treat the stump character fairly, like your personal model, and not a punching bag.",
            "You can see someone else's shirt if you have that same shirt installed.",
            "Snap a clear photo of the stump character using the capture button.",
            "Use the tag offset controls in the case of your name tag being obscured by a shirt.",
            "Attachments are provided for most shirts that position components of your player model to fit better.",
            "Be respectful of the style any given player has and their choice of shirts.",
            "If you often use a selection of shirts of varied packs, keep them in your own favourited collection.",
            "Use the releases menu for shirt management including installation, deletion, and updating.",
            "Shirts that provide custom colour features can be manually overridden to differ from your player colour.",
            "GorillaShirts is a mod for everyone, average modders and content creators alike.",
            "You can support the developer of GorillaShirts, dev9998, on either Patreon or Ko-fi."
        ];

        public override void Enter()
        {
            base.Enter();
            Stand.loadMenuRoot.SetActive(true);

            DateTime date = DateTime.UtcNow.Date;
            Random random = new(date.DayOfYear);
            Stand.didYouKnowText.text = messages[random.Next(0, messages.Length)];
        }

        public void SetLoadAppearance(int assetsLoaded, int assetCount, int errorCount)
        {
            float loadAmount = Mathf.Approximately(assetsLoaded, 0) ? 0 : Mathf.Clamp01(assetsLoaded / (float)assetCount);
            Stand.loadSlider.value = loadAmount;
            Stand.loadPercent.text = $"{Mathf.RoundToInt(loadAmount * 100f)}%";

            if (errorCount != 0 && Stand.greenFlag.activeSelf)
            {
                Stand.greenFlag.SetActive(false);
                Stand.redFlag.SetActive(true);
            }
            Stand.flagText.text = errorCount.ToString();
        }

        public override void Exit()
        {
            base.Exit();
            Stand.loadMenuRoot.SetActive(false);
        }
    }
}
