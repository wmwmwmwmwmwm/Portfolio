using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static SingletonManager;

public class SoundManager : Singleton<SoundManager>
{
	public AudioMixer _Mixer;

	public AudioClip _Title, _Lobby;
	public AudioClip _Click, _Exit, _MouseOver, _Slide, _Card;

	public AudioSource _Bgm;
	public List<AudioSource> _Sfxs;

	public enum BgmType { None, Title, Lobby, };

	public enum SfxType { None, Click, Exit, MouseOver, Slide, Card };

	List<AudioClip> _BgmClips;
	List<AudioClip> _SfxClips;
	BgmType _CurrentBgm;

	protected override void Init()
	{
		_BgmClips = new()
		{
			_Title,
			_Lobby,
		};
		_SfxClips = new()
		{
			null,
			_Click,
			_Exit,
			_MouseOver,
			_Slide,
			_Card,
		};
	}

	public void PlayBgm(BgmType bgmType, float fadeTime = 0f)
	{
		if (bgmType == _CurrentBgm) return;
		_Bgm.DOComplete();
		if (bgmType > BgmType.None)
		{
			_Bgm.volume = 0f;
			_Bgm.DOFade(1f, fadeTime);
			_Bgm.clip = _BgmClips[(int)(bgmType - 1)];
			_Bgm.Play();
		}
		else
		{
			_Bgm.DOFade(0f, 3f).SetEase(Ease.InQuad);
		}
		_CurrentBgm = bgmType;
	}

	public void PlaySfx(SfxType sfxType) => PlaySfx(_SfxClips[(int)sfxType]);
	public void PlaySfx(AudioClip sfxClip)
	{
		if (!sfxClip) return;

		AudioSource sourceAlready = _Sfxs.Find(x => x.clip == sfxClip);
		if (sourceAlready && sourceAlready.isPlaying && GetPosition(sourceAlready) < 0.4f)
		{
			sourceAlready.Play();
			return;
		}

		AudioSource idleSource = null;
		foreach (AudioSource sfx in _Sfxs)
		{
			if (!sfx.isPlaying)
			{
				idleSource = sfx;
				break;
			}
		}
		if (!idleSource)
		{
			idleSource = _Sfxs.MinBy(x => 1f - GetPosition(x));
		}
		idleSource.clip = sfxClip;
		idleSource.Play();
	}

	float GetPosition(AudioSource source) => source.time / source.clip.length;
}
