using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 전투 제어자 - 사운드 */
public partial class BattleController : MonoBehaviour
{
	#region 변수
	[SerializeField] private List<AudioClip> m_oRunSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oWalkSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oLandSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oSwapSoundList = new List<AudioClip>();
	#endregion // 변수

	#region 함수
	/** 달리기 사운드를 재생한다 */
	public void PlayRunSound(GameObject a_oSrc)
	{
		this.PlaySound(m_oRunSoundList, a_oSrc, 15);
	}

	/** 걷기 사운드를 재생한다 */
	public void PlayWalkSound(GameObject a_oSrc)
	{
		this.PlaySound(m_oWalkSoundList, a_oSrc, 15);
	}

	/** 착지 사운드를 재생한다 */
	public void PlayLandSound(GameObject a_oSrc)
	{
		this.PlaySound(m_oLandSoundList, a_oSrc);
	}

	/** 교환 사운드를 재생한다 */
	public void PlaySwapSound(GameObject a_oSrc)
	{
		this.PlaySound(m_oSwapSoundList, a_oSrc);
	}

	/** 사운드를 재생한다 */
	public void PlaySound(AudioClip a_oSound, 
		GameObject a_oSrc, string a_oMixerGroup = "Master/SFX/Battle", float a_fPitch = 1.0f, int a_nMaxDistance = 30)
	{
		// 사운드 재생이 불가능 할 경우
		if(a_oSound == null)
		{
			return;
		}

		// 사운드 발생지가 존재 할 경우
		if (a_oSrc != null)
		{
			GameAudioManager.PlaySFX3D(a_oSound, a_oSrc.transform.position, maxDistance: a_nMaxDistance);
		}
		else
		{
			GameAudioManager.PlaySFX(a_oSound, mixerGroup: a_oMixerGroup, pitch: a_fPitch);
		}
	}

	/** 사운드를 재생한다 */
	public void PlaySound(List<AudioClip> a_oSoundList, Vector3 a_stPos)
	{
		// 사운드가 없을 경우
		if (a_oSoundList == null || a_oSoundList.Count <= 0)
		{
			return;
		}

		GameAudioManager.PlaySFX3D(this.GetRandSound(a_oSoundList), a_stPos);
	}

	/** 사운드를 재생한다 */
	public void PlaySound(List<AudioClip> a_oSoundList, GameObject a_oSrc, int a_nMaxDistance = 30)
	{
		// 사운드가 없을 경우
		if (a_oSoundList == null || a_oSoundList.Count <= 0)
		{
			return;
		}

		this.PlaySound(this.GetRandSound(a_oSoundList), a_oSrc, a_nMaxDistance: a_nMaxDistance);
	}
	#endregion // 함수

	#region 접근 함수
	/** 랜덤 사운드를 반환한다 */
	public AudioClip GetRandSound(List<AudioClip> a_oSoundList)
	{
		return a_oSoundList[Random.Range(0, a_oSoundList.Count)];
	}
	#endregion // 접근 함수
}
