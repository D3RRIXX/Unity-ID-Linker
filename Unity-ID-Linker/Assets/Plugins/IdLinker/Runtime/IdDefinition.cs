using System.Collections.Generic;
using UnityEngine;

namespace Derrixx.IdLinker
{
	[System.Serializable]
	public class IdDefinition
	{
		public string Id;
		[SerializeField] private List<RefData> _assetRefs = new();

		public bool AddObjectRef(RefData refData)
		{
			if (_assetRefs.Contains(refData))
				return false;

			_assetRefs.Add(refData);
			return true;
		}

		public bool RemoveObjectRef(RefData refData) => _assetRefs.Remove(refData);

		public bool Contains(RefData refData) => _assetRefs.Contains(refData);
	}
}