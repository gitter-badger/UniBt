using UnityEngine;

namespace UniBt
{
    public class Node : ScriptableObject
    {
        public Rect position;
        public string comment;

        public BehaviorTrees bt
        {
            get { return this._bt; }
            set { this._bt = value; }
        }

        public Node parent
        {
            get { return this._parent; }
        }

        public Node parentNode
        {
            get { return this._parentNode; }
            set { this._parentNode = value; }
        }

        public BehaviorTrees root
        {
            get { return parent == null && GetType() == typeof(BehaviorTrees) ? this as BehaviorTrees : parent.root; }
        }

        public string Name
        {
            get { return this._name; }
            set
            {
                this._name = value;
                base.name = value;
            }
        }

        public bool hasTopSelector
        {
            get { return this._hasTopSelector; }
        }

        public bool hasBotSelector
        {
            get { return this._hasBotSelector; }
        }

        public Node[] childNodes
        {
            get
            {
                if (this._childNodes == null)
                    this._childNodes = new Node[0];
                return this._childNodes;
            }
            set
            {
                this._childNodes = value;
            }
        }

        public Decorator[] decorators
        {
            get
            {
                if (this._decorators == null)
                    this._decorators = new Decorator[0];
                return this._decorators;
            }
            set
            {
                this._decorators = value;
            }
        }

        protected bool _hasTopSelector;
        protected bool _hasBotSelector;

        [SerializeField]
        private Node _parent;
        [SerializeField]
        private Node _parentNode;
        [SerializeField]
        private string _name;
        [SerializeField]
        private BehaviorTrees _bt;
        [SerializeField]
        private Node[] _childNodes;
        [SerializeField]
        private Decorator[] _decorators;
    }
}
