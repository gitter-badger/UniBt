using UnityEngine;
using System.Collections;

namespace UniBt
{
    public class Task : Node
    {
		public MonoBehaviour targetScript;
		public string targetMethod;
        
        public Task()
        {
            _hasTopSelector = true;
            _hasBotSelector = false;
        }
    }
}
