using UnityEngine;

namespace BoraBattle.Game.BingoMasterKing
{
	// 테스트용 기능들
	public partial class BingoController
	{
#if MobirixTest
		void UpdateDevTest()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				CreateNewItem(BingoItem.ItemType.Select1);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				CreateNewItem(BingoItem.ItemType.Select2);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				CreateNewItem(BingoItem.ItemType.Horizontal);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				CreateNewItem(BingoItem.ItemType.Vertical);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				CreateNewItem(BingoItem.ItemType.RandomBall);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				CreateNewItem(BingoItem.ItemType.AddTime);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				CreateNewItem(BingoItem.ItemType.DoubleScore);
			}
		}
#endif
	}
}
