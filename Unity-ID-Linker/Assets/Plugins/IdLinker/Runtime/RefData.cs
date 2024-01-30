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
		/*[HideInInspector]*/ public string AssetPath;
		public string PropertyPath;
		public Object Asset;

		private static bool UseGuids => IdTable.Instance.UseGuids;

		public RefData(string assetPath, Object asset)
		{
			AssetPath = assetPath;
			Asset = asset;
			PropertyPath = null;
		}

		public static RefData FromAsset([NotNull] Object asset)
		{
			string assetPath = AssetDatabase.GetAssetPath(asset);
			if (UseGuids)
				assetPath = AssetDatabase.AssetPathToGUID(assetPath);

			return new RefData(assetPath, asset);
		}

		public static RefData FromAssetPath([NotNull] string assetPath)
		{
			if (UseGuids)
				assetPath = AssetDatabase.AssetPathToGUID(assetPath);

			return new RefData(assetPath, AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath));
		}

		public static RefData FromGuid([NotNull] string guid)
		{
			if (!UseGuids)
				guid = AssetDatabase.GUIDToAssetPath(guid);

			return new RefData(guid, AssetDatabase.LoadAssetAtPath<ScriptableObject>(guid));
		}

		public override bool Equals(object obj) => obj is RefData other && Equals(other);
		public bool Equals(RefData other) => string.Equals(AssetPath, other.AssetPath);
	}
}