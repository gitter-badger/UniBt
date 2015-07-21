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
            if (GUILayout.Button(BehaviorTreesEditor.activeGameObject != null ? BehaviorTreesEditor.activeGameObject.name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width(100)))
            {
                GenericMenu toolsMenu = new GenericMenu();
                List<Brain> brains = BehaviorTreesEditorUtility.FindInScene<Brain>();
                foreach (Brain brain in brains)
                {
                    GameObject gameObject = brain.gameObject;
                    toolsMenu.AddItem(new GUIContent(gameObject.name), false, delegate ()
                    {
                        BehaviorTreesEditor.SelectGameObject(gameObject);
                    });
                }
                toolsMenu.ShowAsContext();
            }
        }

        private void SelectBehaviorBrain()
        {
            GUIContent content = new GUIContent(BehaviorTreesEditor.active != null ? BehaviorTreesEditor.active.name : "[None Selected]");
            float width = EditorStyles.toolbarDropDown.CalcSize(content).x;
            width = Mathf.Clamp(width, 100f, width);
            if (GUILayout.Button(content, EditorStyles.toolbarDropDown, GUILayout.Width(width)))
            {
                GenericMenu menu = new GenericMenu();
                if (BehaviorTreesEditor.active != null)
                    SelectBehaviorBrainMenu(BehaviorTreesEditor.active, ref menu);

                menu.AddItem(new GUIContent("[Craete New]"), false, delegate ()
                {
                    BehaviorTrees bt = AssetCreator.CreateAsset<BehaviorTrees>(true);
                    if (bt != null)
                    {
                        bt.Name = bt.name;

                        Root root = BehaviorTreesEditorUtility.AddNode<Root>(BehaviorTreesEditor.center, bt);
                        bt.rootNode = root;
                        root.Name = "Root";

                        AssetDatabase.SaveAssets();
                        BehaviorTreesEditor.SelectBehaviorTrees(bt);
                    }
                });
                menu.ShowAsContext();
            }
        }

        private void SelectBehaviorBrainMenu(BehaviorTrees bt, ref GenericMenu menu)
        {
            if (bt != null)
            {
                GUIContent content = new GUIContent(bt.name);
                menu.AddItem(content, false, delegate ()
                {
                    BehaviorTreesEditor.SelectBehaviorTrees(bt);
                });
            }
        }
    }
}
