using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UniBt.Editor
{
    public static class Menu
    {
        [MenuItem("Unity Behavior Tree/Open/Behavior Tree Editor", false, 9)]
        [MenuItem("Assets/Unity Behavior Tree/Open Behavior Tree Editor", false, 0)]
        public static void OpenEditorWindow()
        {
            BehaviorEditor.ShowEditorWindow();
        }

        [MenuItem("Unity Behavior Tree/Open/Code Pack Wizard", false, 9)]
        [MenuItem("Assets/Unity Behavior Tree/Open Code Pack Wizard", false, 0)]
        public static void OpenWizardWindow()
        {
            EditorWindow.GetWindow<WizardWindow>(false, "Wizard Window", true).Show();
        }
    }
}
