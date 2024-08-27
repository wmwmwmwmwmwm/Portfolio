using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static GameInterface;

public class Block : MonoBehaviour, IPointerClickHandler
{
	public Animator thisAnimator;
	public Rigidbody2D thisRigidbody;
	public BoxCollider2D thisCollider;
	public Image baseImage, blockTargetImage, blockBeakerImage;
	public HorizontalLayoutGroup textImageParent;
	public List<Image> textImages;
	public Image overlayImageBeforeText, overlayImageAfterText;
	public BlockType blockTypeInGame;

	[HideInInspector] public BlockData data;
	[HideInInspector] public Cell accupiedCell;
	[HideInInspector] public int mergedTurn, movedTurn;
	[HideInInspector] public List<Connector> attachedConnectors;
	[HideInInspector] public bool mergePriority;

	public bool canMerge => blockTypeInGame is BlockType.Normal or BlockType.Target;

	public void SetNumberType(NumberType newNumberType)
	{
		data.numberType = newNumberType;
		data.numberType = (NumberType)Mathf.Min((int)data.numberType, (int)NumberType.N1024T);

		(int number, int _kmbt) = TypeHelper.NumberTypeToShortNumber(data.numberType);
		int thousand = number / 1000;
		number %= 1000;
		int hundred = number / 100;
		number %= 100;
		int ten = number / 10;
		number %= 10;
		int one = number;
		textImages[0].gameObject.SetActive(thousand > 0);
		textImages[0].sprite = BlockFactoryInst.numberSprites[thousand];
		textImages[1].gameObject.SetActive(thousand > 0 || hundred > 0);
		textImages[1].sprite = BlockFactoryInst.numberSprites[hundred];
		textImages[2].gameObject.SetActive(thousand > 0 || hundred > 0 || ten > 0);
		textImages[2].sprite = BlockFactoryInst.numberSprites[ten];
		textImages[3].gameObject.SetActive(thousand > 0 || hundred > 0 || ten > 0 || one > 0);
		textImages[3].sprite = BlockFactoryInst.numberSprites[one];
		textImages[4].gameObject.SetActive(_kmbt > 0);
		textImages[4].sprite = _kmbt switch
		{
			0 => null,
			1 => BlockFactoryInst.spriteK,
			2 => BlockFactoryInst.spriteM,
			3 => BlockFactoryInst.spriteB,
			4 => BlockFactoryInst.spriteT,
			_ => null,
		};
		textImages.ForEach(x => x.SetNativeSize());
		switch (blockTypeInGame)
		{
			case BlockType.Normal:
			case BlockType.Target:
				baseImage.color = BlockFactoryInst.GetBlockColor(data.numberType);
				blockBeakerImage.color = BlockFactoryInst.GetBlockColor(data.numberType).WithAlpha(0f);
				overlayImageBeforeText.gameObject.SetActive(_kmbt > 0);
				overlayImageBeforeText.sprite = BlockFactoryInst.normalBlockSprites[_kmbt];
				break;
			case BlockType.Ice:
				baseImage.color = BlockFactoryInst.GetBlockColor(data.numberType);
				break;
			case BlockType.Copy:
				overlayImageBeforeText.color = BlockFactoryInst.GetBlockColor(data.numberType);
				break;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
        switch (blockTypeInGame)
        {
            case BlockType.Normal:
            case BlockType.Divide:
            case BlockType.Joker:
                BlockController.instance.selectedBlock = this;
                break;
        }
    }

    public void SetZeroBlock()
	{
        textImages[0].gameObject.SetActive(true);
        textImages[1].gameObject.SetActive(false);
        textImages[2].gameObject.SetActive(false);
        textImages[3].gameObject.SetActive(false);
        textImages[4].gameObject.SetActive(false);
        textImages[0].sprite = BlockFactoryInst.numberSprites[0];
        textImages.ForEach(x => x.SetNativeSize());
    }
}
