using DG.Tweening;
using Naninovel;
using Naninovel.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class BattleMessage : MonoBehaviour
{
	[Serializable]
	public class CharPortrait
	{
		public CharacterName _Name;
		public string _DisplayName;
		public Sprite _PortraitSprite;
	}

	public CanvasGroup _CanvasGroup;
	public Image _Portrait;
	public TMP_Text _Name;
	public TMP_Text _Text;

	public List<CharPortrait> _CharacterPortraits;

	public void Show(Script script, string label)
	{
		int index = script.GetLineIndexForLabel(label);
		if (index == -1)
		{
			Debug.LogWarning($"'{label}' 레이블을 찾을 수 없음");
			return;
		}

		UI.StartCoroutine(Internal());
		IEnumerator Internal()
		{
			gameObject.SetActive(true);
			ITextLocalizer localizer = Engine.GetService<ITextLocalizer>();
			while (true)
			{
				index++;
				if (script.GetLabelForLine(index) != label) break;
				if (script.Lines[index] is not GenericTextScriptLine textLine) continue;

				// 출력
				foreach (Command command in textLine.InlinedCommands)
				{
					if (command is not PrintText printText) continue;

					_Name.text = printText.AuthorId;
					_Portrait.sprite = _CharacterPortraits.Find(x => x._DisplayName == printText.AuthorId)._PortraitSprite;
					_Text.text = localizer.Resolve(printText.Text.Value);
				}

				// 트위닝
				Util.PanelTween(_CanvasGroup, Vector2.left);
				yield return new WaitForSeconds(3f);
			}

			Util.PanelTween(_CanvasGroup, Vector2.left, true);
			yield return new WaitForSeconds(1f);
			gameObject.SetActive(false);
		}
	}
}
