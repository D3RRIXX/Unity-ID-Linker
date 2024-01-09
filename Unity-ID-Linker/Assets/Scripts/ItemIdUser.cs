using Derrixx.IdLinker;
using UnityEngine;

[CreateAssetMenu(order = -90, fileName = "New Item")]
public class ItemIdUser : ScriptableObject
{
	[SerializeField, ItemId] private string _id;
	[SerializeField, ItemId] private int _test;
}