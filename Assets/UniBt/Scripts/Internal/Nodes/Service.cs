using UnityEngine;

namespace UniBt
{
    public sealed class Service : ScriptableObject
    {
        public string comment;
        public MonoBehaviour targetScript;
        public string targetMethod;
        public float tick;

        public Composite parent
        {
            get { return this._parent; }
            set { this._parent = value; }
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

        [SerializeField]
        private Composite _parent;
        [SerializeField]
        private string _name;
    }
}
