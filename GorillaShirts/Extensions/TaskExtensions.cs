using BepInEx;
using GorillaShirts.Behaviours;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Extensions
{
    public static class TaskExtensions
    {
        private static MonoBehaviour MonoBehaviour => ShirtManager.HasInstance ? ShirtManager.Instance : ThreadingHelper.Instance;

        public static async Task AsAwaitable(this YieldInstruction instruction)
        {
            var completionSource = new TaskCompletionSource<YieldInstruction>();
            MonoBehaviour.StartCoroutine(AsAwaitable(instruction, completionSource));
            await completionSource.Task;
        }

        private static IEnumerator AsAwaitable(YieldInstruction instruction, TaskCompletionSource<YieldInstruction> completionSource)
        {
            yield return instruction;
            completionSource.SetResult(instruction);
            yield break;
        }
    }
}
