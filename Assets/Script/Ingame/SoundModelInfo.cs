using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 사운드 모델 정보 */
public class SoundModelInfo : MonoBehaviour
{
	#region 변수
	[SerializeField] private List<AudioClip> m_oFlySoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oFireSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oReloadSoundList = new List<AudioClip>();

	[SerializeField] private List<AudioClip> m_oPassBySoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oExplosionSoundList = new List<AudioClip>();

	[SerializeField] private List<AudioClip> m_oUnitHitSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oUnitKillSoundList = new List<AudioClip>();
	[SerializeField] private List<AudioClip> m_oStructureHitSoundList = new List<AudioClip>();
	#endregion // 변수

	#region 프로퍼티
	public List<AudioClip> FlySoundList => m_oFlySoundList;
	public List<AudioClip> FireSoundList => m_oFireSoundList;
	public List<AudioClip> ReloadSoundList => m_oReloadSoundList;

	public List<AudioClip> PassBySoundList => m_oPassBySoundList;
	public List<AudioClip> ExplosionSoundList => m_oExplosionSoundList;

	public List<AudioClip> UnitHitSoundList => m_oUnitHitSoundList;
	public List<AudioClip> UnitKillSoundList => m_oUnitKillSoundList;
	public List<AudioClip> StructureHitSoundList => m_oStructureHitSoundList;
	#endregion // 프로퍼티
}
