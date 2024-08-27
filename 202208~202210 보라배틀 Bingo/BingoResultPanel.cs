using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.BingoMasterKing
{
	public class BingoResultPanel : MonoBehaviour
	{
		public Image resultButtonFillImage;
		public GameObject markRecord;
		public GameObject timeBonusRecord;
		public GameObject bingoScoreRecord;
		public GameObject boosterBonusRecord;
		public GameObject blackoutScoreRecord;
		public GameObject blackoutBonusRecord;
		public GameObject penaltyRecord;
		public GameObject totalScoreRecord;
		public TMP_Text markScoreSubject, markScoreText;
		public TMP_Text timeBonusSubject, timeBonusText;
		public TMP_Text bingoScoreSubject, bingoScoreText;
		public TMP_Text boosterBonusSubject, boosterBonusText;
		public TMP_Text blackoutScoreSubject, blackoutScoreText;
		public TMP_Text blackoutBonusSubject, blackoutBonusText;
		public TMP_Text penaltySubject, penaltyText;
		public TMP_Text totalScoreSubject, totalScoreText;
		public TMP_Text submitButtonText;
	}
}
