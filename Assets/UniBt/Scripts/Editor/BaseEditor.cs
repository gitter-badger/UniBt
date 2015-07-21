using UnityEngine;
using UnityEditor;

namespace UniBt.Editor
{
    public abstract class BaseEditor : EditorWindow
    {
        public const float MaxCanvasSize = 50000f;

        public static Vector2 center
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            get { return new Vector2(BaseEditor.MaxCanvasSize * 0.5f, BaseEditor.MaxCanvasSize * 0.5f); }
        }

        protected Vector2 _scrollPosition;
        protected Vector2 _mousePosition;
        protected Rect _canvasSize;
        protected Rect _worldViewRect;
        protected Event _currentEvent;
        protected float _debugProgress;

        private const float _GridMinorSize = 12f;
        private const float _GridMajorSize = 120f;

        private Rect _scrollViewRect;
        private Vector2 _offset;
        private Material _material;

        protected virtual void OnEnable()
        {
            this._scrollViewRect = new Rect(0, 0, MaxCanvasSize, MaxCanvasSize);
            this.UpdateScrollPosition(BaseEditor.center);
        }

        protected void UpdateScrollPosition(Vector2 position)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            _offset = _offset + (_scrollPosition - position);
            _scrollPosition = position;

            _worldViewRect = new Rect(this._canvasSize);
            _worldViewRect.x += _scrollPosition.x;
            _worldViewRect.y += _scrollPosition.y;
        }

        protected void Begin()
        {
            _currentEvent = Event.current;
            this._canvasSize = GetCanvasSize();
            if (_currentEvent.type == EventType.Repaint)
            {
                GUIStyle backgroundStyle = "flow background";
                backgroundStyle.Draw(_canvasSize, false, false, false, false);
                DrawGrid();
            }
            Vector2 currentScroll = GUI.BeginScrollView(_canvasSize, _scrollPosition, _scrollViewRect, GUIStyle.none, GUIStyle.none);
            UpdateScrollPosition(currentScroll);
            _mousePosition = _currentEvent.mousePosition;
        }

        protected virtual Rect GetCanvasSize()
        {
            return new Rect(0, 0, position.width, position.height);
        }

        private void DrawGrid()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            GL.PushMatrix();
            GL.Begin(1);
            this.DrawGridLines(_canvasSize, BaseEditor._GridMinorSize, _offset, EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.18f) : new Color(0f, 0f, 0f, 0.1f));
            this.DrawGridLines(_canvasSize, BaseEditor._GridMajorSize, _offset, EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.28f) : new Color(0f, 0f, 0f, 0.15f));
            GL.End();
            GL.PopMatrix();
        }

        private void DrawGridLines(Rect rect, float gridSize, Vector2 offset, Color gridColor)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            GL.Color(gridColor);
            for (float i = rect.x + (offset.x < 0f ? gridSize : 0f) + offset.x % gridSize; i < rect.x + rect.width; i = i + gridSize)
                this.DrawLine(new Vector2(i, rect.y), new Vector2(i, rect.y + rect.height));
            for (float j = rect.y + (offset.y < 0f ? gridSize : 0f) + offset.y % gridSize; j < rect.y + rect.height; j = j + gridSize)
                this.DrawLine(new Vector2(rect.x, j), new Vector2(rect.x + rect.width, j));
        }

        private void DrawLine(Vector2 p1, Vector2 p2)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        protected void End()
        {
            if (!EditorApplication.isPlaying)
                CanvasContextMenu();
            DragCanvas();

            Rect btRect = new Rect(_canvasSize);
            btRect.x += Mathf.Round(_scrollPosition.x);
            btRect.y += Mathf.Round(_scrollPosition.y) + _canvasSize.height - BehaviorTreesEditorStyles.btLabel.CalcSize(new GUIContent("BEHAVIOR TREE")).y * 1.3f;
            GUI.Label(btRect, "BEHAVIOR TREE", BehaviorTreesEditorStyles.btLabel);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Rect simulatingRect = new Rect(_canvasSize);
                simulatingRect.x += Mathf.Round(_scrollPosition.x);
                simulatingRect.y += Mathf.Round(_scrollPosition.y) - BehaviorTreesEditorStyles.btLabel.CalcSize(new GUIContent("SIMULATING")).y * 0.3f;
                GUI.Label(simulatingRect, "SIMULATING", BehaviorTreesEditorStyles.simulatingLabel);
            }

            GUI.EndScrollView();
        }

        protected abstract void CanvasContextMenu();

        private void DragCanvas()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            if (_currentEvent.button != 2) return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (_currentEvent.rawType)
            {
                case EventType.mouseDown:
                    GUIUtility.hotControl = controlID;
                    _currentEvent.Use();
                    break;
                case EventType.mouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        _currentEvent.Use();
                    }
                    break;
                case EventType.mouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        UpdateScrollPosition(_scrollPosition - _currentEvent.delta);
                        _currentEvent.Use();
                    }
                    break;
            }
        }

        protected abstract void OnGUI();

        protected void DrawConnection(Vector3 start, Vector3 end, Color color, int arrows, bool offset, bool on)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            if (_currentEvent.type != EventType.repaint)
                return;
            Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
            if (offset)
            {
                start = start + cross * 6;
                end = end + cross * 6;
            }

            Texture2D tex = (Texture2D)UnityEditor.Graphs.Styles.connectionTexture.image;
            Handles.color = color;
            Handles.DrawAAPolyLine(tex, on ? 12.0f : 5.0f, new Vector3[] { start, end });

            Vector3 vector3 = end - start;
            Vector3 vector31 = vector3.normalized;
            Vector3 vector32 = (vector3 * 0.5f) + start;
            vector32 = vector32 - (cross * 0.5f);
            Vector3 vector33 = vector32 + vector31;

            if (on)
            {
                vector32 = (vector3 * _debugProgress) + start;
                vector32 = vector32 - (cross * _debugProgress);
                vector33 = vector32 + vector31;
            }

            for (int i = 0; i < arrows; i++)
            {
                Vector3 center = vector33 + vector31 * 10.0f * i + vector31 * 5.0f - vector31 * arrows * 5.0f;
                DrawArrow(color, cross, vector31, center, on ? 10.0f : 6.0f);
            }
        }

        private void DrawArrow(Color color, Vector3 cross, Vector3 direction, Vector3 center, float size)
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            Rect rect = new Rect(_worldViewRect);
            rect.y -= _canvasSize.y - size;

            if (!rect.Contains(center))
                return;

            Vector3[] vector3Array = new Vector3[] {
                center + (direction * size),
                (center - (direction * size)) + (cross * size),
                (center - (direction * size)) - (cross * size),
                center + (direction * size)
            };

            Color color1 = color;
            color1.r *= 0.8f;
            color1.g *= 0.8f;
            color1.b *= 0.8f;

            CreateMaterial();
            _material.SetPass(0);

            GL.Begin(GL.TRIANGLES);
            GL.Color(color1);
            GL.Vertex(vector3Array[0]);
            GL.Vertex(vector3Array[1]);
            GL.Vertex(vector3Array[2]);
            GL.End();
        }

        private void CreateMaterial()
        {
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
            if (_material != null)
                return;

            _material = new Material("Shader \"Lines/Colored Blended\" {" +
                                     "SubShader { Pass { " +
                                     "    Blend SrcAlpha OneMinusSrcAlpha " +
                                     "    ZWrite Off Cull Off Fog { Mode Off } " +
                                     "    BindChannels {" +
                                     "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                     "} } }");
            _material.hideFlags = HideFlags.HideAndDontSave;
            _material.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
