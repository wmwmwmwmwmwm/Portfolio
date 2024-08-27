using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
	public Image cellImage1, cellImage2, arrow, escapeArrow, warning;
	public CellData data;

	[HideInInspector] public Block accupiedBlock;
	[HideInInspector] public Image frontImage, frontBottomImage;
	[HideInInspector] public TMP_Text frontNumberText;

	void Awake()
	{
		data = new CellData();
	}
}
