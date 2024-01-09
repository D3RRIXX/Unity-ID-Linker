using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Derrixx.IdLinker.Editor
{
	[CustomPropertyDrawer(typeof(ItemIdAttribute))]
	public class ItemIdDrawer : PropertyDrawer
	{
		private static readonly Regex SPECIAL_VALUE_REGEX = new("([A-Z])\\w+");

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType is not SerializedPropertyType.String)
			{
				EditorGUI.LabelField(position, label.text, "Item ID must be a string.");
				return;
			}
			
			EditorGUI.BeginChangeCheck();
			
			string currentValue = property.stringValue;
			bool hasSpecialValue = currentValue.StartsWith('$');
			string[] options = GetDisplayedOptions();
			
			int initialIndex = Array.IndexOf(options, currentValue);
			int index = EditorGUI.Popup(position, label.text, initialIndex, options);
			if (hasSpecialValue)
				DrawSpecialValue(position, currentValue);
			
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

		private static void DrawSpecialValue(Rect position, string currentValue)
		{
			position = new Rect(position);
			position.x += EditorGUIUtility.labelWidth;
			position.width -= EditorGUIUtility.labelWidth;

			currentValue = SPECIAL_VALUE_REGEX.Match(currentValue).Value;
			
			var style = new GUIStyle(EditorStyles.popup)
			{
				normal =
				{
					textColor = currentValue switch
					{
						"MISSING" => Color.red,
					}
				}
			};

			EditorGUI.LabelField(position, string.Empty, currentValue, style);
		}

		private static string[] GetDisplayedOptions()
		{
			return IdTable.Instance.Ids;
		}
	}
}