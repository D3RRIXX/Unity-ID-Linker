﻿using System.Collections.ObjectModel;
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
				Debug.Log(property.boolValue ? "Use GUIDs" : "Use asset paths");
				if (property.boolValue)
				{
					// Convert all refs to GUIDs
					for (int i = 0; i < _definitionsProperty.arraySize; i++)
					{
						foreach (SerializedProperty refData in _definitionsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_assetRefs"))
						{
							SerializedProperty path = refData.FindPropertyRelative(nameof(RefData.AssetPath));
							path.stringValue = AssetDatabase.AssetPathToGUID(path.stringValue);
						}
					}
				}
				else
				{
					// Convert all refs to asset paths
					for (int i = 0; i < _definitionsProperty.arraySize; i++)
					{
						foreach (SerializedProperty refData in _definitionsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_assetRefs"))
						{
							SerializedProperty path = refData.FindPropertyRelative(nameof(RefData.AssetPath));
							path.stringValue = AssetDatabase.GUIDToAssetPath(path.stringValue);
						}
					}
				}
			});
			
			serializedObject.ApplyModifiedProperties();
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
			foreach (SerializedProperty refData in idDefinitionProperty.FindPropertyRelative("_assetRefs"))
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
	}
}