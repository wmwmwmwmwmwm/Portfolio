using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SingletonManager;

public partial class LobbyController
{
	void HideModels()
	{
		foreach (CharInfo info in _CharInfos)
		{
			if (!info._ModelInst) continue;
			info._ModelInst.transform.SetParent(_CharacterCamera.transform);
			info._ModelInst.SetActive(false);
		}
	}

	void ShowModel(ViewType viewType, CharacterName charName, int posIndex = 0)
	{
		// 로드
		CharInfo info = GetCharInfo(charName);
		if (!info._ModelInst)
		{
			DataManager.CharacterCloth cloth = Data.GetCharacterCloth(charName);
			info._LoadHandle = Addressables.LoadAssetAsync<GameObject>(cloth._Model);
			info._LoadHandle.WaitForCompletion();
			info._ModelInst = Instantiate(info._LoadHandle.Result);
			Util.SetActiveSchoolUniform(info._ModelInst, cloth);
			Animator animator = info._ModelInst.GetComponent<Animator>();
			animator.runtimeAnimatorController = info._AnimatorAsset;
			Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
			info._WeaponModelInst = Instantiate(info._WeaponData.prefab, rightHand);
			info._WeaponModelInst.transform.SetLocalPositionAndRotation(info._WeaponPosition.localPosition, info._WeaponPosition.localRotation);
		}

		// 활성화
		Transform parent = GetModelParent(viewType, posIndex);
		parent.eulerAngles = new(3f, 180f, 0f);
		info._ModelInst.SetActive(true);
		info._ModelInst.transform.SetParent(parent, false);
		info._ModelInst.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

		// 애니메이션
		//info.GetAnimator().Play("Idle", 0, 0f);
	}

	Transform GetModelParent(ViewType viewType, int posIndex = 0)
	{
		return viewType switch
		{
			ViewType.Main => _Main_CharacterPosition,
			ViewType.Character => _Character_CharacterPosition,
			ViewType.Store => _Store_CharacterPosition,
			_ => _Troop_CharacterPositions[posIndex]
		};
	}
}
