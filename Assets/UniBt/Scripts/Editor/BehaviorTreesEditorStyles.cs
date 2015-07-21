using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UniBt.Editor
{
    public static class BehaviorTreesEditorStyles
    {
        public const float NodeNormalWidth = 150f;
        public const float NodeNormalHeight = 70f;

        public static GUIStyle canvasBackground;
        public static GUIStyle label;
        public static GUIStyle nodeBoxNameNormalStyle;
        public static GUIStyle nodeBoxCommentStyle;
        public static GUIStyle btLabel;
        public static GUIStyle simulatingLabel;
        public static GUIStyle nodeIndexLabel;
        public static GUIStyle keyTypeLabel;

        private static Dictionary<string, GUIStyle> nodeStyleCache;
        private static string[] styleCache =
        {
            "flow node 0",
            "flow node 1",
            "flow node 2",
            "flow node 3",
            "flow node 4",
            "flow node 5",
            "flow node 6"
        };

        static BehaviorTreesEditorStyles()
        {
            BehaviorTreesEditorStyles.nodeStyleCache = new Dictionary<string, GUIStyle>();

            nodeBoxNameNormalStyle = "TL Selection H2";
            nodeBoxNameNormalStyle.alignment = TextAnchor.UpperLeft;
            nodeBoxNameNormalStyle.fontStyle = FontStyle.Normal;
            nodeBoxNameNormalStyle.richText = true;

            nodeBoxCommentStyle = "ControlLabel";
            nodeBoxCommentStyle.richText = true;

            Color btColor = Color.white;
            btColor.a = 0.3f;
            btLabel = new GUIStyle();
            btLabel.alignment = TextAnchor.UpperRight;
            btLabel.normal.textColor = btColor;
            btLabel.fontSize = 60;

            Color simulatingColor = Color.yellow;
            simulatingColor.a = 0.3f;
            simulatingLabel = new GUIStyle();
            simulatingLabel.alignment = TextAnchor.UpperRight;
            simulatingLabel.normal.textColor = simulatingColor;
            simulatingLabel.fontSize = 60;

            nodeIndexLabel = new GUIStyle();
            nodeIndexLabel.alignment = TextAnchor.MiddleCenter;
            
            keyTypeLabel = new GUIStyle();
            keyTypeLabel.alignment = TextAnchor.MiddleRight;
            keyTypeLabel.normal.textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
        }

        public static GUIStyle GetNodeStyle(int color, bool on)
        {
            return GetNodeStyle(styleCache[color], on, 2f);
        }

        private static GUIStyle GetNodeStyle(string styleName, bool on, float offset)
        {
            string str = on ? string.Concat(styleName, " on") : styleName;
            if (!BehaviorTreesEditorStyles.nodeStyleCache.ContainsKey(str))
            {
                GUIStyle style = new GUIStyle(str);
                style.contentOffset = new Vector2(0, style.contentOffset.y - offset);
                if (on)
                {
                    style.fontStyle = FontStyle.Bold;
                }
                nodeStyleCache[str] = style;
            }
            return nodeStyleCache[str];
        }

        public static GUIStyle GetSelectorStyle(bool on)
        {
            return on ? "MeTransOnLeft" : "MiniToolbarButton";
        }
    }

    public enum NodeColor
    {
        Grey = 0,
        Blue = 1,
        Aqua = 2,
        Green = 3,
        Yellow = 4,
        Orange = 5,
        Red = 6
    }
}
