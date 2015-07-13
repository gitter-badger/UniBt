using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace UniBt.Editor
{
    [System.Serializable]
    public class MainToolBar
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
            if (GUILayout.Button(BehaviorEditor.activeGameObject != null ? BehaviorEditor.activeGameObject.name : "[None Selected]", EditorStyles.toolbarDropDown, GUILayout.Width(100)))
            {
                GenericMenu toolsMenu = new GenericMenu();
                List<Brain> brains = BehaviorEditorUtility.FindInScene<Brain>();
                foreach (Brain brain in brains)
                {
                    GameObject gameObject = brain.gameObject;
                    toolsMenu.AddItem(new GUIContent(gameObject.name), false, delegate ()
                    {
                        BehaviorEditor.SelectGameObject(gameObject);
                    });
                }
                toolsMenu.ShowAsContext();
            }
        }

        private void SelectBehaviorBrain()
        {
            GUIContent content = new GUIContent(BehaviorEditor.active != null ? BehaviorEditor.active.name : "[None Selected]");
            float width = EditorStyles.toolbarDropDown.CalcSize(content).x;
            width = Mathf.Clamp(width, 100f, width);
            if (GUILayout.Button(content, EditorStyles.toolbarDropDown, GUILayout.Width(width)))
            {
                GenericMenu menu = new GenericMenu();
                if (BehaviorEditor.active != null)
                    SelectBehaviorBrainMenu(BehaviorEditor.active, ref menu);

                menu.AddItem(new GUIContent("[Craete New]"), false, delegate ()
                {
                    BehaviorTrees bt = AssetCreator.CreateAsset<BehaviorTrees>(true);
                    if (bt != null)
                    {
                        bt.Name = bt.name;

                        Root root = BehaviorEditorUtility.AddNode<Root>(BehaviorEditor.center, bt);
                        bt.rootNode = root;
                        root.Name = "Root";

                        AssetDatabase.SaveAssets();
                        BehaviorEditor.SelectBehaviorTrees(bt);
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
                    BehaviorEditor.SelectBehaviorTrees(bt);
                });
            }
        }
    }
}
