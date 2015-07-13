using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniBt
{
    public static class ArrayUtility
    {
        public static T[] Add<T>(T[] array, T item)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            return (new List<T>(array)
                    {
                item
            }).ToArray();
        }

        public static bool Contains<T>(T[] array, T item)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            return (new List<T>(array).Contains(item));
        }

        public static T[] MoveItem<T>(T[] array, int oldIndex, int newIndex)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            List<T> ts = new List<T>(array);
            T item = ts[oldIndex];
            ts.RemoveAt(oldIndex);
            ts.Insert(newIndex, item);
            return ts.ToArray();
        }

        public static T[] Remove<T>(T[] array, T item)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            List<T> ts = new List<T>(array);
            ts.Remove(item);
            return ts.ToArray();
        }
    }
}
