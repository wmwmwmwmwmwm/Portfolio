using System.Collections.Generic;
using UnityEngine;

public class Block_Dummy : SpecialBlock
{
	public List<Sprite> hpSprites;
	public List<GameObject> hpParticles;
	BlockDummyData data;

	public override void Start()
	{
		data = (BlockDummyData)block.data;
		SetHP(data.HP);
	}

	public override void OnMergeNeighbor()
	{
		if (data.HP <= 0) return;
		SetHP(data.HP - 1);
	}

	public void SetHP(int newHP)
	{
		int lastHP = data.HP;
		data.HP = newHP;
		if (data.HP < lastHP)
		{
			controller.DummyBlockSetHPEffect(block);
		}
		int index = Mathf.Clamp(5 - data.HP, 0, hpSprites.Count - 1);
		block.baseImage.sprite = hpSprites[index];
		for (int i = 0; i < hpParticles.Count; i++)
		{
			hpParticles[i].SetActive(i == index);
		}
		if (data.HP == 0)
		{
			controller.RemoveBlock(block, true, true);
		}
	}
}
