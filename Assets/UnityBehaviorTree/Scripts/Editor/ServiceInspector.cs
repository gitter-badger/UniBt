using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UBT.Editor
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
            BehaviorEditorUtility.BeginInspectorGUI(ref name);
            if (name != service.Name)
            {
                service.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorEditorUtility.DrawHeader("Target Code", false))
            {
                BehaviorEditorUtility.DrawTargetScript(OnSelected, serializedObject);
                if (service.targetScript != null && BehaviorEditorUtility.DrawTargetMethod(service.targetScript.GetType(), typeof(void), ref service.targetMethod))
                {
                    UpdateName();
                    UpdateComment();
                    BehaviorEditor.RepaintAll();
                    AssetDatabase.SaveAssets();
                    EditorGUILayout.Space();
                }
            }
            GUILayout.Space(7f);
            if (BehaviorEditorUtility.DrawHeader("Service", false))
            {
                DrawTick();
            }
            BehaviorEditorUtility.EndInspectorGUI(service);
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
            GUILayout.Label("Tick:");
            float tick = EditorGUILayout.FloatField(service.tick);
            if (tick != service.tick)
            {
                if (tick <= 0)
                {
                    tick = 0.1f;
                }
                service.tick = tick;
                service.comment = service.Name + ": tick every " + service.tick + "s";
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
            string comment = "Empty Task";
            if (service.targetScript != null && !string.IsNullOrEmpty(service.targetMethod))
            {
                comment = service.targetScript.name + "." + service.targetMethod;
            }
            service.comment += ": tick every " + service.tick + "s";
            service.comment = comment;
        }
    }
}
