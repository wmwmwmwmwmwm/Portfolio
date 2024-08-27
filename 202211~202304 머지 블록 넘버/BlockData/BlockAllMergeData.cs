using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAllMergeData : BlockData
{
    public Vector2Int direction;

    public override BlockData GetCopy()
    {
        BlockAllMergeData copied = (BlockAllMergeData)base.GetCopy();
        copied.direction = direction;
        return copied;
    }
}