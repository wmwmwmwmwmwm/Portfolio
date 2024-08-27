using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameInterface;
using static SoundManager;

public partial class BlockController
{
	async void UseItemBlock(ItemType itemType)
	{
		usingItemType = itemType;
		BlockData itemBlockData = BlockFactoryInst.CreateBlockData(BlockType.Normal, NumberType.N2);
		itemBlockData.blockType = itemType switch
		{
			ItemType.Divide => BlockType.Divide,
			ItemType.Add => BlockType.Joker,
			_ => throw new System.NotImplementedException(),
		};
		stageData.startBlocks.Insert(0, nextBlockData);
		stageData.startBlocks.Insert(0, currentDropBlock.data);
		nextBlockData = itemBlockData;
		blockRecreateTrigger = true;
		await UniTask.WaitUntil(() => !blockRecreateTrigger);
		bOnCreateItemProcess = false;

		Debug.Log("itemCancelBar ON");
		itemCancelBar.SetActive(true);
		itemCancel_Text.text = itemType switch
		{
			ItemType.Divide => GetText("ingame_item_purchase_divide_desc"),
			ItemType.Add => GetText("ingame_item_purchase_add_desc"),
			_ => ""
		};

		itemCancelTrigger = false;
		await UniTask.WaitUntil(() => itemCancelTrigger || gameState != GameState.Drop);

		if (itemCancelTrigger)
		{
			itemCancelBar.SetActive(false);
			usingItemType = null;
			blockRecreateTrigger = true;
            await UniTask.WaitUntil(() => !blockRecreateTrigger);
            itemCancelTrigger = false;
            return;
		}

        itemCancelBar.SetActive(false);
	}

	void UpdateItemButtons()
	{
		UpdateItemIcon(itemButtonDivide, ItemCountToString(UserMgr.ItemDivideCount), FreeItemAvailable(ItemType.Divide));
		UpdateItemIcon(itemButtonJoker, ItemCountToString(UserMgr.ItemAddCount), FreeItemAvailable(ItemType.Add));
		UpdateItemIcon(itemButtonHammer, ItemCountToString(UserMgr.ItemHammerCount), FreeItemAvailable(ItemType.Hammer));

		void UpdateItemIcon(ItemButton itemButton, string countString, bool freeAvailable)
		{
			itemButton.numBoard.SetActive(false);
			itemButton.freeBoard.SetActive(false);
			itemButton.lineEffect.SetActive(false);
			if (freeAvailable)
			{
				itemButton.freeBoard.SetActive(true);
				itemButton.lineEffect.SetActive(true);
			}
			else
			{
				itemButton.numBoard.SetActive(true);
				itemButton.itemCountText.text = countString;
			}
		}

		string ItemCountToString(int count)
		{
			if (count > 99) return "99+";
			else return count.ToString();
		}
	}

	async void UseItemHammer()
	{
		usingItemType = ItemType.Hammer;
		itemCancelBar.SetActive(true);
		itemCancel_Text.text = GetText("ingame_item_purchase_hammer_desc");
		userMovePause = true;
		itemCancelTrigger = false;
		selectedBlock = null;
		boardInputArea.gameObject.SetActive(false);
		await UniTask.WaitUntil(() => itemCancelTrigger || (selectedBlock && selectedBlock != currentDropBlock));

		itemCancelBar.SetActive(false);
		boardInputArea.gameObject.SetActive(true);
		if (itemCancelTrigger)
		{
			itemCancelTrigger = false;
			usingItemType = null;
			userMovePause = false;
            return;
		}

		await DecreaseItemCount(ItemType.Hammer);
		SoundMgr.PlaySfx(SfxType.ItemHammer);
		List<Block> blocksToRemove = boardBlocks.FindAll(x => x.blockTypeInGame == selectedBlock.blockTypeInGame && x.data.numberType == selectedBlock.data.numberType && x != currentDropBlock);
		foreach (Block block in blocksToRemove)
		{
			RemoveBlock(block, false, true);
			GameObject effect = InitializeEffect(block.transform.position, itemHammerEffectPrefab, true);
			ApplyEffectColor(effect, block);
		}

		await UniTask.Delay(700);
		await BoardProcess();
		usingItemType = null;
		userMovePause = false;
        itemCancelTrigger = false;
    }

	async UniTask DecreaseItemCount(ItemType itemType)
	{
		bool bIsFree = FreeItemAvailable(itemType);

        switch (itemType)
		{
			case ItemType.Divide:
				UserMgr.UpdateQuestCount(QuestType.UseItem0, 1);
				break;
			case ItemType.Add:
				UserMgr.UpdateQuestCount(QuestType.UseItem1, 1);
				break;
			case ItemType.Hammer:
				UserMgr.UpdateQuestCount(QuestType.UseItem2, 1);
				break;
		}
		if (bIsFree)
		{
			switch (itemType)
			{
				case ItemType.Divide:
					UserMgr.LastUsedFreeItem_Divide = UserMgr.CurrentTime.Date;
					break;
				case ItemType.Add:
					UserMgr.LastUsedFreeItem_Joker = UserMgr.CurrentTime.Date;
					break;
				case ItemType.Hammer:
					UserMgr.LastUsedFreeItem_Hammer = UserMgr.CurrentTime.Date;
					break;
			}
		}
		else
		{
			UserMgr.SetItemCount(itemType, UserMgr.GetItemCount(itemType) - 1);
		}
		UserMgr.UpdateQuestCount(QuestType.UseItem, 1);
		UserMgr.SaveUserInfo();

		if(UserMgr.IsLoginUser())
		{
            if (bIsFree)	await UserMgr.UploadUserInfo();
            else			await UserMgr.UploadUserInfo_Coins();
        }

        UpdateItemButtons();
    }

	bool FreeItemAvailable(ItemType itemType)
	{
		int intervalDay = (UserMgr.CurrentTime - itemType switch
		{
			ItemType.Divide => UserMgr.LastUsedFreeItem_Divide,
			ItemType.Add => UserMgr.LastUsedFreeItem_Joker,
			ItemType.Hammer => UserMgr.LastUsedFreeItem_Hammer,
			_ => throw new NotImplementedException()
		}).Days;
		return intervalDay >= 1;
	}
}
