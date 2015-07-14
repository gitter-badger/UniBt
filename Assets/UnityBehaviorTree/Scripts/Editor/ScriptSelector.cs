using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UniBt.Editor
{
    public class ScriptSelector : ScriptableWizard
    {
        public delegate void OnSelectionCallback(Object behaviour);

        private string _title;
        private System.Type _type;
        private OnSelectionCallback _callback;
        private Object[] _objects = null;
        private bool _isSearched = false;
        private Vector2 _scroll = Vector2.zero;

        public static SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property, bool padding, params GUILayoutOption[] options)
        {
            // This code is borrowed from NGUI(https://www.assetstore.unity3d.com/en/#!/content/2413)
            SerializedProperty sp = serializedObject.FindProperty(property);
            if (sp != null)
            {
                if (padding)
                    EditorGUILayout.BeginHorizontal();
                if (label != null)
                    EditorGUILayout.PropertyField(sp, new GUIContent(label), options);
                else
                    EditorGUILayout.PropertyField(sp, options);
                if (padding)
                    EditorGUILayout.EndHorizontal();
            }
            return sp;
        }

        public static void Show<T>(OnSelectionCallback callback) where T : Object
        {
            string title = "Select a target script";
            ScriptSelector selector = ScriptableWizard.DisplayWizard<ScriptSelector>(title);
            selector._title = title;
            selector._type = typeof(T);
            selector._callback = callback;
            selector._objects = Resources.FindObjectsOfTypeAll(typeof(T));
            List<Object> objectList = new List<Object>();
            for (int i = 0; i < selector._objects.Length; i++)
            {
                Object obj = selector._objects[i];
                if (obj.GetType().GetInterface("IInitializable") != null)
                    objectList.Add(obj);
            }
            selector._objects = objectList.ToArray();

            if (selector._objects == null || selector._objects.Length == 0)
                selector.Search();
            else
            {
                System.Array.Sort(selector._objects, delegate (Object a, Object b)
                {
                    if (a == null) return (b == null) ? 0 : 1;
                    if (b == null) return -1;
                    return a.name.CompareTo(b.name);
                });
            }
        }

        private void Search()
        {
            // This code is borrowed from NGUI(https://www.assetstore.unity3d.com/en/#!/content/2413)
            _isSearched = true;
            string[] paths = AssetDatabase.GetAllAssetPaths();
            List<Object> list = new List<Object>();
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                bool valid = false;
                if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    valid = true;
                }
                if (!valid) continue;

                EditorUtility.DisplayProgressBar("Loading", "Searching scripts, please wait...", (float)i / paths.Length);
                Object obj = AssetDatabase.LoadMainAssetAtPath(path);
                if (obj == null) continue;

                if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab)
                {
                    Object t = (obj as GameObject).GetComponent(_type);
                    if (t != null && !list.Contains(t))
                    {
                        if (t.GetType().GetInterface("IInitializable") != null)
                            list.Add(t);
                    }
                }
            }
            list.Sort(delegate (Object a, Object b) { return a.name.CompareTo(b.name); });
            _objects = list.ToArray();
            EditorUtility.ClearProgressBar();
        }

        private void OnGUI()
        {
            EditorGUIUtility.labelWidth = 80f;
            GUILayout.Label(_title, "LODLevelNotifyText");
            GUILayout.Space(6f);

            if (_objects == null || _objects.Length == 0)
            {
                EditorGUILayout.HelpBox("No scripts found.\nTry creating a new one.", MessageType.Info);
                bool isDone = false;

                EditorGUILayout.Space();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (_type == typeof(IInitializable))
                {
                    if (GUILayout.Button("Open the Code Pack Wizard", GUILayout.Width(150f)))
                    {
                        Menu.OpenWizardWindow();
                        isDone = true;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                if (isDone) Close();
            }
            else
            {
                Object selected = null;
                _scroll = GUILayout.BeginScrollView(_scroll);

                foreach (Object obj in _objects)
                    if (DrawObject(obj))
                        selected = obj;
                GUILayout.EndScrollView();

                if (selected != null)
                {
                    _callback(selected);
                    Close();
                }
            }

            if (!_isSearched)
            {
                GUILayout.Space(6f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Show All", "LargeButton", GUILayout.Width(120f)))
                    Search();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private bool DrawObject(Object obj)
        {
            // This code is borrowed from NGUI(https://www.assetstore.unity3d.com/en/#!/content/2413)
            if (obj == null)
                return false;

            bool value = false;
            Component comp = obj as Component;

            GUILayout.BeginHorizontal();

            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                path = "[Embedded]";
                GUI.contentColor = new Color(0.7f, 0.7f, 0.7f);
            }
            else if (comp != null && EditorUtility.IsPersistent(comp.gameObject))
            {
                GUI.contentColor = new Color(0.6f, 0.8f, 1f);
            }

            value |= GUILayout.Button(obj.name, "AS TextArea", GUILayout.Width(160f), GUILayout.Height(20f));
            value |= GUILayout.Button(path.Replace("Assets/", ""), "AS TextArea", GUILayout.Height(20f));

            GUI.contentColor = Color.white;
            value |= GUILayout.Button("Select", "ButtonLeft", GUILayout.Width(60f), GUILayout.Height(16f));

            GUILayout.EndHorizontal();
            return value;
        }
    }
}
