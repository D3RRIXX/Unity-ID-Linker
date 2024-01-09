using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Derrixx.IdLinker.Editor
{
	[CustomPropertyDrawer(typeof(ItemIdAttribute))]
	public class ItemIdDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType is not SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, "Item ID must be a string.");
				return;
			}

			EditorGUI.BeginChangeCheck();
			string[] options = GetDisplayedOptions();
			int initialIndex = Array.IndexOf(options, property.stringValue);
			int index = EditorGUI.Popup(position, label.text, initialIndex, options);
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = options[index];
				Object targetObject = property.serializedObject.targetObject;

				RefData refData = RefData.FromAsset(targetObject);
				if (initialIndex != -1)
				{
					// Remove ref from previous id
					IdTable.Instance.RemoveReferenceFromId(options[initialIndex], refData);
				}

				refData.PropertyPath = property.propertyPath;
				IdTable.Instance.AddReferenceToId(options[index], refData);
			}
		}

		private static string[] GetDisplayedOptions() => IdTable.Instance.Ids;
	}
}