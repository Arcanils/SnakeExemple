using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
	public IBaseEntity RefEntity;
	public readonly TilePersistantData Data;
	public readonly RectTransform RefUiTile;

	public Tile(TilePersistantData data, RectTransform refUiTile)
	{
		RefEntity = null;
		Data = data;
		RefUiTile = refUiTile;
	}
}

[System.Serializable]
public class TilePersistantData
{
	public enum ETileType
	{
		Ground,
		Wall,
	}
	public Sprite Terrain;
	public ETileType Type;
}
