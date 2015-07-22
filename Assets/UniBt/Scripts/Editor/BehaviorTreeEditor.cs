using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UniBt.Editor
{
    public sealed class BehaviorTreeEditor : BaseEditor
    {
        public enum SelectionMode
        {
            None,
            Pick,
            Rect,
        }

        public static BehaviorTreeEditor instance;

        public static BehaviorTree active
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            get
            {
                if (BehaviorTreeEditor.instance == null)
                    return null;
                return BehaviorTreeEditor.instance._active;
            }
        }

        public static GameObject activeGameObject
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            get
            {
                if (BehaviorTreeEditor.instance == null)
                    return null;
                return BehaviorTreeEditor.instance._activeGameObject;
            }
        }

        public static BehaviorTree root
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            get
            {
                if (BehaviorTreeEditor.active == null)
                    return null;
                return BehaviorTreeEditor.active.root;
            }
        }

        public static int SelectionCount
        {
            get
            {
                if (BehaviorTreeEditor.instance != null)
                {
                    return BehaviorTreeEditor.instance._selection.Count;
                }
                return 0;
            }
        }

        private bool _isViewCenter;
        private BehaviorTree _active;
        private GameObject _activeGameObject;
        private Brain _brain;
        private Vector2 _selectionStartPosition;
        private Node _fromNode;
        private bool _isTopSelect;
        private SelectionMode _selectionMode;
        private Rect _shortcutRect;
        private MainToolBar _mainToolBar;
        private List<Node> _selection = new List<Node>();
        private List<Decorator> _decoratorSelection = new List<Decorator>();
        private List<Service> _serviceSelection = new List<Service>();

        private Node[] nodes
        {
            get
            {
                if (BehaviorTreeEditor.active == null)
                    return new Node[0];
                return BehaviorTreeEditor.active.nodes;
            }
        }

        public static BehaviorTreeEditor ShowEditorWindow()
        {
            BehaviorTreeEditor window = EditorWindow.GetWindow<BehaviorTreeEditor>("BT Editor");
            return window;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            BehaviorTreeEditor.instance = this;
            if (_mainToolBar == null)
                _mainToolBar = new MainToolBar();

            _isViewCenter = true;
            EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (_activeGameObject != null)
                {
                    _brain = activeGameObject.GetComponent<Brain>();
                    if (_brain != null && _brain.behaviorTree != null)
                        SelectBehaviorTrees(_brain.behaviorTree);
                }
                _selection.Clear();
                _decoratorSelection.Clear();
                _serviceSelection.Clear();
                UpdateUnitySelection();
            }
            BehaviorTreeEditor.RepaintAll();
        }

        private void Update()
        {
            if (BehaviorTreeEditor.active != null && BehaviorTreeEditor.activeGameObject != null)
            {
                if (EditorApplication.isPlaying)
                {
                    _debugProgress += Time.unscaledDeltaTime * 2.5f;
                    if (_debugProgress >= 1)
                        _debugProgress = 0;
                    BehaviorTreeEditor.RepaintAll();
                }
            }
        }

        protected override Rect GetCanvasSize()
        {
            return new Rect(0, 17f, position.width, position.height - 17f);
        }

        protected override void OnGUI()
        {
            _mainToolBar.OnGUI();

            base.Begin();
            DoNodes();
            base.End();

            if (_isViewCenter)
            {
                CenterView();
                _isViewCenter = false;
            }
        }

        private void DoNodes()
        {
            DoTransitions();
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                DoNode(node);
            }
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                DoNodeEvents();
                DecoratorContextMenu();
                ServiceContextMenu();
                NodeContextMenu();
            }
        }

        private void NodeContextMenu()
        {
            if (_currentEvent.type != EventType.MouseDown || _currentEvent.button != 1 || _currentEvent.clickCount != 1)
                return;

            Node node = MouseOverNode();
            if (node == null)
                return;

            GenericMenu nodeMenu = new GenericMenu();
            if (!(node is Root))
            {
                nodeMenu.AddItem(new GUIContent("Add Decorator/Decorator"), false, delegate ()
                {
                    BehaviorTreeEditorUtility.AddDecorator<Decorator>(node, BehaviorTreeEditor.active);
                });

                if (node is Composite)
                {
                    nodeMenu.AddItem(new GUIContent("Add Service"), false, delegate ()
                    {
                        BehaviorTreeEditorUtility.AddService<Service>((Composite)node, BehaviorTreeEditor.active);
                    });
                }
                else
                {
                    nodeMenu.AddDisabledItem(new GUIContent("Add Service"));
                }
                nodeMenu.AddSeparator("/");
                nodeMenu.AddItem(new GUIContent("Delete Node"), false, delegate ()
                {
                    if (_selection.Contains(node))
                    {
                        foreach (Node mNode in _selection)
                        {
                            if (!(mNode is Root))
                                BehaviorTreeEditorUtility.DeleteNode(mNode, BehaviorTreeEditor.active);
                        }
                        _selection.Clear();
                    }
                    else
                    {
                        BehaviorTreeEditorUtility.DeleteNode(node, BehaviorTreeEditor.active);
                    }
                    UpdateUnitySelection();
                    EditorUtility.SetDirty(BehaviorTreeEditor.active);
                });
            }
            else
            {
                nodeMenu.AddDisabledItem(new GUIContent("Add Decorator"));
                nodeMenu.AddDisabledItem(new GUIContent("Delete Node"));
            }
            nodeMenu.ShowAsContext();
            Event.current.Use();
        }

        private void DecoratorContextMenu()
        {
            if (_currentEvent.type != EventType.MouseDown || _currentEvent.button != 1 || _currentEvent.clickCount != 1)
                return;

            Decorator decorator = MouseOverDecorator();
            if (decorator == null)
                return;

            int currentIndex = 0;
            for (int i = 0; i < decorator.parent.decorators.Length; i++)
            {
                if (decorator.parent.decorators[i] == decorator)
                    break;
                currentIndex++;
            }

            GenericMenu menu = new GenericMenu();
            if (currentIndex > 0)
            {
                menu.AddItem(new GUIContent("Move Up"), false, delegate ()
                {
                    decorator.parent.decorators = ArrayUtility.MoveItem<Decorator>(decorator.parent.decorators, currentIndex, currentIndex - 1);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move Up"));
            }

            if (currentIndex < decorator.parent.decorators.Length - 1)
            {
                menu.AddItem(new GUIContent("Move Down"), false, delegate ()
                {
                    decorator.parent.decorators = ArrayUtility.MoveItem<Decorator>(decorator.parent.decorators, currentIndex, currentIndex + 1);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move Down"));
            }

            menu.AddItem(new GUIContent("Delete Decorator"), false, delegate ()
            {
                if (_decoratorSelection.Contains(decorator))
                    _decoratorSelection.Clear();
                BehaviorTreeEditorUtility.DeleteDecorator(decorator);
                UpdateUnitySelection();
                EditorUtility.SetDirty(BehaviorTreeEditor.active);
            });
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void ServiceContextMenu()
        {
            if (_currentEvent.type != EventType.MouseDown || _currentEvent.button != 1 || _currentEvent.clickCount != 1)
                return;

            Service service = MouseOverService();
            if (service == null)
                return;

            int currentIndex = 0;
            for (int i = 0; i < service.parent.services.Length; i++)
            {
                if (service.parent.services[i] == service)
                    break;
                currentIndex++;
            }
            GenericMenu menu = new GenericMenu();
            if (currentIndex > 0)
            {
                menu.AddItem(new GUIContent("Move Up"), false, delegate ()
                {
                    service.parent.services = ArrayUtility.MoveItem<Service>(service.parent.services, currentIndex, currentIndex - 1);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move Up"));
            }

            if (currentIndex < service.parent.services.Length - 1)
            {
                menu.AddItem(new GUIContent("Move Down"), false, delegate ()
                {
                    service.parent.services = ArrayUtility.MoveItem<Service>(service.parent.services, currentIndex, currentIndex + 1);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Move Down"));
            }

            menu.AddItem(new GUIContent("Delete Service"), false, delegate ()
            {
                if (_serviceSelection.Contains(service))
                    _serviceSelection.Clear();
                BehaviorTreeEditorUtility.DeleteService(service);
                UpdateUnitySelection();
                EditorUtility.SetDirty(BehaviorTreeEditor.active);
            });
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void DoNode(Node node)
        {
            GUIStyle style = BehaviorTreeEditorStyles.GetNodeStyle((int)NodeColor.Grey, _selection.Contains(node));
            if (EditorApplication.isPlaying && CompareLockedNodes(node))
            {
                style = BehaviorTreeEditorStyles.GetNodeStyle((int)NodeColor.Yellow, _selection.Contains(node));
            }

            node.position.width = NodeDrawer.GetMaxWidthContents(node);
            node.position.height = NodeDrawer.GetMaxHeightContents(node);

            GUI.Box(node.position, "", style);

            NodeDrawer.DrawNode(node, _selection.Contains(node));

            if (node.hasTopSelector)
            {
                Rect rect = node.position;
                rect.x += 20;
                rect.width = rect.width - 40;
                rect.height = 10;

                GUIStyle topSelectorStyle = BehaviorTreeEditorStyles.GetSelectorStyle(false);

                if (GUI.Button(rect, "", topSelectorStyle) && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (_fromNode == null)
                    {
                        _fromNode = node;
                        _isTopSelect = true;
                        if (node.parentNode != null)
                        {
                            node.parentNode.childNodes = ArrayUtility.Remove<Node>(node.parentNode.childNodes, node);
                            node.parentNode = null;
                        }
                    }
                    else
                    {
                        if (!_isTopSelect && !ArrayUtility.Contains(_fromNode.childNodes, node))
                            AddTransition(_fromNode, node);
                        _fromNode = null;
                    }

                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;

                    _selection.Clear();
                    _selection.Add(node);

                    UpdateUnitySelection();
                }
            }

            if (node.hasBotSelector)
            {
                Rect rect = node.position;
                rect.x += 20;
                rect.y += rect.height - 14;
                rect.width = rect.width - 40;
                rect.height = 10;

                GUIStyle botSelectorStyle = BehaviorTreeEditorStyles.GetSelectorStyle(false);

                if (GUI.Button(rect, "", botSelectorStyle) && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (_fromNode == null)
                    {
                        _fromNode = node;
                        _isTopSelect = false;
                    }
                    else
                    {
                        if (_isTopSelect && !ArrayUtility.Contains(node.childNodes, _fromNode))
                            AddTransition(node, _fromNode);
                        _fromNode = null;
                    }

                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;

                    _selection.Clear();
                    _selection.Add(node);

                    UpdateUnitySelection();
                }
            }
        }

        private void DoTransitions()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            if (_fromNode != null)
            {
                DrawConnection(_fromNode.position.center, _mousePosition, Color.green, 1, !_isTopSelect, true);
                Repaint();
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                DoTransition(node);
            }
        }

        private void DoTransition(Node node)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            Node[] childNodes = node.childNodes;
            if (node.childNodes != null)
            {
                for (int i = 0; i < childNodes.Length; i++)
                {
                    Node child = childNodes[i];
                    if (EditorApplication.isPlaying)
                    {
                        if (CompareLockedNodes(child))
                            DrawConnection(node.position.center, child.position.center, Color.cyan, 1, false, true);
                        else
                            DrawConnection(node.position.center, child.position.center, Color.gray, 1, false, false);
                    }
                    else
                    {
                        DrawConnection(node.position.center, child.position.center, Color.white, 1, false, false);
                    }
                }
            }
        }

        private void AddTransition(Node fromNode, Node toNode)
        {
            if (fromNode is Root && !(toNode is Composite))
            {
                return;
            }

            if (fromNode is Root)
            {
                for (int i = 0; i < fromNode.childNodes.Length; i++)
                {
                    fromNode.childNodes[i].parentNode = null;
                }
                fromNode.childNodes = null;
            }

            toNode.parentNode = fromNode;
            fromNode.childNodes = ArrayUtility.Add<Node>(fromNode.childNodes, toNode);
            AssetDatabase.SaveAssets();
        }

        private void OnSelectionChange()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            BehaviorTreeEditor.SelectGameObject(Selection.activeGameObject);
        }

        private void DoNodeEvents()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            if (_currentEvent.button != 0)
                return;
            SelectNodes();
            DragNodes();
        }

        private void SelectNodes()
        {
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (_currentEvent.rawType)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID;
                    _selectionStartPosition = _mousePosition;

                    Decorator decorator = MouseOverDecorator();
                    Service service = MouseOverService();
                    Node node = MouseOverNode();

                    if (decorator != null)
                    {
                        _selection.Clear();
                        _serviceSelection.Clear();

                        if (!_decoratorSelection.Contains(decorator))
                        {
                            _decoratorSelection.Clear();
                            _decoratorSelection.Add(decorator);
                        }

                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UpdateUnitySelection();
                        return;
                    }
                    else if (service != null)
                    {
                        _selection.Clear();
                        _decoratorSelection.Clear();

                        if (!_serviceSelection.Contains(service))
                        {
                            _serviceSelection.Clear();
                            _serviceSelection.Add(service);
                        }

                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UpdateUnitySelection();
                        return;
                    }
                    else if (node != null)
                    {
                        _decoratorSelection.Clear();
                        _serviceSelection.Clear();

                        if (_fromNode != null)
                        {
                            if (_fromNode != node)
                            {
                                if (_isTopSelect)
                                {
                                    if (!ArrayUtility.Contains(node.childNodes, _fromNode) && _fromNode.parentNode != node)
                                        AddTransition(node, _fromNode);
                                }
                                else
                                {
                                    if (!ArrayUtility.Contains(_fromNode.childNodes, node))
                                        AddTransition(_fromNode, node);
                                }
                            }
                            _fromNode = null;
                            _selection.Clear();
                            _selection.Add(node);
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                            return;
                        }

                        if (EditorGUI.actionKey || _currentEvent.shift)
                        {
                            if (!_selection.Contains(node))
                                _selection.Add(node);
                            else
                                _selection.Remove(node);
                        }
                        else if (!_selection.Contains(node))
                        {
                            _selection.Clear();
                            _selection.Add(node);
                        }

                        GUIUtility.hotControl = 0;
                        GUIUtility.keyboardControl = 0;
                        UpdateUnitySelection();
                        return;
                    }

                    _fromNode = null;
                    _selectionMode = SelectionMode.Pick;
                    if (!EditorGUI.actionKey && !_currentEvent.shift)
                    {
                        _selection.Clear();
                        _decoratorSelection.Clear();
                        _serviceSelection.Clear();
                        UpdateUnitySelection();
                    }
                    _currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        _selectionMode = SelectionMode.None;
                        GUIUtility.hotControl = 0;
                        _currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID && !EditorGUI.actionKey && !_currentEvent.shift && (_selectionMode == SelectionMode.Pick || _selectionMode == SelectionMode.Rect))
                    {
                        _selectionMode = SelectionMode.Rect;
                        SelectNodesInRect(FromToRect(_selectionStartPosition, _mousePosition));
                        _currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == controlID && _selectionMode == SelectionMode.Rect)
                    {
                        GUIStyle selectionStyle = "SelectionRect";
                        selectionStyle.Draw(FromToRect(_selectionStartPosition, _mousePosition), false, false, false, false);
                    }
                    break;
            }
        }

        private Rect FromToRect(Vector2 start, Vector2 end)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if (rect.width < 0f)
            {
                rect.x = rect.x + rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0f)
            {
                rect.y = rect.y + rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        private void DragNodes()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (_currentEvent.rawType)
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID;
                    _currentEvent.Use();
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        _currentEvent.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        for (int i = 0; i < _selection.Count; i++)
                        {
                            Node node = _selection[i];
                            node.position.position += _currentEvent.delta;
                        }
                        _currentEvent.Use();
                    }
                    break;
                case EventType.Repaint:
                    if (GUIUtility.hotControl == controlID)
                    {
                        AutoPanNodes(1.5f);
                    }
                    break;
            }
        }

        private void AutoPanNodes(float speed)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            Vector2 delta = Vector2.zero;

            if (_mousePosition.x > _canvasSize.width + _scrollPosition.x - 50f)
            {
                delta.x += speed;
            }

            if ((_mousePosition.x < _scrollPosition.x + 50f) && _scrollPosition.x > 0f)
            {
                delta.x -= speed;
            }

            if (_mousePosition.y > _canvasSize.height + _scrollPosition.y - 50f)
            {
                delta.y += speed;
            }

            if ((_mousePosition.y < _scrollPosition.y + 50f) && _scrollPosition.y > 0f)
            {
                delta.y -= speed;
            }

            if (delta != Vector2.zero)
            {
                for (int i = 0; i < _selection.Count; i++)
                {
                    Node node = _selection[i];
                    node.position.position += delta;
                }
                UpdateScrollPosition(_scrollPosition + delta);
                Repaint();
            }
        }

        private Node MouseOverNode()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                if (node.position.Contains(_mousePosition))
                    return node;
            }
            return null;
        }

        private Decorator MouseOverDecorator()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                float sharedHeight = node.position.yMin + 7;
                float sharedWidth = node.position.width - 14;
                for (int j = 0; j < node.decorators.Length; j++)
                {
                    Decorator decorator = node.decorators[j];

                    Rect decoratorRect = node.position;
                    decoratorRect.width = sharedWidth;
                    decoratorRect.yMin = sharedHeight;
                    decoratorRect.yMax = sharedHeight + 32 + NodeDrawer.GetCommentHeight(decorator.comment);

                    if (decoratorRect.Contains(_mousePosition))
                    {
                        return decorator;
                    }

                    sharedHeight += decoratorRect.yMax - decoratorRect.yMin + 5;
                }
            }
            return null;
        }

        private Service MouseOverService()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                if (node is Composite)
                {
                    Composite composite = node as Composite;

                    float sharedHeight = node.position.yMax - 14;
                    for (int j = composite.services.Length - 1; j >= 0; j--)
                    {
                        Service service = composite.services[j];

                        Rect serviceRect = node.position;
                        serviceRect.xMin += 7 + 13;
                        serviceRect.xMax -= 7;
                        serviceRect.yMin = sharedHeight - (32 + NodeDrawer.GetCommentHeight(service.comment));
                        serviceRect.yMax = sharedHeight;

                        if (serviceRect.Contains(_mousePosition))
                            return service;

                        sharedHeight -= serviceRect.yMax - serviceRect.yMin + 5;
                    }
                }
            }
            return null;
        }

        private void SelectNodesInRect(Rect r)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            for (int i = 0; i < nodes.Length; i++)
            {
                Node node = nodes[i];
                Rect rect = node.position;
                if (rect.xMax < r.x || rect.x > r.xMax || rect.yMax < r.y || rect.y > r.yMax)
                {
                    _selection.Remove(node);
                    continue;
                }
                if (!_selection.Contains(node))
                    _selection.Add(node);
            }
            UpdateUnitySelection();
        }

        private void UpdateUnitySelection()
        {
            if (_decoratorSelection.Count > 0)
                Selection.objects = _decoratorSelection.ToArray();
            else if (_serviceSelection.Count > 0)
                Selection.objects = _serviceSelection.ToArray();
            else
                Selection.objects = _selection.ToArray();
        }

        public void CenterView()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            Vector2 center = Vector2.zero;
            if (nodes.Length > 0)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    Node node = nodes[i];
                    center += new Vector2(node.position.center.x - _canvasSize.width * 0.5f, node.position.center.y - _canvasSize.height * 0.5f);
                }
                center /= nodes.Length;
            }
            else
            {
                center = BaseEditor.center;
            }
            UpdateScrollPosition(center);
            Repaint();
        }

        protected override void CanvasContextMenu()
        {
            if (_currentEvent.type != EventType.MouseDown || _currentEvent.button != 1 || _currentEvent.clickCount != 1 || BehaviorTreeEditor.active == null)
                return;

            GenericMenu canvasMenu = new GenericMenu();
            // Composite
            canvasMenu.AddItem(new GUIContent("Create Composite/Selector"), false, delegate ()
            {
                BehaviorTreeEditorUtility.AddNode<Selector>(_mousePosition, BehaviorTreeEditor.active);
            });
            canvasMenu.AddItem(new GUIContent("Create Composite/Sequence"), false, delegate ()
            {
                BehaviorTreeEditorUtility.AddNode<Sequence>(_mousePosition, BehaviorTreeEditor.active);
            });
            // Task
            canvasMenu.AddItem(new GUIContent("Create Task/Wait"), false, delegate ()
            {
                BehaviorTreeEditorUtility.AddNode<Wait>(_mousePosition, BehaviorTreeEditor.active);
            });
            canvasMenu.AddSeparator("Create Task/");
            canvasMenu.AddItem(new GUIContent("Create Task/Task"), false, delegate ()
            {
                BehaviorTreeEditorUtility.AddNode<Task>(_mousePosition, BehaviorTreeEditor.active);
            });

            canvasMenu.ShowAsContext();
        }

        private bool CompareLockedNodes(Node node)
        {
            if (_brain != null && _brain.behaviorTree != null)
            {
                Node cNode = _brain.aliveBehavior;
                while (cNode != null)
                {
                    if (cNode == node)
                        return true;
                    cNode = cNode.parentNode;
                }
            }
            return false;
        }

        public static void SelectBehaviorTrees(BehaviorTree bt)
        {
            if (BehaviorTreeEditor.instance == null || BehaviorTreeEditor.active == bt)
            {
                BehaviorTreeEditor.instance.CenterView();
                return;
            }
            BehaviorTreeEditor.instance._active = bt;
            BehaviorTreeEditor.instance._selection.Clear();
            BehaviorTreeEditor.instance.UpdateUnitySelection();
            BehaviorTreeEditor.instance.CenterView();
        }

        public static void RepaintAll()
        {
            if (BehaviorTreeEditor.instance != null)
                BehaviorTreeEditor.instance.Repaint();
        }

        public static bool CompareSelectedDecorators(Decorator decorator)
        {
            for (int i = 0; i < BehaviorTreeEditor.instance._decoratorSelection.Count; i++)
            {
                if (BehaviorTreeEditor.instance._decoratorSelection[i] == decorator)
                    return true;
            }
            return false;
        }

        public static bool CompareSelectedServices(Service service)
        {
            for (int i = 0; i < BehaviorTreeEditor.instance._serviceSelection.Count; i++)
            {
                if (BehaviorTreeEditor.instance._serviceSelection[i] == service)
                    return true;
            }
            return false;
        }

        public static void SelectGameObject(GameObject gameObject)
        {
            if (BehaviorTreeEditor.instance == null)
                return;

            if (gameObject != null)
            {
                Brain brain = gameObject.GetComponent<Brain>();
                BehaviorTreeEditor.instance._brain = brain;
                if (brain != null && brain.behaviorTree != null)
                {
                    BehaviorTreeEditor.instance._activeGameObject = gameObject;
                    BehaviorTreeEditor.SelectBehaviorTrees(brain.behaviorTree);
                }
            }
        }

        public static bool CheckThisDecoratorClosed(Decorator decorator)
        {
            if (BehaviorTreeEditor.instance == null)
                return false;

            if (BehaviorTreeEditor.instance._brain != null)
            {
                Brain brain = BehaviorTreeEditor.instance._brain;
                for (int i = 0; i < brain.runtimeDecorators.Count; i++)
                {
                    if (brain.runtimeDecorators[i].decorator == decorator)
                    {
#if UNITY_EDITOR
                        if (brain.runtimeDecorators[i].closed)
                            return true;
                        else
                            break;
#else
                        break;
#endif
                    }
                }
            }
            return false;
        }
    }
}
