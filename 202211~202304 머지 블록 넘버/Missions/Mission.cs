using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Mission : ScriptableObject
{
	public MissionType missionType;

	protected BlockController controller => BlockController.instance;

    public abstract void UpdateUI(Image image1, Image image2, List<Block> blockList);

	public abstract void UpdateInGameUI(TMP_Text txt);

	public abstract int GetRemainedCount();

	public abstract bool CheckMissionClear();

	public virtual Mission GetCopy()
	{
		Mission copied = (Mission)CreateInstance(this.GetType());
		copied.missionType = missionType;
		return copied;
	}

    public static Mission CreateMissionFromMissionType(MissionType type)
    {
        Mission mission = null;
        switch (type)
        {
            case MissionType.Score:
                mission = CreateInstance<Mission_Score>();
                break;
            case MissionType.Number:
                mission = CreateInstance<Mission_Number>();
                break;
            case MissionType.TargetCell:
                mission = CreateInstance<Mission_TargetCell>();
                break;
            case MissionType.MergeBlock:
                mission = CreateInstance<Mission_MergeBlock>();
                break;
            case MissionType.EscapeBlock:
                mission = CreateInstance<Mission_EscapeBlock>();
                break;
            case MissionType.DestroyDummy:
                mission = CreateInstance<Mission_DestroyDummy>();
                break;
            case MissionType.DestroyIce:
                mission = CreateInstance<Mission_DestroyIce>();
                break;
            case MissionType.CopyBlock:
                mission = CreateInstance<Mission_CopyBlock>();
                break;
        }
        return mission;
    }
}