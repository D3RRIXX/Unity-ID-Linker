using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Derrixx.IdLinker
{
	[Serializable]
	public struct RefData
	{
		[HideInInspector] public string AssetPath;
		public string PropertyPath;
		public Object Asset;

		public RefData(string assetPath, Object asset)
		{
			AssetPath = assetPath;
			Asset = asset;
			PropertyPath = null;
		}

		public static RefData FromAsset([NotNull] Object asset) => new(AssetDatabase.GetAssetPath(asset), asset);
		public static RefData FromPath([NotNull] string path) => new(path, AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
	
		public override bool Equals(object obj) => obj is RefData other && Equals(other);
		public bool Equals(RefData other) => string.Equals(AssetPath, other.AssetPath);
	}
}