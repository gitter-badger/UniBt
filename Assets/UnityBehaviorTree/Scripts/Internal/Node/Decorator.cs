using UnityEngine;
using System;
using System.Collections;

namespace UBT
{
	public class Decorator : ScriptableObject
	{
		public string comment;
		public MonoBehaviour targetScript;
		public string targetMethod;
		public float tick;
		public bool inversed;
		
		public Node parent
		{
			get { return this._parent; }
			set { this._parent = value; }
		}
		
		public string Name
		{
			get { return this._name; }
			set
			{
				this._name = value;
				base.name = value;
			}
		}
		
		[SerializeField]
		private Node _parent;
		[SerializeField]
		private string _name;
	}
}
