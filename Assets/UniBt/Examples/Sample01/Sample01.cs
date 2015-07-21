using UnityEngine;
using System.Collections;

namespace UniBt.Example
{
    public class Sample01 : MonoBehaviour, IInitializable
    {
        public bool optionValue;

        public void Initialize()
        {
            optionValue = true;
        }

        #region Decorators
        public bool OptionValue()
        {
            return optionValue;
        }
        #endregion Decorators

        #region Tasks
        public IEnumerator DebugMessage()
        {
            Debug.Log("[Task] Debug Message");
            this.FinishExecute(true);
            yield break;
        }
        #endregion Tasks
    }
}
