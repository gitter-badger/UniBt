using UnityEditor;

namespace UniBt.Editor
{
    [CustomEditor(typeof(Node), true)]
    public class NodeInspectorDrawer : UnityEditor.Editor
    {
        protected Node node;

        public virtual void OnEnable()
        {
            node = target as Node;
        }

        public virtual void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            string name = node.Name;
            BehaviorTreesEditorUtility.BeginInspectorGUI(ref name);
            if (name != node.Name)
            {
                node.Name = name;
                AssetDatabase.SaveAssets();
            }
            BehaviorTreesEditorUtility.EndInspectorGUI(node);
        }
    }
}
