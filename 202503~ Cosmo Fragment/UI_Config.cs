using GameCreator.Variables;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SingletonManager;

public class UI_Config : MonoBehaviour 
{
	[Header("공통")]
	public UI_Button _CloseButton;
	public UI_Button _GraphicTabButton, _CameraTabButton, _ControlsTabButton, _AudioTabButton;
	public GameObject _Graphic, _Camera, _Controls, _Audio;

	[Header("사운드")]
	public UI_Config_Element _Master;
	public UI_Config_Element _Bgm;
	public UI_Config_Element _Sfx;
	public UI_Config_Element _Voice;

	bool _Init;
	List<GameObject> _AllTabs;
	List<UI_Button> _AllTabButtons;

	const string VolumeMaster = "Volume-Master";
	const string VolumeBgm = "Volume-Bgm";
	const string VolumeSfx = "Volume-Sfx";
	const string VolumeVoice = "Volume-Voice";

	public void Show(bool show)
	{
		if (!show)
		{
			Sound.PlaySfx(SoundManager.SfxType.Exit);
			gameObject.SetActive(false);
			Game.SaveSharedData();
			return;
		}

		Init();
		TabButton(_Audio, _AudioTabButton);
		gameObject.SetActive(true);
	}

	void Init()
	{
		if (_Init) return;

		// 공통
		_AllTabs = new() { _Graphic, _Camera, _Controls, _Audio };
		_AllTabButtons = new() { _GraphicTabButton, _CameraTabButton, _ControlsTabButton, _AudioTabButton };
		_CloseButton.onClick.AddListener(() => Show(false));
		_GraphicTabButton.onClick.AddListener(() => TabButton(_Graphic, _GraphicTabButton));
		_CameraTabButton.onClick.AddListener(() => TabButton(_Camera, _CameraTabButton));
		_ControlsTabButton.onClick.AddListener(() => TabButton(_Controls, _ControlsTabButton));
		_AudioTabButton.onClick.AddListener(() => TabButton(_Audio, _AudioTabButton));

		// 사운드
		_Master._Slider.onValueChanged.AddListener(MasterSlider);
		_Master._Slider.value = (float)VariablesManager.GetGlobal(VolumeMaster);
		_Bgm._Slider.onValueChanged.AddListener(BgmSlider);
		_Bgm._Slider.value = (float)VariablesManager.GetGlobal(VolumeBgm);
		_Sfx._Slider.onValueChanged.AddListener(SfxSlider);
		_Sfx._Slider.value = (float)VariablesManager.GetGlobal(VolumeSfx);
		_Voice._Slider.onValueChanged.AddListener(VoiceSlider);
		_Voice._Slider.value = (float)VariablesManager.GetGlobal(VolumeVoice);
		_Init = true;
	}

	void TabButton(GameObject activeTab, UI_Button activeTabButton)
	{
		foreach (GameObject tab in _AllTabs)
		{
			bool active = tab == activeTab;
			tab.SetActive(active);
		}

		foreach (UI_Button tabButton in _AllTabButtons)
		{
			bool active = tabButton == activeTabButton;
			tabButton.transform.Find("Active").gameObject.SetActive(active);
		}
	}

	void MasterSlider(float v)
	{
		Sound._Mixer.SetFloat("Master", SliderToVolume(v));
		VariablesManager.SetGlobal(VolumeMaster, v);
		_Master._Slider.GetComponentInChildren<TMP_Text>().text = v.ToPercentString();
	}

	void BgmSlider(float v)
	{
		Sound._Mixer.SetFloat("Bgm", SliderToVolume(v));
		VariablesManager.SetGlobal(VolumeBgm, v);
		_Bgm._Slider.GetComponentInChildren<TMP_Text>().text = v.ToPercentString();
	}

	void SfxSlider(float v)
	{
		Sound._Mixer.SetFloat("Sfx", SliderToVolume(v));
		VariablesManager.SetGlobal(VolumeSfx, v);
		_Sfx._Slider.GetComponentInChildren<TMP_Text>().text = v.ToPercentString();
	}

	void VoiceSlider(float v)
	{
		Sound._Mixer.SetFloat("Voice", SliderToVolume(v));
		VariablesManager.SetGlobal(VolumeVoice, v);
		_Voice._Slider.GetComponentInChildren<TMP_Text>().text = v.ToPercentString();
	}

	float SliderToVolume(float v)
	{
		return Mathf.Lerp(-80f, 0f, Mathf.Sqrt(v));
	}
}
