using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UniBt
{
	[System.Serializable]
	public class BehaviorTrees : Node
	{
		public Node[] nodes
		{
			get
			{
				if (this._nodes == null)
					this._nodes = new Node[0];
				
				return this._nodes;
			}
			set
			{
				this._nodes = value;
			}
		}
		
		public Node[] NodeRecursive
		{
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
			get
			{
				List<Node> nodeList = new List<Node>();
				if (nodes.Length > 0)
					nodeList.AddRange(nodes);
				
				foreach(BehaviorTrees brain in behaviorTrees)
				{
					nodeList.AddRange(brain.NodeRecursive);
				}
				return _nodes.ToArray();
			}
		}
		
		public BehaviorTrees[] behaviorTrees
		{
            // This code is borrowed from ICode(https://www.assetstore.unity3d.com/en/#!/content/13761)
			get
			{
				return this._nodes.Where(node => node.GetType() == typeof(BehaviorTrees)).Cast<BehaviorTrees>().ToArray();
			}
		}
		
		public Root rootNode
		{
			get { return _rootNode; }
			set { this._rootNode = value; }
		}
		
		[SerializeField]
		private Node[] _nodes;
		[SerializeField]
		private Root _rootNode;
	}
}
