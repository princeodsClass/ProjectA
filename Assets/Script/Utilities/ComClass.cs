using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

[Serializable]
public class OptionData
{
	public OptionAgreements m_Aggrements = new OptionAgreements();
	public OptionSound m_Audio = new OptionSound();
	public OptionGame m_Game = new OptionGame();
	public OptionNotification m_Notice = new OptionNotification();
}

[Serializable]
public class OptionAgreements
{
	public bool bUse = true;
	public bool bShopBuy = true;
	public bool bAdds = true;
}

[Serializable]
public class OptionNotification
{
	public bool bPush = true;
	public bool bNightPush = true;
}

[Serializable]
public class OptionGame
{
	public int nQuality = 2;
	public bool bVibration = true;
	public bool bPost = true;
	public bool bAntiAliasing = false;
	public bool bViewJoystic = true;
}

[Serializable]
public class OptionSound
{
	public float fBGM_Vol = 0.5f;
	public float fSFX_Vol = 0.5f;
}

public class TooltipItemData
{
	public Sprite sprIcon = null;
	public string strName = string.Empty;
	public string strDesc = string.Empty;
	public Vector2 vSize = Vector2.zero;
}

public class CommonEffectValue
{
	public string strKey;
	public string strIcon;
	public int nType;
	public int nValue;
}

#region 추가
/** 색상 */
[System.Serializable]
public struct STColor : System.IEquatable<STColor>
{
	[JsonProperty("R")] public float m_fR;
	[JsonProperty("G")] public float m_fG;
	[JsonProperty("B")] public float m_fB;
	[JsonProperty("A")] public float m_fA;

	#region IEquatable
	/** 동일 여부를 검사한다 */
	public bool Equals(STColor a_stColor)
	{
		return Mathf.Approximately(m_fR, a_stColor.m_fR) && Mathf.Approximately(m_fG, a_stColor.m_fG) && Mathf.Approximately(m_fB, a_stColor.m_fB) && Mathf.Approximately(m_fA, a_stColor.m_fA);
	}
	#endregion // IEquatable

	#region 함수
	/** 생성자 */
	public STColor(float a_fR, float a_fG, float a_fB, float a_fA)
	{
		m_fR = a_fR;
		m_fG = a_fG;
		m_fB = a_fB;
		m_fA = a_fA;
	}

	/** 색상으로 변환한다 */
	public static implicit operator STColor(Color a_stSender)
	{
		return new STColor(a_stSender.r, a_stSender.g, a_stSender.b, a_stSender.a);
	}

	/** 색상으로 변환한다 */
	public static implicit operator Color(STColor a_stSender)
	{
		return new Color(a_stSender.m_fR, a_stSender.m_fG, a_stSender.m_fB, a_stSender.m_fA);
	}
	#endregion // 함수
}

/** 벡터 */
[System.Serializable]
public struct STVec3
{
	[JsonProperty("X")] public float m_fX;
	[JsonProperty("Y")] public float m_fY;
	[JsonProperty("Z")] public float m_fZ;

	#region 함수
	/** 생성자 */
	public STVec3(float a_fX, float a_fY, float a_fZ)
	{
		m_fX = a_fX;
		m_fY = a_fY;
		m_fZ = a_fZ;
	}

	/** 3 차원 벡터로 변환한다 */
	public static implicit operator STVec3(Vector3 a_stSender)
	{
		return new STVec3(a_stSender.x, a_stSender.y, a_stSender.z);
	}

	/** 3 차원 벡터로 변환한다 */
	public static implicit operator Vector3(STVec3 a_stSender)
	{
		return new Vector3(a_stSender.m_fX, a_stSender.m_fY, a_stSender.m_fZ);
	}
	#endregion // 함수
}

/** 피격 정보 */
public struct STHitInfo
{
	public bool m_bIsSkill;

	public int m_nDamage;
	public int m_nOriginDamage;

	public float m_fRagdollMinForce;
	public float m_fRagdollMaxForce;

	public EHitType m_eHitType;
	public EWeaponType m_eAtackWeaponType;
}

/** 광원 정보 */
[System.Serializable]
public struct STLightInfo
{
	[JsonProperty("I")] public float m_fIntensity;
	[JsonProperty("C")] public STColor m_stColor;
	[JsonProperty("TI")] public STTransInfo m_stTransInfo;

	#region 함수
	/** 생성자 */
	public STLightInfo(float a_fIntensity, STColor a_stColor, STTransInfo a_stTransInfo)
	{
		m_stColor = a_stColor;
		m_fIntensity = a_fIntensity;
		m_stTransInfo = a_stTransInfo;
	}
	#endregion // 함수
}

/** 카메라 정보 */
[System.Serializable]
public struct STCameraInfo
{
	[JsonProperty("V")] public float m_fFOV;
	[JsonProperty("H")] public float m_fHeight;
	[JsonProperty("F")] public float m_fForward;
	[JsonProperty("D")] public float m_fDistance;
	[JsonProperty("S")] public float m_fSmoothTime;

	#region 함수
	/** 생성자 */
	public STCameraInfo(float a_fFOV, float a_fHeight, float a_fForward, float a_fDistance, float a_fSmoothTime)
	{
		m_fFOV = a_fFOV;
		m_fHeight = a_fHeight;
		m_fForward = a_fForward;
		m_fDistance = a_fDistance;
		m_fSmoothTime = a_fSmoothTime;
	}
	#endregion // 함수
}

/** 트랜스 폼 정보 */
[System.Serializable]
public struct STTransInfo
{
	[JsonProperty("TP")] public STVec3 m_stPos;
	[JsonProperty("TS")] public STVec3 m_stScale;
	[JsonProperty("TR")] public STVec3 m_stRotate;

	#region 함수
	/** 생성자 */
	public STTransInfo(STVec3 a_stPos, STVec3 a_stScale, STVec3 a_stRotate)
	{
		m_stPos = a_stPos;
		m_stScale = a_stScale;
		m_stRotate = a_stRotate;
	}
	#endregion // 함수
}

/** 프리팹 정보 */
[System.Serializable]
public struct STPrefabInfo
{
	[JsonProperty("PK")] public uint m_nKey;
	[JsonProperty("PG")] public int m_nTheme;
	[JsonProperty("PN")] public string m_oName;
	[JsonProperty("RT")] public EResourceType m_eResType;

	#region 상수
	public static readonly STPrefabInfo INVALID = new STPrefabInfo()
	{
		m_eResType = EResourceType.None
	};
	#endregion // 상수

	#region 함수
	/** 생성자 */
	public STPrefabInfo(uint a_nKey, int a_nTheme, string a_oName, EResourceType a_eResType)
	{
		m_nKey = a_nKey;
		m_oName = a_oName;
		m_nTheme = a_nTheme;
		m_eResType = a_eResType;
	}
	#endregion // 함수
}

/** 식별자 정보 */
[System.Serializable]
public struct STIDInfo
{
	public int m_nStageID;
	public int m_nChapterID;
	public int m_nEpisodeID;

	#region 상수
	public static readonly STIDInfo INVALID = new STIDInfo()
	{
		m_nStageID = -1,
		m_nChapterID = -1,
		m_nEpisodeID = -1
	};
	#endregion // 상수

	#region 프로퍼티
	[JsonIgnore] public ulong UniqueStageID => ComUtil.MakeUStageID(m_nStageID, m_nChapterID, m_nEpisodeID);
	[JsonIgnore] public ulong UniqueChapterID => ComUtil.MakeUChapterID(m_nStageID, m_nChapterID);
	[JsonIgnore] public ulong UniqueEpisodeID => ComUtil.MakeUEpisodeID(m_nEpisodeID);
	#endregion // 프로퍼티

	#region 함수
	/** 생성자 */
	public STIDInfo(int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		m_nStageID = a_nStageID;
		m_nChapterID = a_nChapterID;
		m_nEpisodeID = a_nEpisodeID;
	}
	#endregion // 함수
}

/** 잠금 정보 */
public struct STLockInfo
{
	public bool m_bIsLock;
	public float m_fRemainTime;
	public float m_fMaxLockTime;
}

/** 재장전 정보 */
public struct STReloadInfo
{
	public bool m_bIsReload;
	public float m_fRemainTime;
	public float m_fMaxReloadTime;
}

/** 탄창 정보 */
public struct STMagazineInfo
{
	public int m_nSlotIdx;
	public int m_nNumBullets;
	public int m_nMaxNumBullets;
}

/** 효과 스택 정보 */
public struct STEffectStackInfo
{
	public int m_nStackCount;
	public int m_nApplyTimes;
	public int m_nMaxStackCount;

	public float m_fVal;
	public float m_fDuration;
	public float m_fInterval;
	public float m_fRemainTime;

	public bool m_bIsIgnoreStandardAbility;
	public EEquipEffectType m_eEffectType;
	public EEquipEffectType m_eAbilityEffectType;
}

/** 적용 버프 UI */
[System.Serializable]
public struct STApplyBuffUIs
{
	public TMP_Text m_oNameText;
	public TMP_Text m_oDescText;
	public TMP_Text m_oEmptyText;

	public Image m_oIconImg;
	public Image m_oFrameImg;

	public GameObject m_oDescUIs;
	public GameObject m_oEmptyUIs;
	public GameObject m_oActiveUIs;
	public GameObject m_oApplyBuffUIs;
}

/** 전투 플레이 정보 */
public struct STBattlePlayInfo {
	public int m_nRewardEXP;
	public int m_nRewardPassEXP;

	public int m_nMaxCountRevive;
	public int m_nReviveItemCount;

	public int m_nRewardGroup;
	public int m_nStandardNPCLevel;

	#region 함수
	/** 생성자 */
	public STBattlePlayInfo(int a_nRewardEXP, 
		int a_nRewardPassEXP, int a_nMaxCountRevive, int a_nReviveItemCount, int a_nRewardGroup, int a_nStandardNPCLevel) {

		m_nRewardEXP = a_nRewardEXP;
		m_nRewardPassEXP = a_nRewardPassEXP;

		m_nMaxCountRevive = a_nMaxCountRevive;
		m_nReviveItemCount = a_nReviveItemCount;

		m_nRewardGroup = a_nRewardGroup;
		m_nStandardNPCLevel = a_nStandardNPCLevel;
	}
	#endregion // 함수
}

/** 갱신 인터페이스 */
public partial interface IUpdatable
{
	/** 상태를 갱신한다 */
	public void OnUpdate(float a_fDeltaTime);

	/** 상태를 갱신한다 */
	public void OnLateUpdate(float a_fDeltaTime);

	/** 상태를 갱신한다 */
	public void OnFixedUpdate(float a_fDeltaTime);

	/** 상태를 갱신한다 */
	public void OnCustomUpdate(float a_fDeltaTime);
}

/** 풀 리스트 래퍼 */
public partial class CPoolListWrapper<T>
{
	public List<T> m_oList = new List<T>();
	public Queue<T> m_oQueue = new Queue<T>();

	#region 함수
	/** 클리어한다 */
	public void Clear()
	{
		m_oList?.Clear();
		m_oQueue?.Clear();
	}
	#endregion // 함수
}
#endregion // 추가
