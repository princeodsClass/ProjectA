using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameAudioManager : SingletonMono<GameAudioManager>
{
	private AudioSource _asBGM;
	private AudioSource[] _asSFX;
	private string nowBGM = string.Empty;

	private float m_fBgmVolume = 0.6f;
	private float m_fSfxVolume = 0.6f;

	private static AudioMixer _audioMix = null;

	private int MAX_COUNT_SFX = 20;

	private static List<AudioSource> _sfxReadyAoudioSources = new List<AudioSource>();
	private static List<AudioSource> _sfxPlayAoudioSources = new List<AudioSource>();

	private static Dictionary<string, AudioMixerGroup> _dicAudioMixGroup = new Dictionary<string, AudioMixerGroup>();

	private void Awake()
	{
		Create();
	}

	public void Create()
	{
		_sfxReadyAoudioSources.Clear();
		_sfxPlayAoudioSources.Clear();

		if (null == _asBGM)
		{
			_asBGM = new AudioSource();
			_asBGM = gameObject.AddComponent<AudioSource>();
			_asBGM.playOnAwake = false;
			_asBGM.loop = true;
			_asBGM.volume = m_fBgmVolume;
		}

		for (int i = 0; i < MAX_COUNT_SFX; i++)
		{
			var oAudioGameObj = new GameObject("SFXAudio");
			oAudioGameObj.transform.SetParent(this.gameObject.transform, false);

			var audios = oAudioGameObj.AddComponent<AudioSource>();
			audios.playOnAwake = false;
			audios.loop = false;
			audios.volume = m_fSfxVolume;
			audios.rolloffMode = AudioRolloffMode.Linear;
			_sfxReadyAoudioSources.Add(audios);
		}

		_dicAudioMixGroup.Clear();
		if (_audioMix == null)
			_audioMix = Resources.Load<AudioMixer>("audios/MasterMixerGroup");

		_audioMix.FindMatchingGroups("Master");
		_dicAudioMixGroup.Add("Master", _audioMix.FindMatchingGroups("Master")[0]);

		for (int i = 0; i < ComType.AudioMixPaths.Length; i++)
		{
			string path = ComType.AudioMixPaths[i];
			var mix = _audioMix.FindMatchingGroups(path);

			if (mix != null)
			{
				_dicAudioMixGroup.Add(path, _audioMix.FindMatchingGroups(path)[0]);
			}
		}
	}

	public static void ChangeAudioMixSnapShot(string scene)
	{
		AudioMixerSnapshot snapShot = _audioMix.FindSnapshot(scene);

		if (snapShot != null)
			_audioMix.TransitionToSnapshots(new AudioMixerSnapshot[] { snapShot }, new float[] { .5f }, Time.deltaTime * 5.0f);
		else
		{
			snapShot = _audioMix.FindSnapshot("Default");
			_audioMix.TransitionToSnapshots(new AudioMixerSnapshot[] { snapShot }, new float[] { .5f }, Time.deltaTime * 5.0f);
		}
	}

	public void SetOption(OptionSound cOption)
	{
		m_fBgmVolume = cOption.fBGM_Vol;
		m_fSfxVolume = cOption.fSFX_Vol;

		_asBGM.volume = m_fBgmVolume;
	}

	private static AudioSource GetSFXReadyAudioSource()
	{
		for (int i = 0; i < _sfxPlayAoudioSources.Count; i++)
		{
			var tmp = _sfxPlayAoudioSources[i];

			if (tmp.isPlaying == false)
			{
				_sfxReadyAoudioSources.Add(tmp);
				_sfxPlayAoudioSources.RemoveAt(i);
			}
		}

		if (_sfxReadyAoudioSources.Count > 0)
		{
			var tmp = _sfxReadyAoudioSources[0];

			_sfxReadyAoudioSources.RemoveAt(0);
			_sfxPlayAoudioSources.Add(tmp);

			return tmp;
		}
		else
		{
			var tmp = _sfxPlayAoudioSources[0];
			_sfxPlayAoudioSources.Add(tmp);
			_sfxPlayAoudioSources.RemoveAt(0);

			return tmp;
		}
	}

	private static void _PlaySFX(AudioClip audioClip, string mixerGroup = "Master/SFX/Battle", bool loop = false, float pitch = 1.0f)
	{
		if (null == Singleton) return;

		if (_dicAudioMixGroup.ContainsKey(mixerGroup) == false)
			GameManager.Log($"Mixer Group Not found : {mixerGroup}", "red");

		if (0f >= Singleton.m_fSfxVolume) return;

		var selectAudio = GetSFXReadyAudioSource();
		selectAudio.Stop();

		selectAudio.volume = Singleton.m_fSfxVolume;
		selectAudio.clip = audioClip;
		selectAudio.loop = loop;
		selectAudio.spatialBlend = 0.0f;
		selectAudio.outputAudioMixerGroup = _dicAudioMixGroup[mixerGroup];
		selectAudio.pitch = Mathf.Clamp(pitch, 1f, GlobalTable.GetData<float>("valueSFXMaxPitch"));
		selectAudio.reverbZoneMix = 0f;
        selectAudio.rolloffMode = AudioRolloffMode.Linear;
        selectAudio.Play();
	}

	private static void _PlaySFX3D(AudioClip audioClip, Vector3 vec, float minDistance = 1, float maxDistance = 30, string mixerGroup = "Master/SFX/Battle", bool loop = false)
	{
		if (null == Singleton) return;

		if (_dicAudioMixGroup.ContainsKey(mixerGroup) == false)
			GameManager.Log($"Mixer Group Not found : {mixerGroup}", "red");

		if (0f >= Singleton.m_fSfxVolume) return;

		var selectAudio = GetSFXReadyAudioSource();

		selectAudio.pitch = 1.0f;
		selectAudio.volume = Singleton.m_fSfxVolume;
		selectAudio.clip = audioClip;
		selectAudio.loop = loop;
		selectAudio.outputAudioMixerGroup = _dicAudioMixGroup[mixerGroup];
		selectAudio.transform.position = vec;
		selectAudio.spatialBlend = 1f;
		selectAudio.minDistance = minDistance;
		selectAudio.maxDistance = maxDistance;
		selectAudio.reverbZoneMix = 0f;
        selectAudio.rolloffMode = AudioRolloffMode.Linear;
        selectAudio.Play();
	}

	private static IEnumerator _PlaySFXRoutine(AudioClip audioClip, float delay, bool bLoop, string mixerGroup, float pitch = 1.0f)
	{
		yield return YieldInstructionCache.WaitForSeconds(delay);

		_PlaySFX(audioClip, mixerGroup, bLoop, pitch);
	}

	private static IEnumerator _PlayBGMRoutine(AudioClip audioClip, bool bLoop)
	{
		if (true == Singleton._asBGM.isPlaying)
		{
			while (Singleton._asBGM.volume > 0f)
			{
				Singleton._asBGM.volume -= 0.01f;
				yield return YieldInstructionCache.WaitForEndOfFrame;
			}

			Singleton._asBGM.Stop();
		}

		if (null != audioClip)
		{
			Singleton._asBGM.loop = bLoop;
			Singleton._asBGM.clip = audioClip;

			Singleton._asBGM.outputAudioMixerGroup = _dicAudioMixGroup[ComType.BGM_MIX];
			Singleton._asBGM.Play();

			while (Singleton._asBGM.volume < Singleton.m_fBgmVolume)
			{
				Singleton._asBGM.volume += 0.01f;
				yield return YieldInstructionCache.WaitForEndOfFrame;
			}
		}
	}

	private static void _PlayBGMInstantly(AudioClip audioClip, bool bLoop)
	{
		if (null != audioClip)
		{
			Singleton._asBGM.volume = Singleton.m_fBgmVolume;
			Singleton._asBGM.loop = bLoop;
			Singleton._asBGM.clip = audioClip;
			Singleton._asBGM.outputAudioMixerGroup = _dicAudioMixGroup[ComType.BGM_MIX];
			Singleton._asBGM.Play();
		}
	}

	public static void PlaySFX(AudioClip audioClip, float delay = 0f, string mixerGroup = "Master/SFX/Battle", bool loop = false, float pitch = 1.0f)
	{
		if (null == audioClip) return;

		if (delay > 0f)
		{
			Singleton.StartCoroutine(_PlaySFXRoutine(audioClip, delay, loop, mixerGroup, pitch));
		}
		else
		{
			_PlaySFX(audioClip, mixerGroup, loop, pitch);
		}
	}

	public static void PlaySFX3D(AudioClip audioClip, Vector3 vec, float minDistance = 1, float maxDistance = 30, string mixerGroup = "Master/SFX/Battle", bool loop = false)
	{
		if (null == audioClip) return;

		_PlaySFX3D(audioClip, vec, minDistance, maxDistance, mixerGroup, loop);
	}

	public static void PlayBGM(AudioClip audioClip, bool bLoop = true)
	{
		if (null == audioClip) return;

		_PlayBGMInstantly(audioClip, bLoop);
	}

	public static void PlaySFX(string audioName, float delay = 0f, bool loop = false, string mixKey = "Master/SFX/Battle", float pitch = 1.0f)
	{
		if (false == string.IsNullOrEmpty(audioName))
		{
			AudioClip clip = GameResourceManager.Singleton.LoadAudioClip(audioName);
			if (null != clip)
			{
				PlaySFX(clip, delay, mixKey, loop, pitch);
			}
			else
				GameManager.Log("audioClip is null : PlaySFX(), " + audioName, "red");
		}
	}

	public static void PlaySFX3D(string audioName, Vector3 vec, float minDistance = 1, float maxDistance = 30, string mixKey = "Master/SFX/Battle", bool loop = false)
	{
		if (false == string.IsNullOrEmpty(audioName))
		{
			AudioClip clip = GameResourceManager.Singleton.LoadAudioClip(audioName);

			if (null != clip)
			{
				PlaySFX3D(clip, vec, minDistance, maxDistance, mixKey, loop);
			}
			else
				GameManager.Log("audioClip is null : PlaySFX3D(), " + audioName, "red");
		}
	}

	public static bool IsPlaySFX(string audioName)
	{
		for (int i = 0; i < _sfxPlayAoudioSources.Count; i++)
		{
			if (_sfxPlayAoudioSources[i].isPlaying)
			{
				if (_sfxPlayAoudioSources[i].clip.name.Equals(audioName))
				{
					return true;
				}
			}
		}

		return false;
	}

	public static void StopSFX(string audioName)
	{
		for (int i = 0; i < _sfxPlayAoudioSources.Count; i++)
		{
			if (_sfxPlayAoudioSources[i].name == audioName)
				_sfxPlayAoudioSources[i].Stop();
		}
	}

	public static void StopSFXAll()
	{
		for (int i = 0; i < _sfxPlayAoudioSources.Count; ++i)
		{
			_sfxPlayAoudioSources[i].loop = false;
			_sfxPlayAoudioSources[i].Stop();
		}
	}

    public static void PlayBGM(ESceneType sceneType)
    {
        if (sceneType == ESceneType.Title)
        {
            PlayBGM("BGM/TitleLobby");
			return;
        }

        int episode = GameManager.Singleton.user.CorrectEpisode;
        int chapter = GameManager.Singleton.user.CorrectChapter;
		int stage = GameDataManager.Singleton.PlayStageID;

		if ( sceneType == ESceneType.Lobby )
		{
			EpisodeTable ep = EpisodeTable.GetData(ComUtil.GetEpisodeKey(episode + 1));
            PlayBGM($"BGM/{ep.BGMLobby}");
		}
		else if ( sceneType == ESceneType.Battle )
		{
            switch (GameDataManager.Singleton.PlayMode)
            {
                case EPlayMode.ADVENTURE: PlayBGM("BGM/adventures"); break;
                case EPlayMode.DEFENCE: PlayBGM("BGM/defence"); break;
                case EPlayMode.ABYSS: PlayBGM("BGM/abyss"); break;
                case EPlayMode.INFINITE: PlayBGM("BGM/Zombie"); break;
                default:
                    PlayBGM($"BGM/{StageTable.GetData(episode + 1, chapter + 1, stage + 1).BGM}");
                    break;
            }
        }        
    }

    public static void PlayBGM(string audioName)
	{
		// if (0f >= Singleton.m_fBgmVolume) return;

		if (false == string.IsNullOrEmpty(audioName))
		{
			if (true == Singleton._asBGM.isPlaying)
			{
				if (true == Singleton.nowBGM.Equals(audioName))
					return;
				else
				{
					Singleton.StartCoroutine(ChangeBGMRoutine(audioName));
					return;
				}
			}

			AudioClip clip = GameResourceManager.Singleton.LoadAudioClip(audioName);
			if (null != clip)
			{
				Singleton.nowBGM = audioName;
				PlayBGM(clip, true);
			}
		}
	}

	private static IEnumerator ChangeBGMRoutine(string audioName)
	{
		float fTime = 1f;

		while (0f < fTime)
		{
			fTime -= Time.deltaTime;
			Singleton._asBGM.volume = fTime / 1f * Singleton.m_fBgmVolume;
			yield return YieldInstructionCache.WaitForEndOfFrame;
		}

		Singleton._asBGM.volume = 0f;
		Singleton._asBGM.Stop();

		AudioClip clip = GameResourceManager.Singleton.LoadAudioClip(audioName);

		if (null != clip)
		{
			Singleton.nowBGM = audioName;
			Singleton._asBGM.clip = clip;
			Singleton._asBGM.loop = true;
			Singleton._asBGM.volume = 0f;
			Singleton._asBGM.outputAudioMixerGroup = _dicAudioMixGroup[ComType.BGM_MIX];
			Singleton._asBGM.Play();
		}

		if (0f != Singleton.m_fBgmVolume)
		{
			fTime = 0f;

			while (1f > fTime)
			{
				fTime += Time.deltaTime;
				Singleton._asBGM.volume = fTime * Singleton.m_fBgmVolume;
				yield return YieldInstructionCache.WaitForEndOfFrame;
			}

			Singleton._asBGM.volume = Singleton.m_fBgmVolume;
		}
	}

	public static void StopBGM(bool bInstantStop = true)
	{
		if (bInstantStop == false)
		{
			Singleton.nowBGM = string.Empty;
			Singleton.StartCoroutine(_PlayBGMRoutine(null, false));
		}
		else
		{
			Singleton._asBGM.Stop();
		}
	}
}
