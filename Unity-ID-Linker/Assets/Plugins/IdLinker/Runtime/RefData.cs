using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Derrixx.IdLinker
{
	[System.Serializable]
	public struct RefData
	{
		[HideInInspector] public string Path;
		public Object Asset;

		public RefData(string path, Object asset)
		{
			Path = path;
			Asset = asset;
		}

		public static RefData FromAsset([NotNull] Object asset) => new(AssetDatabase.GetAssetPath(asset), asset);
		public static RefData FromPath([NotNull] string path) => new(path, AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
	
		public override bool Equals(object obj) => obj is RefData other && Equals(other);
		public bool Equals(RefData other) => string.Equals(Path, other.Path);
	}
}