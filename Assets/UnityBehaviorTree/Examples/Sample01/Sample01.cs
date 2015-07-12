using UnityEngine;
using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UBT.Trigger;

namespace UBT.Example
{
    public class Sample01 : MonoBehaviour, IInitializable
    {
        public bool optionValue;

        private Subject<Unit> _debugMessage;

        public void Initialize()
        {
            optionValue = false;
        }

        #region Decorators
        public bool AlwaysTrue()
        {
            return true;
        }

        public bool OptionValue()
        {
            return optionValue;
        }
        #endregion

        #region Services
        public void DebugMessageAsService()
        {
            Debug.Log("[Service] Debug Message");
        }
        #endregion Services

        #region Tasks
        public IDisposable Wait()
        {
            this.UpdateAsObservable();
            var stream = Observable.Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => this.FinishExecute(true));
            return stream;
        }

        public IDisposable DebugMessage()
        {
            Debug.Log("[Task] Debug Message");
            this.FinishExecute(true);
            return null;
        }
        #endregion Tasks
    }
}
