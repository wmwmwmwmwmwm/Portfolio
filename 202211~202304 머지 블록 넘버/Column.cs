using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Column : MonoBehaviour
{
	public Image thisImage;
	public enum ColumnState { None, Touch, Drop };
	ColumnState columnState;

	public void SetColumnState(ColumnState _columnState)
	{
		if (columnState == _columnState) return;
		columnState = _columnState;
		thisImage.DOKill();
		//thisImage.color = Color.white.WithAlpha(0f);
		switch (columnState)
		{
			case ColumnState.None:
				thisImage.DOFade(0f, 0.4f);
				break;
			case ColumnState.Touch:
				thisImage.DOFade(0.4f, 0.4f);
				break;
			case ColumnState.Drop:
				Sequence sequence = DOTween.Sequence();
				sequence.Append(thisImage.DOFade(0.4f, 0.1f));
				sequence.Append(thisImage.DOFade(0f, 0.3f));
				break;
		}
	}
}
