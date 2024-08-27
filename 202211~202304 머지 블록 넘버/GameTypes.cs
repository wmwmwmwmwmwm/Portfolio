using UnityEngine;

public enum MissionType { Score, Number, TargetCell, MergeBlock, EscapeBlock, DestroyDummy, DestroyIce, CopyBlock };
public enum BlockType { Normal, Target, Escape, Dummy, Ice, Copy, AllMerge, Brick, Divide, Multiply, Joker };
public enum NumberType { None, N2, N4, N8, N16, N32, N64, N128, N256, N512, N1024, N2K, N4K, N8K, N16K, N32K, N64K, N128K, N256K, N512K, N1024K, N2M, N4M, N8M, N16M, N32M, N64M, N128M, N256M, N512M, N1024M, N2B, N4B, N8B, N16B, N32B, N64B, N128B, N256B, N512B, N1024B, N2T, N4T, N8T, N16T, N32T, N64T, N128T, N256T, N512T, N1024T };
public enum CellType { Blank, TargetNumber };
public enum ItemType { Divide, Add, Hammer };
public enum QuestType { GameStart = 1, MergeNumber, TotalNumber, Over2048, PlayUnlimit, DailyMissionClear, WeeklyMissionClear, UseItem, IapPurchase, PlayStage, UseSkill, UseItem0, UseItem1, UseItem2, UseNextBlock, StageLevel, SeeAdvertise, MultiMerge, SkinCount, PurchasePiggy, PurchasePass, Rank };

public static class PrefsKey
{
	public const string IsFirstAccess = "IsFirstAccess";
	public const string Reviewed = "Reviewed";
	public const string BgmMute = "BgmMute";
	public const string SfxMute = "SfxMute";
}

public static class SceneName
{
    public const string Intro = "Intro";
	public const string MainMenu = "MainMenu";
	public const string InGame = "InGame";
	public const string MapTool = "MapTool";
}

public static class TypeHelper
{
	public static (int number, int _kmbt) NumberTypeToShortNumber(NumberType numberType)
	{
		if (numberType == NumberType.None) return (0, 0);
		int numberRemainder = (int)(numberType - 1) % 10;
		int number = (int)Mathf.Pow(2, numberRemainder + 1);
		int _kmbt = (int)(numberType - 1) / 10;
		return (number, _kmbt);
	}

	public static string NumberTypeToString(NumberType numberType)
	{
		(int number, int _kmbt) = NumberTypeToShortNumber(numberType);
		string kmbt = _kmbt switch
		{
			1 => "K",
			2 => "M",
			3 => "B",
			4 => "T",
			_ => ""
		};
		return string.Format("{0}{1}", number, kmbt);
	}

	public static long NumberTypeToLong(NumberType numberType)
	{
		if (numberType == NumberType.None) return 0L;
		long n = 2;
		for (int i = 0; i < (int)numberType - 1; i++)
		{
			n *= 2;
		}
		return n;
	}

	public static long IntNumberTypeToLong(int numberType)
	{
		if ((NumberType)numberType == NumberType.None) return 0L;
		long n = 2;
		for (int i = 0; i < numberType - 1; i++)
		{
			n *= 2;
		}
		return n;
	}
}