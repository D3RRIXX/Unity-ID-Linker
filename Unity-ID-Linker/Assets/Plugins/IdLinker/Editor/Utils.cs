using System;
using UnityEditor;
using UnityEngine;

namespace Derrixx.IdLinker.Editor
{
	public static class Utils
	{
		public static void DrawPropertyFieldWithCallbackOnValueChanged(SerializedProperty property, GUIContent label, Action<SerializedProperty> callback)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property, label);
			if (EditorGUI.EndChangeCheck())
				callback.Invoke(property);
		}
	}
}