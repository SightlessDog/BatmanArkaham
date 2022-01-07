namespace GameCreator.Behavior
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;

	public static class BehaviorResources
	{
		private const string PATH_LR = "Assets/Plugins/GameCreator/Behavior/Data/Textures/{0}.png";
		private const string PATH_HR = "Assets/Plugins/GameCreator/Behavior/Data/Textures/{0}@2x.png";

		public enum Name
		{
			Grid,
			InputEmpty,
			InputLink,
			OutputEmpty,
			OutputLink,
            Noodle,
            CompositeSelector,
            CompositeSequence,
            CompositeRandomSelector,
            CompositeRandomSequence,
            CompositeParallel,
        }

		public enum Format
		{
			Auto,
			LowRes,
			HighRes
		}
		
		private class TextureData
		{
			public Texture2D textureLR;
			public Texture2D textureHR;

			public TextureData(Texture2D textureLR, Texture2D textureHR = null)
			{
				this.textureLR = textureLR;
				this.textureHR = (textureHR != null ? textureHR : textureLR);
			}
		}
		
		// PROPERTIES: ----------------------------------------------------------------------------

		private static Dictionary<int, TextureData> TEXTURES = new Dictionary<int, TextureData>();
		
		// PUBLIC METHODS: -----------------------------------------------------------------------

		public static Texture2D GetTexture(Name name, Format format)
		{
			TextureData data;
			if (!TEXTURES.TryGetValue((int)name, out data))
			{
				string pathLR = string.Format(PATH_LR, name.ToString());
				string pathHR = string.Format(PATH_HR, name.ToString());
				
				data = new TextureData(
					AssetDatabase.LoadAssetAtPath<Texture2D>(pathLR),
					AssetDatabase.LoadAssetAtPath<Texture2D>(pathHR)
				);
				
				TEXTURES.Add((int)name, data);
			}

			if (data == null) return null;
			if (format == Format.Auto)
			{
				switch (EditorGUIUtility.pixelsPerPoint < 1.5f)
				{
					case true  : format = Format.LowRes;  break;
					case false : format = Format.HighRes; break;
				}
			}
			
			switch (format)
			{
				case Format.LowRes: return data.textureLR;
				case Format.HighRes: return data.textureHR;
			}

			return null;
		}
	}
}