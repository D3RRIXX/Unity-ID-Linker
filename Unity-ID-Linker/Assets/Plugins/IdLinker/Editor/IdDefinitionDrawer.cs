using UnityEditor;
using UnityEngine;

namespace Derrixx.IdLinker.Editor
{
	[CustomPropertyDrawer(typeof(IdDefinition))]
	public class IdDefinitionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var idProperty = property.FindPropertyRelative(nameof(IdDefinition.Id));
			var assetRefsProperty = property.FindPropertyRelative("_assetRefs");
			
			string oldId = idProperty.stringValue;

			var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

			EditorGUI.BeginChangeCheck();
			EditorGUI.DelayedTextField(rect, idProperty, GUIContent.none);

#if DRAW_REFS
			position.y += EditorGUIUtility.singleLineHeight;
			position.height -= EditorGUIUtility.singleLineHeight;
			
			EditorGUI.PropertyField(position, assetRefsProperty);
#endif

			if (EditorGUI.EndChangeCheck() && !string.Equals(idProperty.stringValue, oldId))
			{
				// Update fields of all asset refs
				foreach (SerializedProperty refData in assetRefsProperty)
				{
					UpdatedFieldValue(refData, idProperty.stringValue);
				}
			}
		}

		private static void UpdatedFieldValue(SerializedProperty refData, string newId)
		{
			using var serObj = new SerializedObject(refData.FindPropertyRelative(nameof(RefData.Asset)).objectReferenceValue);
			string propertyPath = refData.FindPropertyRelative(nameof(RefData.PropertyPath)).stringValue;
			serObj.FindProperty(propertyPath).stringValue = newId;
			serObj.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float lineHeight = EditorGUIUtility.singleLineHeight;
			
			float height = lineHeight;
#if DRAW_REFS
			while (property.NextVisible(true))
			{
				height += lineHeight;
			}
#endif
			
			return height;
		}
	}
}