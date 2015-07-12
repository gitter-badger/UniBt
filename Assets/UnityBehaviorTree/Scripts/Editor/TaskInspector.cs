using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UBT.Editor
{
    [CustomEditor(typeof(Task))]
    public class TaskInspector : NodeInspector
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
			BehaviorEditorUtility.BeginInspectorGUI(ref name);
            if (name != task.Name)
            {
                task.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorEditorUtility.DrawHeader("Target Code", false))
            {
                BehaviorEditorUtility.DrawTargetScript(OnSelected, serializedObject);
                if (task.targetScript != null && BehaviorEditorUtility.DrawTargetMethod(task.targetScript.GetType(), typeof(System.IDisposable), ref task.targetMethod))
                {
                    UpdateName();
                    UpdateComment();
                    BehaviorEditor.RepaintAll();
                    AssetDatabase.SaveAssets();
                }
            }
			BehaviorEditorUtility.EndInspectorGUI(node);
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
