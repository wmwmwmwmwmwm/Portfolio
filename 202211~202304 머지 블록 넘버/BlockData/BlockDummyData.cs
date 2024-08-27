using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDummyData : BlockData
{
    public int HP;

    public override BlockData GetCopy()
    {
        BlockDummyData copied = (BlockDummyData)base.GetCopy();
        copied.HP = HP;
        return copied;
    }
}