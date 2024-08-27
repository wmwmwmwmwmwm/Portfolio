using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoraBattle.Game.ShootingKing
{
	public class ShootingResultPanel : MonoBehaviour
	{
		public CanvasGroup pointScoreRecord, perfectShotRecord, niceShotRecord, goodShotRecord, timeBonusRecord, totalScoreRecord;
		public TMP_Text pointScoreSubject, perfectShotSubject, niceShotSubject, goodShotSubject, timeBonusSubject, totalScoreSubject;
		public TMP_Text pointScoreText, perfectShotText, niceShotText, goodShotText, timeBonusText, totalScoreText;
		public Image submitScoreButtonGauge;
	}
}