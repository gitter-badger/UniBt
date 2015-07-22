using UnityEngine;
using UnityEditor;

namespace UniBt.Editor.Inspector
{
    [CustomEditor(typeof(Wait))]
    public sealed class WaitInspector : TaskInspector
    {
        private Wait wait;

        public override void OnEnable()
        {
            base.OnEnable();
            wait = task as Wait;
        }

        public override void OnInspectorGUI()
        {
            string name = wait.Name;
            BehaviorTreeEditorUtility.BeginInspectorGUI(ref name);
            if (name != wait.Name)
            {
                wait.Name = name;
                AssetDatabase.SaveAssets();
            }
            GUILayout.Space(7f);
            if (BehaviorTreeEditorUtility.DrawHeader("Wait Time", false))
            {
                DrawTick();
            }
            BehaviorTreeEditorUtility.EndInspectorGUI(node);
        }

        private void DrawTick()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            float tick = EditorGUILayout.FloatField("Tick", wait.tick);
            if (tick != wait.tick)
            {
                if (tick <= 0)
                    tick = 0.1f;
                wait.tick = tick;
                UpdateComment();
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();
        }

        private void UpdateComment()
        {
            string comment = "";
            comment += "Wait: ";
            comment += wait.tick + "s";
            wait.comment = comment;
        }
    }
}
