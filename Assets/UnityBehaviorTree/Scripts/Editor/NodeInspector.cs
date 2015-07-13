using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;

namespace UniBt.Editor
{
	[CustomEditor(typeof(Node), true)]
	public class NodeInspector : UnityEditor.Editor
	{
		protected Node node;
		
		public virtual void OnEnable()
		{
			node = target as Node;
		}
		
		public virtual void OnDisable()
		{
		}
		
		public override void OnInspectorGUI()
		{
			string name = node.Name;
			BehaviorEditorUtility.BeginInspectorGUI(ref name);
			if (name != node.Name)
			{
				node.Name = name;
				AssetDatabase.SaveAssets();
			}
			BehaviorEditorUtility.EndInspectorGUI(node);
		}
	}
}
