using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UBT
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
