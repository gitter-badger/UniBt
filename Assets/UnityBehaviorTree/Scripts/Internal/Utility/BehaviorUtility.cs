﻿using UnityEngine;
using System.Collections;

namespace UBT
{
	public static class BehaviorUtility
	{
		public static bool NodeExists(BehaviorTree brain, string name)
		{
			BehaviorTree root = brain.root;
			if (FindNode(root, name) == null)
			{
				return false;
			}
			return true;
		}
		
		public static Node FindNode(BehaviorTree root, string name)
		{
			if (root.Name == name)
				return root;
			
			Node[] nodes = root.NodeRecursive;
			for (int i = 0; i < (int)nodes.Length; i++)
			{
				Node node = nodes[i];
				if (node.Name == name)
					return node;
			}
			
			return null;
		}
		
		public static int GetNodeCountInAllChildren<T>(Node node)
		{
			int count = 0;
			for (int i = 0; i < node.childNodes.Length; i++)
			{
				if (node.childNodes[i] is T)
				{
					count++;
				}
				if (node.childNodes[i].childNodes.Length > 0)
				{
					count += BehaviorUtility.GetNodeCountInAllChildren<T>(node.childNodes[i]);
				}
			}
			return count;
		}
	}
}
