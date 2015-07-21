using UnityEngine;
using UniRx;

namespace UniBt
{
    public partial class Brain : MonoBehaviour
    {
        private RuntimeTask Initialize_Wait(Task t)
        {
            RuntimeTask rt = null;
            rt = new RuntimeTask(t, "Wait_UniRx");
            rt.taskFunc = Wait;
            return rt;
        }

        private System.IDisposable Wait()
        {
            Wait w = _aliveBehavior as Wait;
            var ss = Observable.Timer(System.TimeSpan.FromSeconds(w.tick))
                .Subscribe(_ => FinishExecute(true));
            return ss;
        }
    }
}
