using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace UniBt.Editor
{
    public sealed class CodePackMaker : EditorWindow
    {
        private Vector2 _scroll = Vector2.zero;

        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnGUI()
        {
            _scroll = GUILayout.BeginScrollView(_scroll);
            EditorGUIUtility.labelWidth = 80f;

            int selectedCount = GetSelectedScriptsCount();
            if (selectedCount != 1)
            {
                GUILayout.Label("Select one script", "LODLevelNotifyText");
                GUILayout.Space(6f);
                if (selectedCount > 1)
                {
                    EditorGUILayout.HelpBox("Multi-script editing not supported.", MessageType.Warning);
                }
                DrawCreateButton(true);
            }
            else if (selectedCount == 1)
            {
                Object obj = GetSelectedScript();
                if (obj is MonoScript)
                {
                    MonoScript script = obj as MonoScript;
                    System.Type initializable = script.GetClass().GetInterface("IInitializable");
                    if (initializable != null)
                        CreateCodePack(script.GetClass());
                    else
                    {
                        GUILayout.Label("Select one Unity Behavior Tree script", "LODLevelNotifyText");
                        GUILayout.Space(6f);
                        DrawCreateButton(true);
                    }
                }
                else
                    EditorGUILayout.HelpBox("This object is not script.\nTry selecting a Unity Behavior Tree script.", MessageType.Info);
            }
            GUILayout.EndScrollView();
        }

        private void CreateCodePack(System.Type type)
        {
            GUILayout.Label("Selected \"" + type.Name + "\"", "LODLevelNotifyText");
            GUILayout.Space(6f);
            List<MethodInfo> decos = GetMethodInfos(type, typeof(bool));
            List<MethodInfo> servs = GetMethodInfos(type, typeof(void));
            List<MethodInfo> tasks = GetMethodInfos(type, typeof(System.IDisposable));
            tasks.AddRange(GetMethodInfos(type, typeof(IEnumerator)));
            bool created = DrawCreateButton(!(decos.Count > 0) && !(decos.Count > 0) && !(tasks.Count > 0));
            if (decos.Count > 0)
            {
                if (BehaviorTreesEditorUtility.DrawHeader("Decorators", false))
                    DrawMethods(decos);
            }
            if (servs.Count > 0)
            {
                if (BehaviorTreesEditorUtility.DrawHeader("Services", false))
                    DrawMethods(servs);
            }
            if (tasks.Count > 0)
            {
                if (BehaviorTreesEditorUtility.DrawHeader("Tasks", false))
                    DrawMethods(tasks);
            }
            if (created)
            {
                CreatePrefab(type, "Code Pack");
            }
        }

        private void DrawMethods(List<MethodInfo> miList)
        {
            if (miList == null)
                return;
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);
            GUILayout.BeginVertical();

            for (int i = 0; i < miList.Count; i++)
            {
                GUILayout.Space(-1f);
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                GUI.backgroundColor = Color.white;
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(24f));
                GUILayout.Label(miList[i].Name, GUILayout.Height(20f));
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        private bool DrawCreateButton(bool disabled)
        {
            EditorGUILayout.HelpBox("You can create a new prefab by selecting one Unity Behavior Tree script in the Project View window, then clicking \"Create\".", MessageType.Info);
            EditorGUI.BeginDisabledGroup(disabled);
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            bool value = GUILayout.Button("Create");
            GUILayout.Space(20f);
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            return value;
        }

        private int GetSelectedScriptsCount()
        {
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                Object[] objects = EditorUtility.CollectDependencies(Selection.objects);
                return objects.Length;
            }
            return 0;
        }

        private Object GetSelectedScript()
        {
            if (Selection.objects != null && Selection.objects.Length > 0)
            {
                Object[] objects = EditorUtility.CollectDependencies(Selection.objects);
                if (objects.Length == 1)
                    return objects[0];
                else
                    return null;
            }
            return null;
        }

        private List<MethodInfo> GetMethodInfos(System.Type scriptType, System.Type returnType)
        {
            List<MethodInfo> miList = new List<MethodInfo>();

            MethodInfo[] methodInfos = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                if (methodInfo.Name == "Initialize")
                    continue;
                if (methodInfo.ReturnType != returnType)
                    continue;
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length > 0)
                    continue;
                miList.Add(methodInfo);
            }

            return miList;
        }

        private void CreatePrefab(System.Type type, string name)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save As", "New " + name + ".prefab", "prefab", "Save " + name + " prefab as...", "Assets/");
            if (!string.IsNullOrEmpty(path))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

                Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(path);
                string scriptName = path.Replace(".prefab", "");
                scriptName = scriptName.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                go = new GameObject(scriptName);
                go.AddComponent(type);

                PrefabUtility.ReplacePrefab(go, prefab);
                DestroyImmediate(go);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                Selection.activeGameObject = go;
            }
        }
    }
}
