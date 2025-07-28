using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;
using GorillaShirts.Tools;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_WrongVersion(Stand stand, string installedVersion, string latestVersion, TaskCompletionSource<bool> completionSource) : Menu_StateBase(stand)
    {
        protected string installed = installedVersion;
        protected string latest = latestVersion;
        protected TaskCompletionSource<bool> completionSource = completionSource;

        public override void Enter()
        {
            base.Enter();
            stand.versionMenuRoot.SetActive(true);
            stand.versionDiffText.text = string.Format(stand.versionDiffFormat, installed, latest);
        }

        public override void Exit()
        {
            base.Exit();
            stand.versionMenuRoot.SetActive(false);
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button == EButtonType.GeneralUse1)
            {
                completionSource.SetResult(true);
                return;
            }

            if (button == EButtonType.GeneralUse2)
            {
                string url = @"https://github.com/developer9998/GorillaShirts/releases/latest";
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                    Application.OpenURL(url);
                }
            }
        }
    }
}
