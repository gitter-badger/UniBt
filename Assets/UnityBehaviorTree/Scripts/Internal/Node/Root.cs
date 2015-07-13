using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniBt
{
    public class Root : Node
    {
        public Root()
        {
            _hasTopSelector = false;
            _hasBotSelector = true;
        }
    }
}
