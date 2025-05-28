using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static SingletonManager;

public class DataManager : Singleton<DataManager>
{
	[Serializable]
	public class Stage
	{
		public string _Name;
		public int _StageNumber;
		public string _DisplayName;
		public string _SceneName;
		public Sprite _Thumbnail;
	}
	public List<Stage> _Stages;

	[Serializable]
	public class CharacterCloth
	{
		public string _Name;
		public Sprite _Icon;
		public AssetReferenceT<GameObject> _Model;
	}

	[Serializable]
	public class Character
	{
		public CharacterName _Name;
		public string _DisplayName;
		public int _Attack, _Defence, _HP;
		public float _CritChance, _CritDamage;
		public Sprite _Thumbnail;
		public Sprite _Thumbnail2;
		public List<Naninovel.Script> _StoryScripts;
		public List<CharacterCloth> _Clothes;

		public CharacterCloth GetCloth(string name) => _Clothes.Find(x => x._Name == name);
	}
	public List<Character> _Characters;

	[Serializable]
	public class Card
	{
		public CardName _Name;
		public CardType _Type;
		public string _DisplayName;
		public string _Desc;
		[HideIf("_IsSuperior")] public CardName _SuperiorCard;
		public bool _IsSuperior;
	}
	public List<Card> _Cards;

	protected override void Init()
	{
	}

	public Character GetCharacter(CharacterName name)
	{
		return _Characters[(int)name];
	}

	public CharacterCloth GetCharacterCloth(CharacterName name)
	{
		string cloth = UserData.GetCharacterInfo(name)._ClothName;
		return GetCharacter(name).GetCloth(cloth);
	}

	public int GetCharacterPower(CharacterName name)
	{
		Character character = GetCharacter(name);
		float power = character._Attack * 5;
		power += character._Defence * 5;
		power += character._HP;
		power += character._CritChance * 100f * 5f;
		power += (character._CritDamage - 1f) * 100f * 5f;
		return (int)power;
	}

	public int GetCharacterAttack(CharacterName name)
	{
		Character character = GetCharacter(name);
		return character._Attack + User.GetWeaponLevel(name) * 5;
	}

	public float GetCharacterCritChance(CharacterName name)
	{
		Character character = GetCharacter(name);
		return character._CritChance + User.GetWeaponLevel(name) * 0.01f;
	}

	public float GetCharacterCritDamage(CharacterName name)
	{
		Character character = GetCharacter(name);
		return character._CritDamage + User.GetWeaponLevel(name) * 0.05f;
	}

	public Card GetCard(CardName name)
	{
		return _Cards[(int)name];
	}
}