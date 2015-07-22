using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UniBt.Editor
{
    [System.Serializable]
    public sealed class MainToolBar
    {
        public void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            SelectGameObject();
            SelectBehaviorBrain();
            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void SelectGameObject()
        {
            if (GUILayout.Button(BehaviorTreeEditor.activeGameObject != null ? BehaviorTreeEditor.activeGameObject.name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width(100)))
            {
                GenericMenu toolsMenu = new GenericMenu();
                List<Brain> brains = BehaviorTreeEditorUtility.FindInScene<Brain>();
                foreach (Brain brain in brains)
                {
                    GameObject gameObject = brain.gameObject;
                    toolsMenu.AddItem(new GUIContent(gameObject.name), false, delegate ()
                    {
                        BehaviorTreeEditor.SelectGameObject(gameObject);
                    });
                }
                toolsMenu.ShowAsContext();
            }
        }

        private void SelectBehaviorBrain()
        {
            GUIContent content = new GUIContent(BehaviorTreeEditor.active != null ? BehaviorTreeEditor.active.name : "[None Selected]");
            float width = EditorStyles.toolbarDropDown.CalcSize(content).x;
            width = Mathf.Clamp(width, 100f, width);
            if (GUILayout.Button(content, EditorStyles.toolbarDropDown, GUILayout.Width(width)))
            {
                GenericMenu menu = new GenericMenu();
                if (BehaviorTreeEditor.active != null)
                    SelectBehaviorBrainMenu(BehaviorTreeEditor.active, ref menu);

                menu.AddItem(new GUIContent("[Craete New]"), false, delegate ()
                {
                    BehaviorTree bt = AssetCreator.CreateAsset<BehaviorTree>(true);
                    if (bt != null)
                    {
                        bt.Name = bt.name;

                        Root root = BehaviorTreeEditorUtility.AddNode<Root>(BehaviorTreeEditor.center, bt);
                        bt.rootNode = root;
                        root.Name = "Root";

                        AssetDatabase.SaveAssets();
                        BehaviorTreeEditor.SelectBehaviorTrees(bt);
                    }
                });
                menu.ShowAsContext();
            }
        }

        private void SelectBehaviorBrainMenu(BehaviorTree bt, ref GenericMenu menu)
        {
            if (bt != null)
            {
                GUIContent content = new GUIContent(bt.name);
                menu.AddItem(content, false, delegate ()
                {
                    BehaviorTreeEditor.SelectBehaviorTrees(bt);
                });
            }
        }
    }
}
