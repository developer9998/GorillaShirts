using GorillaShirts.Tools;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GorillaShirts.Behaviours;

internal class ThreadingUtility : MonoBehaviour
{
    private static Action _unityActions;

    public void Update()
    {
        if (_unityActions != null)
        {
            foreach (Action action in _unityActions.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Logging.Error(ex);
                }
            }

            _unityActions = null;
        }
    }

    public static void InvokeUnityMethod(Action action)
    {
        _unityActions = (Action)Delegate.Combine(_unityActions, action);
    }

    public static void InvokeMainMethod(Action action)
    {
        if (!ThreadPool.QueueUserWorkItem(Method)) return;

        void Method(object _)
        {
            action();
        }
    }
}
