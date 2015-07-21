using UnityEditor;

namespace UniBt.Editor
{
    public static class Menu
    {
        [MenuItem("Unity Behavior Tree/Open/Behavior Tree Editor", false, 9)]
        public static void OpenEditorWindow()
        {
            BehaviorTreesEditor.ShowEditorWindow();
        }

        [MenuItem("Unity Behavior Tree/Open/Code Pack Maker", false, 9)]
        [MenuItem("Assets/Unity Behavior Tree/Open Code Pack Maker", false, 0)]
        public static void OpenCodePackMaker()
        {
            EditorWindow.GetWindow<CodePackMaker>(false, "Code Pack Maker", true).Show();
        }
    }
}
