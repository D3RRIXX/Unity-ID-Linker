using System.Collections.ObjectModel;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Derrixx.IdLinker.Editor
{
	[CustomEditor(typeof(IdTable))]
	public class IdTableEditor : UnityEditor.Editor
	{
		private ReorderableList _reorderableList;

		private void OnEnable()
		{
			_reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("_idDefinitions"))
			{
				displayAdd = true,
				displayRemove = true,
				onRemoveCallback = OnItemRemoved
			};
		}

		private void OnItemRemoved(ReorderableList list)
		{
			ReadOnlyCollection<int> indices = list.selectedIndices;
			foreach (int index in indices)
			{
				Debug.Log(index);
			}
		}

		public override void OnInspectorGUI()
		{
			_reorderableList.DoLayoutList();
		}
	}
}