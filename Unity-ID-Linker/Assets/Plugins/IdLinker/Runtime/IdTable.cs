using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Derrixx.IdLinker
{
	[CreateAssetMenu(order = -100, fileName = TABLE_NAME)]
	public class IdTable : ScriptableObject
	{
		private const string TABLE_NAME = "ID Table";

		public static IdTable Instance => Resources.Load<IdTable>(TABLE_NAME);

		[SerializeField] private List<IdDefinition> _idDefinitions = new();
		[SerializeField] private bool _useGuids;
	
		public string[] Ids => _idDefinitions.Select(x => x.Id).ToArray();
		public bool UseGuids => _useGuids;

		public void AddReferenceToId(string id, RefData refData)
		{
			GetIdDefinition(id).AddObjectRef(refData);
			EditorUtility.SetDirty(this);
		}

		public void RemoveReferenceFromId(string option, RefData refData)
		{
			GetIdDefinition(option).RemoveObjectRef(refData);
		
			EditorUtility.SetDirty(this);
		}

		public bool RemoveAllReferencesOfObject(RefData refData)
		{
			return _idDefinitions.Aggregate(false, (current, idDefinition) => current | idDefinition.RemoveObjectRef(refData));
		}

		private IdDefinition GetIdDefinition(string id)
		{
			return _idDefinitions.First(x => x.Id == id);
		}
	}
}