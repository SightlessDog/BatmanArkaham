namespace GameCreator.Behavior
{
	using  System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using GameCreator.Core;
	using GameCreator.Variables;

	[Serializable]
	public class Blackboard
	{
		[Serializable]
		public class Item
		{
			public string name = String.Empty;
			public Variable.DataType type = Variable.DataType.Null;

			public Item(string name, Variable.DataType type)
			{
				this.name = name;
				this.type = type;
			}
		}
		
		// PROPERTIES: ----------------------------------------------------------------------------
		
		public List<Item> list = new List<Item>();
	}	
}