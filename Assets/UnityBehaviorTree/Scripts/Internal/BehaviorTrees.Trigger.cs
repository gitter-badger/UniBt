using UnityEngine;

namespace UniBt.Triggers
{
    public static class BehaviorTreesTriggerExtensions
    {
        public static void FinishExecute(this Component component, bool value)
        {
            Brain b = component.GetComponent<Brain>();
            if (component != null && component.gameObject != null && b != null)
                b.FinishExecute(value);
        }
    }
}
