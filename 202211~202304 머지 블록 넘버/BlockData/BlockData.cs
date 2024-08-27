using System;
using UnityEngine;

public class BlockData : ScriptableObject
{
	/// <summary>
	/// 0 : ���� �Ҵ���� ���� / 1~1000 : �������� ���忡 �̸� ������ ��� / 1001~ : ���� �� ���� ��ϵ�
	/// </summary>
	public int Id;
	public Vector2Int coord;
	public BlockType blockType;
	public NumberType numberType;
	public float spawnChance;
	public int boardLimit, spawnCount;

	public virtual BlockData GetCopy()
	{
		BlockData copied = (BlockData)CreateInstance(GetType());
		copied.Id = Id;
		copied.coord = coord;
		copied.blockType = blockType;
		copied.numberType = numberType;
		copied.spawnChance = spawnChance;
		copied.boardLimit = boardLimit;
		copied.spawnCount = spawnCount;
		return copied;
	}

    public static BlockData CreateBlockDataFromBlockType(BlockType type)
    {
        BlockData data = null;
        switch (type)
        {
            case BlockType.Dummy:
                data = CreateInstance<BlockDummyData>();
                break;
            case BlockType.AllMerge:
                data = CreateInstance<BlockAllMergeData>();
                break;
            default:
                data = CreateInstance<BlockData>();
                break;
        }
        return data;
    }
}

[Serializable]
public class CellData
{
	[HideInInspector] public Vector2Int coord;
	public CellType cellType;
}