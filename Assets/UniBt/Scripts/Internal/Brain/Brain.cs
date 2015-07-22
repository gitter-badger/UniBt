using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace UniBt
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Unity Behavior Tree/Behavior Tree Brain")]
    public partial class Brain : MonoBehaviour
    {
        public BehaviorTree behaviorTree;

        public Node aliveBehavior
        {
            get { return this._aliveBehavior; }
        }

        private Dictionary<Node, int> _currentChildNodeIndex = new Dictionary<Node, int>();
        private Node _aliveBehavior = null;
        private bool _isChecked = false;

        private void Awake()
        {
            if (CheckBehaviorBrain())
                InitializeNodes();
        }

        private void OnEnable()
        {
            StartNode(behaviorTree.rootNode);
        }

        private void OnDisable()
        {
            _aliveBehavior = null;
        }

        private bool CheckBehaviorBrain()
        {
            if (behaviorTree == null)
            {
                Debug.LogError("Can't initialize to BTBehaviour, because the behavior brain is null.");
                return false;
            }
            return true;
        }

        private void InitializeNodes()
        {
            for (int i = 0; i < behaviorTree.nodes.Length; i++)
            {
                Node node = behaviorTree.nodes[i] as Node;
                if (!(node is Task) && !_currentChildNodeIndex.ContainsKey(node))
                    _currentChildNodeIndex.Add(node, 0);

                InitializeDecorator(node);
                if (node is Composite)
                    InitializeService(node as Composite);
                else if (node is Task)
                    InitializeTask(node as Task);
            }
        }

        private UnityEngine.Object GetEqualTypeComponent(Type scriptType)
        {
            UnityEngine.Object value = null;
            MonoBehaviour[] comps = gameObject.GetComponents<MonoBehaviour>();
            for (int k = 0; k < comps.Length; k++)
            {
                if (comps[k].GetType() == scriptType)
                {
                    value = comps[k];
                    break;
                }
            }
            return value;
        }

        public void StartNode(Node node)
        {
            _aliveBehavior = node;
            if (node.decorators.Length > 0)
            {
                for (int i = 0; i < node.decorators.Length; i++)
                {
                    RuntimeDecorator rd = GetRuntimeDecorator(node.decorators[i]);
#if UNITY_EDITOR
                    rd.closed = false;
#endif
                    if (!StartDecorator(rd))
                    {
                        FinishExecute(false);
                        return;
                    }
                }
            }

            if (node is Task)
            {
                StartTask(node);
            }
            else
            {
                if (node is Composite)
                {
                    if ((node as Composite).services.Length > 0)
                    {
                        for (int i = 0; i < _runtimeServices.Count; i++)
                            if (_runtimeServices[i].parent == node as Composite)
                                StartService(_runtimeServices[i]);
                    }
                    if (node.childNodes.Length == 0)
                    {
                        if (node is Selector)
                            FinishExecute(true);
                        else if (node is Sequence)
                            FinishExecute(false);
                    }
                }

                if (_currentChildNodeIndex[node] < node.childNodes.Length)
                    StartNode(node.childNodes[_currentChildNodeIndex[node]]);
            }
        }

        public void FinishExecute(bool value)
        {
            if (_aliveBehavior == null) return;
            FinishExecuteImmediate();
            if (_aliveBehavior is Selector)
            {
                if (value)
                    FinishExecute(true);
                else
                {
                    _currentChildNodeIndex[_aliveBehavior] += 1;
                    if (_currentChildNodeIndex[_aliveBehavior] >= _aliveBehavior.childNodes.Length)
                        FinishExecute(false);
                    else
                        StartNode(_aliveBehavior);
                }
            }
            else if (_aliveBehavior is Sequence)
            {
                if (value)
                {
                    _currentChildNodeIndex[_aliveBehavior] += 1;
                    if (_currentChildNodeIndex[_aliveBehavior] >= _aliveBehavior.childNodes.Length)
                        FinishExecute(true);
                    else
                        StartNode(_aliveBehavior);
                }
                else
                    FinishExecute(false);
            }
            else if (_aliveBehavior is Root)
            {
                if (_isChecked)
                {
                    _isChecked = false;
                    Observable.FromCoroutine(WaitOneFrame)
                        .TakeUntilDestroy(this)
                        .TakeUntilDestroy(this)
                        .First()
                        .Subscribe(_ => StartNode(behaviorTree.rootNode));
                }
                else
                {
                    _isChecked = true;
                    StartNode(_aliveBehavior);
                }
            }
        }

        private void FinishExecuteImmediate()
        {
            FinishDecorators();
            if (_aliveBehavior is Composite)
                FinishComposite(_aliveBehavior as Composite);
            FinishTask();
            _aliveBehavior = _aliveBehavior.parentNode;
        }

        private void FinishComposite(Composite composite)
        {
            _currentChildNodeIndex[composite as Node] = 0;
            FinishServices(composite);
        }

        private IEnumerator WaitOneFrame()
        {
            yield return null;
        }
    }
}
