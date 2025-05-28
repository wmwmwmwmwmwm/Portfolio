using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SingletonManager;
using CharacterInfo = UserInfoManager.SaveData.CharacterInfo;

public class UserInfoManager : Singleton<UserInfoManager>
{
	[Serializable]
	public class SaveData
	{
		[Serializable]
		public class CharacterInfo
		{
			public CharacterName _CharacterName;
			public int _WeaponLevel;
			public List<CardName> _EquippedCards_Single, _EquippedCards_Multi;
			public List<int> _HaveSeenStorys;
			public string _ClothName;

			[JsonIgnore] public List<CardName> EquippedCards => Game._IsSingleMode ? _EquippedCards_Single : _EquippedCards_Multi;
		}
		[ReadOnly] public int _Gold, _Chip;
		[ReadOnly] public List<CharacterName> _Troop_Single, _Troop_Multi;
		[ReadOnly] public List<CharacterInfo> _CharacterInfos;
		[ReadOnly] public List<CardName> _Cards;
		[ReadOnly] public List<CardName> _StoreSoldCards;

		[JsonIgnore] public List<CharacterName> Troop => Game._IsSingleMode ? _Troop_Single : _Troop_Multi;

		public CharacterInfo GetCharacterInfo(CharacterName charName) => _CharacterInfos[(int)charName];
	}

	[ReadOnly] public SaveData _SaveData;
	int _Profile;

	protected override void Init()
	{
		_Profile = -1;

		// 유저 정보 로드
		Game.LoadSharedData();
		if (Game._StartSceneName != SceneName.Intro)
		{
			Load(1);
		}
	}

	public bool CheckProfile(int profile)
	{
		return File.Exists(GetFilePath(profile)) && Game._SharedData._LoadPreviews.Exists(x => x._Profile == profile);
	}

	public void Save()
	{
		if (_Profile < 0) return;

		// 파일로 저장
		string json = JsonConvert.SerializeObject(_SaveData);
		File.WriteAllText(GetFilePath(_Profile), json);

		// 로드 프리뷰 저장
		Game._SharedData._LoadPreviews.RemoveAll(x => x._Profile == _Profile);
		GameManager.SharedData.LoadPreview slot = new()
		{
			_Profile = _Profile,
			_LastPlayedTime = DateTime.Now.ToOADate(),
			_NextStageName = "", // todo
		};
		Game._SharedData._LoadPreviews.Add(slot);
	}

	public void Load(int profile)
	{
		_Profile = profile;

		if (CheckProfile(profile))
		{
			// 파일에서 불러오기
			string json = File.ReadAllText(GetFilePath(_Profile));
			_SaveData = JsonConvert.DeserializeObject<SaveData>(json);
		}
		else
		{
			// 초기 데이터
			_SaveData._Gold = 1000000;
			_SaveData._Chip = 50000;
			_SaveData._Troop_Single = new() { CharacterName.Lin, CharacterName.Hana, CharacterName.Sora };
			_SaveData._Troop_Multi = new() { CharacterName.Lin, CharacterName.Hana, CharacterName.Sora };
		}

		// --- 유효성 검사 ---

		// Troop
		_SaveData._Troop_Single ??= new();
		if (_SaveData._Troop_Single == null || _SaveData._Troop_Single.Count < 3)
		{
			_SaveData._Troop_Single = new() { CharacterName.Lin, CharacterName.Hana, CharacterName.Sora };
		}
		_SaveData._Troop_Multi ??= new();
		if (_SaveData._Troop_Multi == null || _SaveData._Troop_Multi.Count < 3)
		{
			_SaveData._Troop_Multi = new() { CharacterName.Lin, CharacterName.Hana, CharacterName.Sora };
		}

		// CharacterInfo
		_SaveData._CharacterInfos ??= new();
		foreach (CharacterName charName in Enum.GetValues(typeof(CharacterName)))
		{
			if (_SaveData._CharacterInfos.Exists(x => x._CharacterName == charName)) continue;

			CharacterInfo newInfo = new()
			{
				_CharacterName = charName,
			};
			_SaveData._CharacterInfos.Add(newInfo);
		}
		foreach (CharacterInfo info in _SaveData._CharacterInfos)
		{
			if (info._EquippedCards_Single == null || info._EquippedCards_Single.Count != 3)
			{
				info._EquippedCards_Single = new() { (CardName)(-1), (CardName)(-1), (CardName)(-1) };
			}
			if (info._EquippedCards_Multi == null || info._EquippedCards_Multi.Count != 3)
			{
				info._EquippedCards_Multi = new() { (CardName)(-1), (CardName)(-1), (CardName)(-1) };
			}
			info._HaveSeenStorys ??= new();
			if (string.IsNullOrEmpty(info._ClothName) || Data.GetCharacter(info._CharacterName).GetCloth(info._ClothName) == null) 
			{
				info._ClothName = "School_Acc";
			}
		}

		// 기타
		_SaveData._Cards ??= new();
		_SaveData._StoreSoldCards ??= new();

		// --- 유효성 검사 ---

		//// Dictionary 초기화
		//foreach (CharacterInfo item in _SaveData._CharacterInfos)
		//{
		//	_CharacterInfoDict.Add(item._CharacterName, item);
		//}
	}

	public void DeleteData(int profile)
	{ 
		File.Delete(GetFilePath(profile));
	}

	public int GetWeaponLevel(CharacterName charName)
	{
		return _SaveData.GetCharacterInfo(charName)._WeaponLevel;
	}

	public void SetWeaponLevel(CharacterName charName, int level)
	{
		_SaveData.GetCharacterInfo(charName)._WeaponLevel = level;
	}

	string GetFilePath(int profile) => Path.Combine(Application.persistentDataPath, $"save{profile}.sav");
}