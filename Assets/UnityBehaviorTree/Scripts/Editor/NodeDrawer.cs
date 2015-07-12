﻿using UnityEngine;
using UnityEditor;
using System.Collections;

namespace UBT.Editor
{
    public static class NodeDrawer
    {
        private static Texture2D rootIcon;
        private static Texture2D selectorIcon;
        private static Texture2D sequenceIcon;
        private static Texture2D taskIcon;
        private static Texture2D decoratorIcon;
        private static Texture2D serviceIcon;
        private static Texture2D circleIcon;

        static NodeDrawer()
        {
            rootIcon = (Texture2D)Resources.Load("RootIcon", typeof(Texture2D));
            selectorIcon = (Texture2D)Resources.Load("SelectorIcon", typeof(Texture2D));
            sequenceIcon = (Texture2D)Resources.Load("SequenceIcon", typeof(Texture2D));
            taskIcon = (Texture2D)Resources.Load("CustomTaskIcon", typeof(Texture2D));
            decoratorIcon = (Texture2D)Resources.Load("CustomDecoratorIcon", typeof(Texture2D));
            serviceIcon = (Texture2D)Resources.Load("ServiceIcon", typeof(Texture2D));

            circleIcon = EditorGUIUtility.Load("ConsoleCountBadge") as Texture2D;
        }

        public static void DrawNode(Node node, bool selected)
        {
            if (node is Root)
            {
                DrawNode(node, selected, rootIcon, (int)NodeColor.Grey);
            }
            else if (node is Selector)
            {
                DrawNode(node, selected, selectorIcon, (int)NodeColor.Grey);
            }
            else if (node is Sequence)
            {
                DrawNode(node, selected, sequenceIcon, (int)NodeColor.Grey);
            }
            else if (node is Task)
            {
                DrawNode(node, selected, taskIcon, (int)NodeColor.Green);
            }
        }

        private static void DrawNode(Node node, bool selected, Texture2D iconImage, int boxColor)
        {
            float sharedHeight = node.position.yMin + 15;
            DrawDecorators(node, ref sharedHeight);

            Rect insideMainNode = node.position;
            insideMainNode.xMin += 7;
            insideMainNode.xMax -= 7;
            insideMainNode.yMin = sharedHeight;
            insideMainNode.yMax = sharedHeight + 32 + GetCommentHeight(node.comment);
            GUIStyle insideMainNodeStyle = BehaviorEditorStyles.GetNodeStyle(boxColor, false);
            GUI.Box(insideMainNode, "", insideMainNodeStyle);

            int? myIndex = BehaviorEditorUtility.GetMyIndex(node);
            if (myIndex != null)
            {
                float circleIconWidth = circleIcon.width * 1.1f;
                float circleIconHeight = circleIcon.height * 1.1f;
                Rect indexRect = new Rect(insideMainNode.xMax - circleIconWidth / 2f + 12, insideMainNode.yMin - circleIconHeight / 2f, circleIconWidth, circleIconHeight);
                GUI.DrawTexture(indexRect, circleIcon);
                indexRect.xMin -= 2;
                indexRect.yMin -= 2;
                GUI.Label(indexRect, "<color=white>" + (myIndex + 1).ToString() + "</color>", BehaviorEditorStyles.nodeIndexLabel);
            }

            Rect iconRect = node.position;
            iconRect.x = node.position.xMin + 7;
            iconRect.y = sharedHeight - 1;
            GUI.Label(iconRect, iconImage);

            Rect nameRect = node.position;
            nameRect.x = node.position.xMin + 42;
            nameRect.y = sharedHeight + 5;

            GUI.Label(nameRect, "<color=white>" + node.Name + "</color>", BehaviorEditorStyles.nodeBoxNameNormalStyle);

            GUIContent commentContent = new GUIContent(node.comment);
            Rect commentRect = node.position;
            commentRect.x = node.position.xMin + 7;
            commentRect.y = sharedHeight + 30;
            commentRect.width = BehaviorEditorStyles.nodeBoxCommentStyle.CalcSize(commentContent).x + 10;
            GUI.Label(commentRect, "<color=white>" + node.comment + "</color>", BehaviorEditorStyles.nodeBoxCommentStyle);

            sharedHeight += insideMainNode.yMax - insideMainNode.yMin + 5;
            DrawServices(node, sharedHeight);
        }

        private static void DrawDecorators(Node node, ref float height)
        {
            for (int i = 0; i < node.decorators.Length; i++)
            {
                Decorator decorator = node.decorators[i];

                Rect decoratorRect = node.position;
                decoratorRect.xMin += 7;
                decoratorRect.xMax -= 7;
                decoratorRect.yMin = height;
                decoratorRect.yMax = height + 32 + GetCommentHeight(decorator.comment);

                GUIStyle decoratorStyle = BehaviorEditorStyles.GetNodeStyle((int)NodeColor.Blue, BehaviorEditor.CompareSelectedDecorators(decorator));
                if (EditorApplication.isPlaying)
                    decoratorStyle = BehaviorEditorStyles.GetNodeStyle(BehaviorEditor.CheckThisDecoratorClosed(decorator) ? (int)NodeColor.Red : (int)NodeColor.Blue, BehaviorEditor.CompareSelectedDecorators(decorator));
                GUI.Box(decoratorRect, "", decoratorStyle);

                Rect decoratorIconRect = node.position;
                decoratorIconRect.x = node.position.xMin + 7;
                decoratorIconRect.y = height - 1;
                GUI.Label(decoratorIconRect, decoratorIcon);

                Rect decoratorNameRect = node.position;
                decoratorNameRect.x = node.position.xMin + 42;
                decoratorNameRect.y = height + 5;
                GUI.Label(decoratorNameRect, "<color=white>" + decorator.Name + "</color>", BehaviorEditorStyles.nodeBoxNameNormalStyle);

                GUIContent decoratorCommentContent = new GUIContent(decorator.comment);
                Rect decoratorCommentRect = node.position;
                decoratorCommentRect.x = node.position.xMin + 7;
                decoratorCommentRect.y = height + 30;
                decoratorCommentRect.width = BehaviorEditorStyles.nodeBoxCommentStyle.CalcSize(decoratorCommentContent).x + 10;
                GUI.Label(decoratorCommentRect, "<color=white>" + decorator.comment + "</color>", BehaviorEditorStyles.nodeBoxCommentStyle);

                height += decoratorRect.yMax - decoratorRect.yMin + 5;
            }
        }

        private static void DrawServices(Node node, float height)
        {
            if (node is Composite)
            {
                Composite composite = node as Composite;
                for (int i = 0; i < composite.services.Length; i++)
                {
                    Service service = composite.services[i];

                    Rect serviceRect = node.position;
                    serviceRect.xMin += 7 + 13;
                    serviceRect.xMax -= 7;
                    serviceRect.yMin = height;
                    serviceRect.yMax = height + 32 + GetCommentHeight(service.comment);
                    GUIStyle serviceStyle = BehaviorEditorStyles.GetNodeStyle((int)NodeColor.Aqua, BehaviorEditor.CompareSelectedServices(service));
                    GUI.Box(serviceRect, "", serviceStyle);

                    Rect serviceIconRect = node.position;
                    serviceIconRect.x = node.position.xMin + 7 + 13;
                    serviceIconRect.y = height - 1;
                    GUI.Label(serviceIconRect, serviceIcon);

                    Rect serviceNameRect = node.position;
                    serviceNameRect.x = node.position.xMin + 42 + 13;
                    serviceNameRect.y = height + 5;
                    GUI.Label(serviceNameRect, "<color=white>" + service.Name + "</color>", BehaviorEditorStyles.nodeBoxNameNormalStyle);

                    GUIContent serviceCommentContent = new GUIContent(service.comment);
                    Rect serviceCommentRect = node.position;
                    serviceCommentRect.x = node.position.xMin + 7 + 13;
                    serviceCommentRect.y = height + 30;
                    serviceCommentRect.width = BehaviorEditorStyles.nodeBoxCommentStyle.CalcSize(serviceCommentContent).x + 10;
                    GUI.Label(serviceCommentRect, "<color=white>" + service.comment + "</color>", BehaviorEditorStyles.nodeBoxCommentStyle);
                }
            }
        }

        public static float GetMaxWidthContents(Node node)
        {
            float calcWidth = 0f;

            float nameWidth = 53 + GetNameWidth(node.Name);
            float commentWidth = 14 + GetCommentWidth(node.comment);
            calcWidth = nameWidth > commentWidth ? nameWidth : commentWidth;

            for (int i = 0; i < node.decorators.Length; i++)
            {
                Decorator decorator = node.decorators[i];
                float decoratorNameWidth = 53 + GetNameWidth(decorator.Name);
                float decoratorCommentWidth = 14 + GetCommentWidth(decorator.comment);
                float tempCalcWidth = decoratorNameWidth > decoratorCommentWidth ? decoratorNameWidth : decoratorCommentWidth;
                calcWidth = tempCalcWidth > calcWidth ? tempCalcWidth : calcWidth;
            }

            if (node is Composite)
            {
                Composite composite = node as Composite;

                for (int i = 0; i < composite.services.Length; i++)
                {
                    Service service = composite.services[i];
                    float serviceNameWidth = 53 + GetNameWidth(service.Name) + 13;
                    float serviceCommentWidth = 14 + 13 + GetCommentWidth(service.comment);
                    float tempCalcWidth = serviceNameWidth > serviceCommentWidth ? serviceNameWidth : serviceCommentWidth;
                    calcWidth = tempCalcWidth > calcWidth ? tempCalcWidth : calcWidth;
                }
            }

            return Mathf.Clamp(calcWidth, 80f, calcWidth);
        }

        private static float GetNameWidth(string name)
        {
            return BehaviorEditorStyles.nodeBoxNameNormalStyle.CalcSize(new GUIContent(name)).x;
        }

        private static float GetCommentWidth(string comment)
        {
            return BehaviorEditorStyles.nodeBoxCommentStyle.CalcSize(new GUIContent(comment)).x;
        }

        public static float GetMaxHeightContents(Node node)
        {
            float calcHeight = 0f;

            calcHeight = 45 + GetCommentHeight(node.comment) + 17;
            for (int i = 0; i < node.decorators.Length; i++)
            {
                Decorator decorator = node.decorators[i];
                calcHeight += GetCommentHeight(decorator.comment) + 32 + 5;
            }

            if (node is Composite)
            {
                Composite composite = node as Composite;
                for (int i = 0; i < composite.services.Length; i++)
                {
                    Service service = composite.services[i];
                    calcHeight += GetCommentHeight(service.comment) + 32 + 5;
                }
            }

            return Mathf.Clamp(calcHeight, 45f, calcHeight);
        }

        public static float GetCommentHeight(string comment)
        {
            GUIContent content = new GUIContent(comment);
            float width = BehaviorEditorStyles.nodeBoxCommentStyle.CalcSize(content).x + 10;
            return BehaviorEditorStyles.nodeBoxCommentStyle.CalcHeight(content, width);
        }
    }
}
