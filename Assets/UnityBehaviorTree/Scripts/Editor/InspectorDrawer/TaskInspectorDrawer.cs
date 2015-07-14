using UnityEngine;
using UnityEditor;

namespace UniBt.Editor
{
    [CustomEditor(typeof(Task))]
    public class TaskInspectorDrawer : NodeInspectorDrawer
    {
        private Task task;

        public override void OnEnable()
        {
            base.OnEnable();
            task = target as Task;
        }

        public override void OnInspectorGUI()
        {
            string name = task.Name;
            BehaviorTreesEditorUtility.BeginInspectorGUI(ref name);
            if (name != task.Name)
            {
                task.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Target Code", false))
            {
                BehaviorTreesEditorUtility.DrawTargetScript(OnSelected, serializedObject);
                if (task.targetScript != null && BehaviorTreesEditorUtility.DrawTargetMethod(task.targetScript.GetType(), typeof(System.IDisposable), ref task.targetMethod))
                {
                    UpdateName();
                    UpdateComment();
                    BehaviorTreesEditor.RepaintAll();
                    AssetDatabase.SaveAssets();
                }
            }
            BehaviorTreesEditorUtility.EndInspectorGUI(node);
        }

        private void OnSelected(Object obj)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("targetScript");
            sp.objectReferenceValue = obj;
            serializedObject.ApplyModifiedProperties();
            task.targetScript = obj as MonoBehaviour;
        }

        private void UpdateName()
        {
            string name = "Task";
            if (task.targetScript != null)
                name = string.IsNullOrEmpty(task.targetMethod) ? task.targetScript.name : task.targetMethod;
            task.Name = name;
        }

        private void UpdateComment()
        {
            string comment = "Empty Task";
            if (task.targetScript != null && !string.IsNullOrEmpty(task.targetMethod))
            {
                comment = task.targetScript.name + "." + task.targetMethod;
            }
            task.comment = comment;
        }
    }
}
