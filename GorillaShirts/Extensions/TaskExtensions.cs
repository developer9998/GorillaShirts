using BepInEx;
using GorillaShirts.Behaviours;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class TaskExtensions
    {
        private static MonoBehaviour MonoBehaviour => Main.HasInstance ? Main.Instance : ThreadingHelper.Instance;

        public static async Task YieldAsync(this YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            MonoBehaviour.StartCoroutine(YieldRoutine(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator YieldRoutine(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
        {
            yield return instruction;
            completionSource.SetResult(instruction);
            yield break;
        }

        /*
        public static async Task YieldAsync(UnityWebRequest webRequest)
        {

        }
        */
    }
}
