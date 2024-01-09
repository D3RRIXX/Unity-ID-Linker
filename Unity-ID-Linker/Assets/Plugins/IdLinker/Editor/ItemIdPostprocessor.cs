using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Derrixx.IdLinker.Editor
{
	public class ItemIdPostprocessor : AssetPostprocessor
	{
		private readonly struct AssetRefData
		{
			public AssetRefData(ScriptableObject asset, FieldInfo[] fieldsWithAttribute)
			{
				Asset = asset;
				FieldsWithAttribute = fieldsWithAttribute;
			}

			public ScriptableObject Asset { get; }
			public FieldInfo[] FieldsWithAttribute { get; }
		}

		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			var table = IdTable.Instance;

			bool updatedTable = false;

			updatedTable |= ProcessImportedAssets(table, LoadAssetsFromPaths(importedAssets));
			updatedTable |= ProcessDeletedAssets(table, deletedAssets);

			if (updatedTable)
			{
				EditorUtility.SetDirty(table);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		private static bool ProcessImportedAssets(IdTable table, IEnumerable<ScriptableObject> importedAssets)
		{
			int itemsRegistered = 0;
			foreach (var data in ProcessAssets(importedAssets))
			{
				foreach (string val in GetFieldsThatUseIdTable(table, data))
				{
					RefData refData = RefData.FromAsset(data.Asset);
					table.AddReferenceToId(val, refData);
					itemsRegistered++;
				}
			}

			if (itemsRegistered > 0)
				Debug.Log($"Total items registered: {itemsRegistered}");

			return itemsRegistered > 0;
		}

		private static IEnumerable<string> GetFieldsThatUseIdTable(IdTable table, AssetRefData data)
		{
			foreach (FieldInfo field in data.FieldsWithAttribute)
			{
				if (field.GetValue(data.Asset) is string str && table.Ids.Contains(str))
					yield return str;
			}
		}

		private static bool ProcessDeletedAssets(IdTable table, IEnumerable<string> deletedAssets)
		{
			int itemsUnregistered = deletedAssets.Select(RefData.FromPath).Count(AssetWasUnregistered);
			if (itemsUnregistered > 0)
				Debug.Log($"Total objects unregistered: {itemsUnregistered}");

			return itemsUnregistered > 0;

			bool AssetWasUnregistered(RefData refData)
			{
				if (table.RemoveAllReferencesOfObject(refData))
				{
					Debug.Log($"Removed object {refData.Path} from the ref table");
					return true;
				}

				return false;
			}
		}

		private static IEnumerable<ScriptableObject> LoadAssetsFromPaths(IEnumerable<string> paths) =>
			paths.Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>).Where(x => x is not null);

		private static IEnumerable<AssetRefData> ProcessAssets(IEnumerable<ScriptableObject> assets)
		{
			return
				from asset in assets
				let applicableFields = GetApplicableFields(asset.GetType()).Where(FieldHasItemIdAttribute).ToArray()
				where applicableFields.Length > 0
				select new AssetRefData(asset, applicableFields);

			static bool FieldHasItemIdAttribute(FieldInfo fieldInfo) => fieldInfo.CustomAttributes.Any(x => x.AttributeType == typeof(ItemIdAttribute));

			static FieldInfo[] GetApplicableFields(Type type)
			{
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
				return type.GetFields(flags);
			}
		}
	}
}