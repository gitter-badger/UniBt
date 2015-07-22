using UnityEngine;

namespace UniBt
{
    public static class BehaviorTreesFinishExecuteExtensions
    {
        /// <summary>
        /// This will transfer to the brain that work of task has been completed.
        /// </summary>
        /// <param name="value">It means the success of task.</param>
        public static void FinishExecute(this Component component, bool value)
        {
            Brain b = component.GetComponent<Brain>();
            if (component != null && component.gameObject != null && b != null)
                b.FinishExecute(value);
        }
    }
}
