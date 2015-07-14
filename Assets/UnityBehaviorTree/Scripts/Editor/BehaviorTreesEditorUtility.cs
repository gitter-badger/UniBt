using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniBt.Editor
{
    public static class BehaviorTreesEditorUtility
    {
        public static string GenerateName<T>()
        {
            return GenerateName(typeof(T));
        }

        public static string GenerateName(Type type)
        {
            string uniqueName = "";

            if (type == typeof(Root))
            {
                uniqueName = "Root";
            }
            else if (type == typeof(Task))
            {
                uniqueName = "Task";
            }
            else if (type == typeof(Selector))
            {
                uniqueName = "Selector";
            }
            else if (type == typeof(Sequence))
            {
                uniqueName = "Sequence";
            }
            else if (type == typeof(Decorator))
            {
                uniqueName = "Decorator";
            }
            else if (type == typeof(Service))
            {
                uniqueName = "Service";
            }

            return uniqueName;
        }

        public static T AddNode<T>(Vector2 position, BehaviorTrees bt)
        {
            if (bt == null)
            {
                Debug.LogWarning("Can't add node to behavior tree, because the behavior trees are null.");
                return default(T);
            }

            Node node = ScriptableObject.CreateInstance(typeof(T)) as Node;
            node.hideFlags = HideFlags.HideInHierarchy;

            node.Name = BehaviorTreesEditorUtility.GenerateName<T>();
            node.comment = node.Name;
            node.bt = bt;
            bt.nodes = ArrayUtility.Add<Node>(bt.nodes, node);
            node.position = new Rect(position.x, position.y, BehaviorTreesEditorStyles.NodeNormalWidth, BehaviorTreesEditorStyles.NodeNormalHeight);

            if (EditorUtility.IsPersistent(bt))
                AssetDatabase.AddObjectToAsset(node, bt);

            if (node.GetType() == typeof(BehaviorTrees))
            {
                node.position.width = 150f;
                node.position.height = 45f;

                Root root = BehaviorTreesEditorUtility.AddNode<Root>(BehaviorTreesEditor.center, node as BehaviorTrees);
                root.Name = "Root";
            }

            AssetDatabase.SaveAssets();
            return (T)(object)node;
        }

        public static void DeleteNode(Node node, BehaviorTrees bt)
        {
            if (node.parentNode != null)
                node.parentNode.childNodes = ArrayUtility.Remove<Node>(node.parentNode.childNodes, node);

            foreach (Node child in node.childNodes)
            {
                child.parentNode = null;
            }

            if (node.decorators.Length > 0)
            {
                foreach (Decorator decorator in node.decorators)
                {
                    BehaviorTreesEditorUtility.DeleteDecorator(decorator);
                }
            }
            if (node is Composite && (node as Composite).services.Length > 0)
            {
                foreach (Service service in (node as Composite).services)
                {
                    BehaviorTreesEditorUtility.DeleteService(service);
                }
            }
            bt.nodes = ArrayUtility.Remove<Node>(bt.nodes, node);
            BehaviorTreesEditorUtility.DestroyImmediate(node);
        }

        public static void DestroyImmediate(ScriptableObject obj)
        {
            if (obj == null)
                return;

            UnityEngine.Object.DestroyImmediate(obj, true);
            AssetDatabase.SaveAssets();
        }

        public static T AddDecorator<T>(Node parent, BehaviorTrees bt)
        {
            if (parent == null)
            {
                Debug.LogWarning("Can't add decorator to behavior tree, because the behavior trees are null.");
                return default(T);
            }

            Decorator decorator = ScriptableObject.CreateInstance(typeof(T)) as Decorator;
            decorator.hideFlags = HideFlags.HideInHierarchy;

            decorator.Name = BehaviorTreesEditorUtility.GenerateName<T>();
            decorator.comment = decorator.Name;
            decorator.parent = parent;
            parent.decorators = ArrayUtility.Add<Decorator>(parent.decorators, decorator);

            if (EditorUtility.IsPersistent(bt))
                AssetDatabase.AddObjectToAsset(decorator, bt);

            AssetDatabase.SaveAssets();
            return (T)(object)decorator;
        }

        public static void DeleteDecorator(Decorator decorator)
        {
            decorator.parent.decorators = ArrayUtility.Remove<Decorator>(decorator.parent.decorators, decorator);
            BehaviorTreesEditorUtility.DestroyImmediate(decorator);
        }

        public static T AddService<T>(Composite parent, BehaviorTrees bt)
        {
            if (parent == null)
            {
                Debug.LogWarning("Can't add service to behavior tree, because the behavior trees are null.");
                return default(T);
            }

            Service service = ScriptableObject.CreateInstance(typeof(T)) as Service;
            service.hideFlags = HideFlags.HideInHierarchy;

            service.Name = BehaviorTreesEditorUtility.GenerateName<T>();
            service.tick = 0.1f;
            service.comment = service.Name + ": tick every 0.1s";
            service.parent = parent;
            parent.services = ArrayUtility.Add<Service>(parent.services, service);

            if (EditorUtility.IsPersistent(bt))
                AssetDatabase.AddObjectToAsset(service, bt);

            AssetDatabase.SaveAssets();
            return (T)(object)service;
        }

        public static void DeleteService(Service service)
        {
            service.parent.services = ArrayUtility.Remove<Service>(service.parent.services, service);
            BehaviorTreesEditorUtility.DestroyImmediate(service);
        }

        public static EventType ReverseEvent(params Rect[] areas)
        {
            EventType eventType = Event.current.type;
            foreach (Rect area in areas)
            {
                if (area.Contains(Event.current.mousePosition) &&
                    (eventType == EventType.MouseDown ||
                    eventType == EventType.ScrollWheel))
                    Event.current.type = EventType.Ignore;
            }
            return eventType;
        }

        public static void ReleaseEvent(EventType type)
        {
            if (Event.current.type != EventType.Used)
                Event.current.type = type;
        }

        public static List<T> FindInScene<T>() where T : Component
        {
            T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
            List<T> list = new List<T>();
            foreach (T comp in comps)
            {
                if (comp.gameObject.hideFlags == 0)
                {
                    string path = AssetDatabase.GetAssetPath(comp.gameObject);
                    if (string.IsNullOrEmpty(path)) list.Add(comp);
                }
            }
            return list;
        }

        public static int? GetMyIndex(Node node)
        {
            if (!(node is Root) && node.parentNode != null)
            {
                if (node.parentNode.childNodes.Length > 1)
                {
                    for (int i = 0; i < node.parentNode.childNodes.Length; i++)
                    {
                        if (node.parentNode.childNodes[i] == node)
                            return i;
                    }
                }
            }
            return null;
        }

        public static void BeginInspectorGUI(ref string name, string style = "IN BigTitle", float width = 50f)
        {
            GUILayout.BeginVertical(style);
            EditorGUIUtility.labelWidth = width;

            if (BehaviorTreesEditorUtility.DrawHeader("Default", false))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(7f);
                name = EditorGUILayout.TextField("Name", name);
                GUILayout.EndHorizontal();
            }
        }

        public static void EndInspectorGUI(UnityEngine.Object obj)
        {
            GUILayout.EndVertical();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(obj);
                BehaviorTreesEditor.RepaintAll();
            }
        }

        public static void DrawTargetScript(ScriptSelector.OnSelectionCallback onSelected, SerializedObject serializedObject)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            if (GUILayout.Button("Target Script", "DropDown", GUILayout.Width(96f)))
                ScriptSelector.Show<MonoBehaviour>(onSelected);
            ScriptSelector.DrawProperty("", serializedObject, "targetScript", false, GUILayout.MinWidth(20f));
            GUILayout.EndHorizontal();
        }

        public static bool DrawTargetMethod(System.Type scriptType, System.Type returnType, ref string currentMethod)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(7f);
            GUILayout.Label("Target Method:");

            List<string> methodList = new List<string>();
            MethodInfo[] methods = scriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            int oldMethodIndex = 0;
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.ReturnType != returnType)
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0)
                    continue;

                if (method.Name == "Initialize")
                    continue;

                methodList.Add(method.Name);
                if (method.Name == currentMethod)
                {
                    oldMethodIndex = methodList.Count - 1;
                }
            }

            if (methodList.Count > 0)
            {
                int methodIndex = EditorGUILayout.Popup(oldMethodIndex, methodList.ToArray());
                if (currentMethod != methodList[methodIndex])
                {
                    currentMethod = methodList[methodIndex];
                    GUILayout.EndHorizontal();
                    return true;
                }
            }
            GUILayout.EndHorizontal();
            return false;
        }

        public static bool DrawHeader(string text, bool detailed)
        {
            // This code is borrowed from NGUI(https://www.assetstore.unity3d.com/en/#!/content/2413)
            return DrawHeader(text, text, detailed, !detailed);
        }

        public static bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            // This code is borrowed from NGUI(https://www.assetstore.unity3d.com/en/#!/content/2413)
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC" + text;
                else text = "\u25BA" + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }
    }
}
