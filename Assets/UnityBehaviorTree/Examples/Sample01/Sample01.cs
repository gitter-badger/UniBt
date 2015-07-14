using UnityEngine;
using System;
using System.Collections;
using UniRx;
using UniBt.Triggers;

namespace UniBt.Example
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
            var subscription = Observable.Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ => this.FinishExecute(true));
            return subscription;
        }

        public IEnumerator DebugMessage()
        {
            Debug.Log("[Task] Debug Message");
            this.FinishExecute(true);
            yield break;
        }
        #endregion Tasks
    }
}
