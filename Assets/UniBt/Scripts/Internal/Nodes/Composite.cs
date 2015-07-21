using UnityEngine;

namespace UniBt
{
    public class Composite : Node
    {
        public Service[] services
        {
            get
            {
                if (this._services == null)
                    this._services = new Service[0];
                return this._services;
            }
            set
            {
                this._services = value;
            }
        }

        [SerializeField]
        private Service[] _services;

        public Composite()
        {
            _hasTopSelector = true;
            _hasBotSelector = true;
        }
    }
}
