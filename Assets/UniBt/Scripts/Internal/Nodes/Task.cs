using UnityEngine;

namespace UniBt
{
    public class Task : Node
    {
        public MonoBehaviour targetScript;
        public string targetMethod;
        public bool isCoroutine;

        public Task()
        {
            _hasTopSelector = true;
            _hasBotSelector = false;
        }
    }
}
