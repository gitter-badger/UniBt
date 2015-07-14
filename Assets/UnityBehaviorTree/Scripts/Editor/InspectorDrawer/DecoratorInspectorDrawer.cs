using UnityEngine;
using UnityEditor;

namespace UniBt.Editor
{
    [CustomEditor(typeof(Decorator), true)]
    public class DecoratorInspectorDrawer : UnityEditor.Editor
    {
        protected Decorator decorator;

        public virtual void OnEnable()
        {
            decorator = target as Decorator;
        }

        public override void OnInspectorGUI()
        {
            string name = decorator.Name;
            BehaviorTreesEditorUtility.BeginInspectorGUI(ref name);
            if (name != decorator.Name)
            {
                decorator.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Target Code", false))
            {
                BehaviorTreesEditorUtility.DrawTargetScript(OnSelected, serializedObject);
                if (decorator.targetScript != null && BehaviorTreesEditorUtility.DrawTargetMethod(decorator.targetScript.GetType(), typeof(bool), ref decorator.targetMethod))
                {
                    UpdateName();
                    UpdateComment();
                    AssetDatabase.SaveAssets();
                    EditorGUILayout.Space();
                }
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Key Query", false))
            {
                DrawInverseCondition();
            }
            GUILayout.Space(7f);
            if (BehaviorTreesEditorUtility.DrawHeader("Decorator", false))
            {
                DrawTick();
            }
            BehaviorTreesEditorUtility.EndInspectorGUI(decorator);
        }

        private void OnSelected(Object obj)
        {
            serializedObject.Update();
            SerializedProperty sp = serializedObject.FindProperty("targetScript");
            sp.objectReferenceValue = obj;
            serializedObject.ApplyModifiedProperties();
            decorator.targetScript = obj as MonoBehaviour;
        }

        private void DrawTick()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            GUILayout.Label("Tick:");
            float tick = EditorGUILayout.FloatField(decorator.tick);
            if (tick != decorator.tick)
            {
                if (tick < 0)
                {
                    tick = 0;
                }
                decorator.tick = tick;
                UpdateComment();
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawInverseCondition()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            GUILayout.Label("Inverse Condition:");
            bool inversed = EditorGUILayout.Toggle(decorator.inversed);
            if (inversed != decorator.inversed)
            {
                decorator.inversed = inversed;
                UpdateComment();
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateName()
        {
            string name = "Decorator";
            if (decorator.targetScript != null)
                name = string.IsNullOrEmpty(decorator.targetMethod) ? decorator.targetScript.name : decorator.targetMethod;
            decorator.Name = name;
        }

        private void UpdateComment()
        {
            string comment = "";
            if (decorator.inversed)
            {
                comment += "( inversed )\n";
            }
            if (decorator.targetScript != null)
            {
                comment += decorator.targetScript.name;
                if (!string.IsNullOrEmpty(decorator.targetMethod))
                    comment += "." + decorator.targetMethod;
            }
            else
            {
                comment += "Decorator";
            }
            if (decorator.tick > 0)
            {
                comment += ": tick every " + decorator.tick + "s";
            }
            decorator.comment = comment;
            BehaviorTreesEditor.RepaintAll();
        }
    }
}
