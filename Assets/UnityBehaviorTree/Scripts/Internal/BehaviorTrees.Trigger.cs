using UnityEngine;

namespace UBT.Trigger
{
    public static class BehaviorTreesTriggerExtensions
    {
        public static void FinishExecute(this Component component, bool value)
        {
            Brain b = component.GetComponent<Brain>();
            if (component != null && component.gameObject != null && b != null)
                b.FinishExecute(value);
            else
                Debug.LogError("No brain");
        }
    }
}
