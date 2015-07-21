using UnityEngine;
using UnityEditor;

namespace UniBt.Editor.Inspector
{
    [CustomEditor(typeof(Service), true)]
    public class ServiceInspector : UnityEditor.Editor
    {
        protected Service service;

        public virtual void OnEnable()
        {
            service = target as Service;
        }

        public override void OnInspectorGUI()
        {
            string name = service.Name;
            BehaviorTreesEditorUtility.BeginInspectorGUI(ref name);
            if (name != service.Name)
            {
                service.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Target Code", false))
            {
                BehaviorTreesEditorUtility.DrawTargetScript(OnSelected, serializedObject);
                if (service.targetScript != null && BehaviorTreesEditorUtility.DrawTargetMethod(service.targetScript.GetType(), typeof(void), ref service.targetMethod))
                {
                    UpdateName();
                    UpdateComment();
                    BehaviorTreesEditor.RepaintAll();
                    AssetDatabase.SaveAssets();
                    EditorGUILayout.Space();
                }
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Service", false))
            {
                DrawTick();
            }
            BehaviorTreesEditorUtility.EndInspectorGUI(service);
        }

        private void OnSelected(Object obj)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("targetScript");
            sp.objectReferenceValue = obj;
            serializedObject.ApplyModifiedProperties();
            service.targetScript = obj as MonoBehaviour;
        }

        private void DrawTick()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            float tick = EditorGUILayout.FloatField("Interval", service.tick);
            if (tick != service.tick)
            {
                if (tick <= 0)
                    tick = 0.1f;
                service.tick = tick;
                UpdateComment();
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateName()
        {
            string name = "Service";
            if (service.targetScript != null)
                name = string.IsNullOrEmpty(service.targetMethod) ? service.targetScript.name : service.targetMethod;
            service.Name = name;
        }

        private void UpdateComment()
        {
            string comment = "Empty Service";
            if (service.targetScript != null && !string.IsNullOrEmpty(service.targetMethod))
                comment = service.targetScript.name + "." + service.targetMethod;
            service.comment += ": tick every " + service.tick + "s";
            service.comment = comment;
        }
    }
}
