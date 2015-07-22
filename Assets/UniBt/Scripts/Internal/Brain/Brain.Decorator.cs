using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

namespace UniBt
{
    public partial class Brain : MonoBehaviour
    {
        public List<RuntimeDecorator> runtimeDecorators
        {
            get { return this._runtimeDecorators; }
        }

        private List<RuntimeDecorator> _runtimeDecorators = new List<RuntimeDecorator>();

        private void InitializeDecorator(Node node)
        {
            if (node.decorators.Length > 0)
            {
                for (int i = 0; i < node.decorators.Length; i++)
                {
                    Decorator dc = node.decorators[i];
                    RuntimeDecorator rd = new RuntimeDecorator(node, dc, dc.tick, dc.inversed);
                    MonoBehaviour comp = GetEqualTypeComponent(dc.targetScript.GetType()) as MonoBehaviour;
                    if (comp == null)
                    {
                        comp = gameObject.AddComponent(dc.targetScript.GetType()) as MonoBehaviour;
                        IInitializable initializable = comp as IInitializable;
                        initializable.Initialize();
                    }
                    rd.decoratorFunc = Delegate.CreateDelegate(typeof(Func<bool>), comp, dc.targetMethod) as Func<bool>;
                    _runtimeDecorators.Add(rd);
                }
            }
        }

        private bool StartDecorator(RuntimeDecorator runtimeDecorator)
        {
            bool value = runtimeDecorator.inversed ? runtimeDecorator.decoratorFunc() : !runtimeDecorator.decoratorFunc();

            if (value)
            {
#if UNITY_EDITOR
                runtimeDecorator.closed = true;
#endif
                return false;
            }
#if UNITY_EDITOR
            runtimeDecorator.closed = false;
#endif
            if (runtimeDecorator.tick > 0f)
            {
                if (runtimeDecorator.subscription != null)
                    runtimeDecorator.subscription.Dispose();

                runtimeDecorator.activeSelf = true;
                runtimeDecorator.subscription = Observable.Interval(TimeSpan.FromSeconds(runtimeDecorator.tick))
                    .Select(_ => runtimeDecorator.activeSelf)
                    .TakeWhile(x => x)
                    .Subscribe(_ =>
                    {
                        if (!runtimeDecorator.activeSelf)
                            return;
                        bool _value = runtimeDecorator.inversed ? runtimeDecorator.decoratorFunc() : !runtimeDecorator.decoratorFunc();
                        if (_value)
                        {
#if UNITY_EDITOR
                            runtimeDecorator.closed = true;
#endif
                            runtimeDecorator.activeSelf = false;
                            runtimeDecorator.subscription = null;
                            while (true)
                            {
                                if (_aliveBehavior != runtimeDecorator.parent)
                                    FinishExecuteImmediate();
                                else if (_aliveBehavior == runtimeDecorator.parent)
                                {
                                    FinishExecute(false);
                                    break;
                                }
                            }
                        }
                    });
            }
            return true;
        }

        private void FinishDecorators()
        {
            if (_aliveBehavior.decorators.Length > 0)
            {
                for (int i = 0; i < _aliveBehavior.decorators.Length; i++)
                {
                    RuntimeDecorator rd = GetRuntimeDecorator(_aliveBehavior.decorators[i]);
                    if (rd.activeSelf)
                        rd.activeSelf = false;
                }
            }
        }

        private RuntimeDecorator GetRuntimeDecorator(Decorator decorator)
        {
            for (int i = 0; i < _runtimeDecorators.Count; i++)
            {
                if (_runtimeDecorators[i].decorator == decorator)
                    return _runtimeDecorators[i];
            }
            return null;
        }
    }
}
