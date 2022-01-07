namespace GameCreator.Melee
{
	using System.Text;
	using System.Collections;
	using System.Collections.Generic;

	public class TreeCombo<TKey, TValue> : IEnumerable<TreeCombo<TKey, TValue>>
	{
		private readonly Dictionary<TKey, TreeCombo<TKey, TValue>> children = new Dictionary<TKey, TreeCombo<TKey, TValue>>();

		private readonly TKey id;
		private TreeCombo<TKey, TValue> parent;
		private TValue[] data;

		// INITIALIZE: -----------------------------------------------------------------------------------------------------

        public TreeCombo(TKey id)
        {
			this.id = id;
			this.data = new TValue[0];
        }

		public TreeCombo(TKey id, TValue data) : this(id)
		{
			this.data = new TValue[1] { data };
		}

		// PUBLIC METHODS: -------------------------------------------------------------------------------------------------

		public TKey GetID()
		{
			return this.id;
		}

		public TValue[] GetData()
		{
			return this.data;
		}

		public void SetData(TValue value)
		{
			TValue[] newData = new TValue[this.data.Length + 1];
            for (int i = 0; i < this.data.Length; ++i)
            {
				newData[i] = this.data[i];
            }

			newData[newData.Length - 1] = value;
			this.data = newData;
		}

		public TreeCombo<TKey, TValue> GetChild(TKey id)
		{
			return this.children[id];
		}

		public bool HasChild(TKey id)
		{
			return this.children.ContainsKey(id);
		}

		public TreeCombo<TKey, TValue> AddChild(TreeCombo<TKey, TValue> item)
		{
			if (item.parent != null)
			{
				item.parent.children.Remove(item.id);
			}

			item.parent = this;
			this.children.Add(item.id, item);
			return this.GetChild(item.id);
		}

		// STRING METHODS: ---------------------------------------------------------------------------------------------

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			BuildString(sb, this, 0);

			return sb.ToString();
		}

		public static string BuildString(TreeCombo<TKey, TValue> tree)
		{
			StringBuilder sb = new StringBuilder();

			BuildString(sb, tree, 0);

			return sb.ToString();
		}

		private static void BuildString(StringBuilder sb, TreeCombo<TKey, TValue> node, int depth)
		{
            string id = node.id.ToString();
			sb.AppendLine(id.PadLeft(id.Length + depth));

			foreach (TreeCombo<TKey, TValue> child in node)
			{
				BuildString(sb, child, depth + 1);
			}
		}

		// ENUMERATOR METHODS: -----------------------------------------------------------------------------------------

		public IEnumerator<TreeCombo<TKey, TValue>> GetEnumerator()
		{
			return this.children.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}