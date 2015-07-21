using UnityEditor;

namespace UniBt.Editor
{
    public static class Menu
    {
        [MenuItem("UniBt/Open/Behavior Trees Editor", false, 9)]
        public static void OpenEditorWindow()
        {
            BehaviorTreesEditor.ShowEditorWindow();
        }

        [MenuItem("UniBt/Open/Code Pack Maker", false, 9)]
        [MenuItem("Assets/UniBt/Open Code Pack Maker", false, 0)]
        public static void OpenCodePackMaker()
        {
            EditorWindow.GetWindow<CodePackMaker>(false, "Code Pack Maker", true).Show();
        }
    }
}
