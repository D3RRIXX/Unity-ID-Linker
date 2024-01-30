using System;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Derrixx.IdLinker.Editor
{
	[CustomEditor(typeof(IdTable))]
	public class IdTableEditor : UnityEditor.Editor
	{
		private ReorderableList _list;
		private SerializedProperty _definitionsProperty;
		private SerializedProperty _useGuids;

		private void OnEnable()
		{
			_definitionsProperty = serializedObject.FindProperty("_idDefinitions");
			_useGuids = serializedObject.FindProperty("_useGuids");

			_list = new ReorderableList(serializedObject, _definitionsProperty)
			{
				displayAdd = true,
				displayRemove = true,
				onAddCallback = OnItemAdded,
				onRemoveCallback = OnItemRemoved,
				drawElementCallback = DrawElement,
				elementHeightCallback = GetElementHeight,
				drawHeaderCallback = DrawHeader,
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_list.DoLayoutList();

			Utils.DrawPropertyFieldWithCallbackOnValueChanged(_useGuids, new GUIContent("Use GUIDs"), property =>
			{
				bool useGuids = property.boolValue;
				Func<string, string> conversion = useGuids ? AssetDatabase.AssetPathToGUID : AssetDatabase.GUIDToAssetPath;
				ConvertAssetRefsPaths(_definitionsProperty, conversion);
			});

			serializedObject.ApplyModifiedProperties();
		}

		private static void ConvertAssetRefsPaths(SerializedProperty definitionsProperty, Func<string, string> conversion)
		{
			foreach (SerializedProperty idDefinitionProperty in definitionsProperty)
			foreach (SerializedProperty refData in EnumerateAssetRefs(idDefinitionProperty))
			{
				SerializedProperty pathProperty = refData.FindPropertyRelative(nameof(RefData.AssetPath));
				pathProperty.stringValue = conversion(pathProperty.stringValue);
			}
		}

		private void OnItemAdded(ReorderableList list)
		{
			var arrayProperty = list.serializedProperty;
			arrayProperty.InsertArrayElementAtIndex(arrayProperty.arraySize);
			var elementProperty = arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1);
			elementProperty.FindPropertyRelative("Id").stringValue = string.Empty;
			elementProperty.FindPropertyRelative("_assetRefs").ClearArray();

			list.serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private void OnItemRemoved(ReorderableList list)
		{
			var arrayProperty = list.serializedProperty;

			ReadOnlyCollection<int> indices = list.selectedIndices;

			foreach (int index in indices)
			{
				RemoveIdFromReferencedAssets(arrayProperty.GetArrayElementAtIndex(index));
				arrayProperty.DeleteArrayElementAtIndex(index);
			}
		}

		private static void RemoveIdFromReferencedAssets(SerializedProperty idDefinitionProperty)
		{
			foreach (SerializedProperty refData in EnumerateAssetRefs(idDefinitionProperty))
			{
				SerializedProperty assetProperty = refData.FindPropertyRelative(nameof(RefData.Asset));
				using var serObj = new SerializedObject(assetProperty.objectReferenceValue);
				string propertyPath = refData.FindPropertyRelative(nameof(RefData.PropertyPath)).stringValue;

				serObj.FindProperty(propertyPath).stringValue = "$MISSING";
				serObj.ApplyModifiedProperties();
			}
		}

		private void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "ID Table");
		}

		private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			EditorGUI.PropertyField(rect, _definitionsProperty.GetArrayElementAtIndex(index), includeChildren: true);
		}

		private float GetElementHeight(int index) => EditorGUI.GetPropertyHeight(_definitionsProperty.GetArrayElementAtIndex(index), includeChildren: true);

		private static SerializedProperty EnumerateAssetRefs(SerializedProperty idDefinitionProperty) => idDefinitionProperty.FindPropertyRelative("_assetRefs");
	}
}