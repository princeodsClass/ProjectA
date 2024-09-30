using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

/** 객체 정보 */
[JsonObject]
public class CObjInfo : System.ICloneable
{
	#region 변수
	[JsonProperty("RE")] public bool m_bIsRandExclude;
	[JsonProperty("RR01")] public bool m_bIsReturnRotate01;
	[JsonProperty("RR02")] public bool m_bIsReturnRotate02;

	[JsonProperty("PreD")] public float m_fPreDelay;
	[JsonProperty("PostD")] public float m_fPostDelay;

	[JsonProperty("D01")] public float m_fDuration01;
	[JsonProperty("D02")] public float m_fDuration02;

	[JsonProperty("RA01")] public float m_fRotateAngle01;
	[JsonProperty("RA02")] public float m_fRotateAngle02;

	[JsonProperty("TI")] public STTransInfo m_stTransInfo;
	[JsonProperty("PI")] public STPrefabInfo m_stPrefabInfo;

	[JsonProperty("PT")] public EPatrolType m_ePatrolType;
	[JsonProperty("WayPIL")] public List<CWayPointInfo> m_oWayPointInfoList = new List<CWayPointInfo>();
	#endregion // 변수

	#region ICloneable
	/** 사본 객체를 생성한다 */
	public object Clone()
	{
		var oObjInfo = this.CreateCloneInst();
		this.SetupCloneInst(oObjInfo);

		oObjInfo.OnAfterDeserialize();
		return oObjInfo;
	}

	/** 사본 인스턴스를 생성한다 */
	public virtual CObjInfo CreateCloneInst()
	{
		return new CObjInfo();
	}
	#endregion // ICloneable

	#region 함수
	/** 사본 객체를 설정한다 */
	public virtual void SetupCloneInst(CObjInfo a_oObjInfo)
	{
		a_oObjInfo.m_bIsReturnRotate01 = m_bIsReturnRotate01;
		a_oObjInfo.m_bIsReturnRotate02 = m_bIsReturnRotate02;

		a_oObjInfo.m_fPreDelay = m_fPreDelay;
		a_oObjInfo.m_fPostDelay = m_fPostDelay;

		a_oObjInfo.m_fDuration01 = m_fDuration01;
		a_oObjInfo.m_fDuration02 = m_fDuration02;

		a_oObjInfo.m_fRotateAngle01 = m_fRotateAngle01;
		a_oObjInfo.m_fRotateAngle02 = m_fRotateAngle02;

		a_oObjInfo.m_stTransInfo = m_stTransInfo;
		a_oObjInfo.m_stPrefabInfo = m_stPrefabInfo;

		a_oObjInfo.m_ePatrolType = m_ePatrolType;
		a_oObjInfo.m_oWayPointInfoList = a_oObjInfo.m_oWayPointInfoList ?? new List<CWayPointInfo>();

		for (int i = 0; i < m_oWayPointInfoList.Count; ++i)
		{
			a_oObjInfo.m_oWayPointInfoList.Add(m_oWayPointInfoList[i].Clone() as CWayPointInfo);
			a_oObjInfo.m_oWayPointInfoList[i].m_oOwner = this;
		}
	}

	/** 직렬화 될 경우 */
	public virtual void OnBeforeSerialize()
	{
		// Do Something
	}

	/** 역직렬화 되었을 경우 */
	public virtual void OnAfterDeserialize()
	{
		m_oWayPointInfoList = m_oWayPointInfoList ?? new List<CWayPointInfo>();

		for (int i = 0; i < m_oWayPointInfoList.Count; ++i)
		{
			m_oWayPointInfoList[i].m_oOwner = this;
		}

		// 제어 수치 초기화가 필요 할 경우
		if(this.IsNeedResetControlValues())
		{
			m_bIsReturnRotate01 = true;
			m_bIsReturnRotate02 = true;

			m_fPreDelay = 1.5f;
			m_fPostDelay = 1.5f;

			m_fDuration01 = 0.5f;
			m_fDuration02 = 0.5f;

			m_fRotateAngle01 = -30.0f;
			m_fRotateAngle02 = 30.0f;
		}
	}

	/** 제어 수치 초기화 여부를 검사한다 */
	private bool IsNeedResetControlValues()
	{
#if DISABLE_THIS
		bool bIsNeedReset = m_bIsReturnRotate01;
		bIsNeedReset = bIsNeedReset && m_bIsReturnRotate02;
		bIsNeedReset = bIsNeedReset && m_fPreDelay.ExIsEquals(1.0f);
		bIsNeedReset = bIsNeedReset && m_fPostDelay.ExIsEquals(1.0f);
		bIsNeedReset = bIsNeedReset && m_fDuration01.ExIsEquals(0.2f);
		bIsNeedReset = bIsNeedReset && m_fDuration02.ExIsEquals(0.2f);
		bIsNeedReset = bIsNeedReset && m_fRotateAngle01.ExIsEquals(-30.0f);
		bIsNeedReset = bIsNeedReset && m_fRotateAngle02.ExIsEquals(30.0f);

		return bIsNeedReset;
#else
		return false;
#endif // #if DISABLE_THIS
	}

	/** 직렬화 될 경우 */
	[OnSerializing]
	private void OnSerializingMethod(StreamingContext a_oContext)
	{
		this.OnBeforeSerialize();
	}

	/** 역직렬화 되었을 경우 */
	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext a_oContext)
	{
		this.OnAfterDeserialize();
	}
	#endregion // 함수
}

/** 이동 지점 정보 */
[JsonObject]
public class CWayPointInfo : CObjInfo
{
	#region 변수
	[JsonIgnore] public CObjInfo m_oOwner;
	[JsonProperty("WPT")] public EWayPointType m_eWayPointType;
	#endregion // 변수

	#region ICloneable
	/** 사본 인스턴스를 생성한다 */
	public override CObjInfo CreateCloneInst()
	{
		return new CWayPointInfo();
	}
	#endregion // ICloneable

	#region 함수
	/** 사본 객체를 설정한다 */
	public override void SetupCloneInst(CObjInfo a_oObjInfo)
	{
		base.SetupCloneInst(a_oObjInfo);

		// 이동 지점 정보 일 경우
		if (a_oObjInfo is CWayPointInfo)
		{
			(a_oObjInfo as CWayPointInfo).m_eWayPointType = m_eWayPointType;
		}
	}

	/** 직렬화 될 경우 */
	public override void OnBeforeSerialize()
	{
		base.OnBeforeSerialize();
	}

	/** 역직렬화 되었을 경우 */
	public override void OnAfterDeserialize()
	{
		base.OnAfterDeserialize();
	}
	#endregion // 함수
}

/** 맵 정보 */
[System.Serializable]
public class CMapInfo : System.ICloneable
{
	#region 변수
	[JsonProperty("V")] public int m_nVer;

	[JsonProperty("D")] public bool m_bIsSetupDefValues;
	[JsonProperty("C")] public bool m_bIsSetupDefCameraValues;

	[JsonProperty("PP")] public STVec3 m_stPlayerPos;
	[JsonProperty("PR")] public STVec3 m_stPlayerRotate;
	[JsonProperty("LI")] public STLightInfo m_stLightInfo;
	[JsonProperty("CI")] public STCameraInfo m_stCameraInfo;

	[JsonProperty("MIT")] public EMapInfoType m_eMapInfoType;
	[JsonProperty("MapOIL")] public List<CObjInfo> m_oMapObjInfoList = new List<CObjInfo>();

	[JsonIgnore] public STIDInfo m_stIDInfo = STIDInfo.INVALID;
	#endregion // 변수

	#region ICloneable
	/** 사본 객체를 생성한다 */
	public virtual object Clone()
	{
		var oMapInfo = new CMapInfo();
		this.SetupCloneInst(oMapInfo);

		oMapInfo.OnAfterDeserialize();
		return oMapInfo;
	}
	#endregion // ICloneable

	#region 함수
	/** 사본 객체를 설정한다 */
	public virtual void SetupCloneInst(CMapInfo a_oMapInfo)
	{
		a_oMapInfo.m_stIDInfo = m_stIDInfo;
		a_oMapInfo.m_stPlayerPos = m_stPlayerPos;
		a_oMapInfo.m_stPlayerRotate = m_stPlayerRotate;
		a_oMapInfo.m_stLightInfo = m_stLightInfo;
		a_oMapInfo.m_stCameraInfo = m_stCameraInfo;

		a_oMapInfo.m_bIsSetupDefValues = m_bIsSetupDefValues;
		a_oMapInfo.m_bIsSetupDefCameraValues = m_bIsSetupDefCameraValues;

		for (int i = 0; i < m_oMapObjInfoList.Count; ++i)
		{
			a_oMapInfo.m_oMapObjInfoList.Add(m_oMapObjInfoList[i].Clone() as CObjInfo);
		}
	}

	/** 직렬화 될 경우 */
	public virtual void OnBeforeSerialize()
	{
		// Do Something
	}

	/** 역직렬화 되었을 경우 */
	public virtual void OnAfterDeserialize()
	{
		m_nVer = Mathf.Max(1, m_nVer);
		m_oMapObjInfoList = m_oMapObjInfoList ?? new List<CObjInfo>();

		// 카메라 기본 설정이 안되었을 경우
		if (!m_bIsSetupDefCameraValues)
		{
			m_bIsSetupDefCameraValues = true;
			m_stCameraInfo = new STCameraInfo(35.0f, 25.0f, -30.0f, 4.0f, 0.5f);
		}
	}

	/** 직렬화 될 경우 */
	[OnSerializing]
	private void OnSerializingMethod(StreamingContext a_oContext)
	{
		this.OnBeforeSerialize();
	}

	/** 역직렬화 되었을 경우 */
	[OnDeserialized]
	private void OnDeserializedMethod(StreamingContext a_oContext)
	{
		this.OnAfterDeserialize();
	}
	#endregion // 함수
}

/** 맵 정보 래퍼 */
public class CMapInfoWrapper
{
	public EMapInfoType m_eMapInfoType = EMapInfoType.NONE;
	public Dictionary<int, Dictionary<int, int>> m_oNumStagesDictContainer = new Dictionary<int, Dictionary<int, int>>();
	public Dictionary<int, Dictionary<int, Dictionary<int, CMapInfo>>> m_oMapInfoDictContainer = new Dictionary<int, Dictionary<int, Dictionary<int, CMapInfo>>>();
}

/** 맵 정보 테이블 */
public partial class CMapInfoTable : SingletonMono<CMapInfoTable>
{
	#region 프로퍼티
	public Dictionary<EMapInfoType, CMapInfoWrapper> MapInfoWrapperDict = new Dictionary<EMapInfoType, CMapInfoWrapper>();
	public string MapInfoDownloadDirPath => $"{Application.persistentDataPath}/Map/";
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		this.Reset();
	}

	/** 상태를 리셋한다 */
	public void Reset()
	{
		this.MapInfoWrapperDict.Clear();

		for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
		{
			// 맵 정보 타입이 유효하지 않을 경우
			if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
			{
				continue;
			}

			var oMapInfoWrapper = new CMapInfoWrapper()
			{
				m_eMapInfoType = (EMapInfoType)i
			};

			this.MapInfoWrapperDict.TryAdd(oMapInfoWrapper.m_eMapInfoType, oMapInfoWrapper);
		}
	}

	/** 맵 정보를 추가한다 */
	public void AddMapInfo(CMapInfo a_oMapInfo, EMapInfoType a_eMapInfoType, bool a_bIsReplace = false)
	{
		a_oMapInfo.m_eMapInfoType = (a_eMapInfoType != EMapInfoType.NONE) ? a_eMapInfoType : a_oMapInfo.m_eMapInfoType;

		var oMapInfoWrapper = this.MapInfoWrapperDict[a_oMapInfo.m_eMapInfoType];
		var oEpisodeMapInfoDictContainer = oMapInfoWrapper.m_oMapInfoDictContainer.GetValueOrDefault(a_oMapInfo.m_stIDInfo.m_nEpisodeID) ?? new Dictionary<int, Dictionary<int, CMapInfo>>();
		var oChapterMapInfoDict = oEpisodeMapInfoDictContainer.GetValueOrDefault(a_oMapInfo.m_stIDInfo.m_nChapterID) ?? new Dictionary<int, CMapInfo>();

		// 맵 정보 추가가 가능 할 경우
		if (a_bIsReplace || !oChapterMapInfoDict.ContainsKey(a_oMapInfo.m_stIDInfo.m_nStageID))
		{
			oChapterMapInfoDict.ExReplaceVal(a_oMapInfo.m_stIDInfo.m_nStageID, a_oMapInfo);
			oEpisodeMapInfoDictContainer.ExReplaceVal(a_oMapInfo.m_stIDInfo.m_nChapterID, oChapterMapInfoDict);

			oMapInfoWrapper.m_oMapInfoDictContainer.ExReplaceVal(a_oMapInfo.m_stIDInfo.m_nEpisodeID, oEpisodeMapInfoDictContainer);
		}
	}

	/** 에피소드 맵 정보를 제거한다 */
	public void RemoveEpisodeMapInfos(EMapInfoType a_eMapInfoType, int a_nEpisodeID)
	{
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];

		for (int i = a_nEpisodeID + 1; i < oMapInfoWrapper.m_oMapInfoDictContainer.Count; ++i)
		{
			for (int j = 0; j < oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
			{
				for (int k = 0; k < oMapInfoWrapper.m_oMapInfoDictContainer[i][j].Count; ++k)
				{
					oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k].m_stIDInfo.m_nEpisodeID -= 1;
				}
			}

			oMapInfoWrapper.m_oMapInfoDictContainer.ExReplaceVal(i - 1, oMapInfoWrapper.m_oMapInfoDictContainer[i]);
		}

		oMapInfoWrapper.m_oMapInfoDictContainer.ExRemoveVal(oMapInfoWrapper.m_oMapInfoDictContainer.Count - 1);
	}

	/** 챕터 맵 정보를 제거한다 */
	public void RemoveChapterMapInfos(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID)
	{
		this.TryGetEpisodeMapInfos(a_eMapInfoType, a_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> oEpisodeMapInfoDictContainer);

		for (int i = a_nChapterID + 1; i < oEpisodeMapInfoDictContainer.Count; ++i)
		{
			for (int j = 0; j < oEpisodeMapInfoDictContainer[i].Count; ++j)
			{
				oEpisodeMapInfoDictContainer[i][j].m_stIDInfo.m_nChapterID -= 1;
			}

			oEpisodeMapInfoDictContainer.ExReplaceVal(i - 1, oEpisodeMapInfoDictContainer[i]);
		}

		oEpisodeMapInfoDictContainer.ExRemoveVal(oEpisodeMapInfoDictContainer.Count - 1);

		// 맵 정보가 없을 경우
		if (oEpisodeMapInfoDictContainer.Count <= 0)
		{
			this.RemoveEpisodeMapInfos(a_eMapInfoType, a_nEpisodeID);
		}
	}

	/** 맵 정보를 제거한다 */
	public void RemoveMapInfo(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		this.TryGetChapterMapInfos(a_eMapInfoType, a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);

		for (int i = a_nStageID + 1; i < oChapterMapInfoDict.Count; ++i)
		{
			oChapterMapInfoDict[i].m_stIDInfo.m_nStageID -= 1;
			oChapterMapInfoDict.ExReplaceVal(i - 1, oChapterMapInfoDict[i]);
		}

		oChapterMapInfoDict.ExRemoveVal(oChapterMapInfoDict.Count - 1);

		// 맵 정보가 없을 경우
		if (oChapterMapInfoDict.Count <= 0)
		{
			this.RemoveChapterMapInfos(a_eMapInfoType, a_nChapterID, a_nEpisodeID);
		}
	}

	/** 에피소드 맵 정보를 이동한다 */
	public void MoveEpisodeMapInfos(EMapInfoType a_eMapInfoType, int a_nSrcID, int a_nDestID)
	{
		int nOffset = (a_nSrcID <= a_nDestID) ? 1 : -1;
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];
		var oSrcChapterMapInfoDict = oMapInfoWrapper.m_oMapInfoDictContainer[a_nSrcID];

		for (int i = a_nSrcID + nOffset; i != a_nDestID + nOffset; i += nOffset)
		{
			for (int j = 0; j < oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
			{
				for (int k = 0; k < oMapInfoWrapper.m_oMapInfoDictContainer[i][j].Count; ++k)
				{
					oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k].m_stIDInfo.m_nEpisodeID -= nOffset;
				}
			}

			oMapInfoWrapper.m_oMapInfoDictContainer.ExReplaceVal(i - nOffset, oMapInfoWrapper.m_oMapInfoDictContainer[i]);
		}

		for (int i = 0; i < oSrcChapterMapInfoDict.Count; ++i)
		{
			for (int j = 0; j < oSrcChapterMapInfoDict[i].Count; ++j)
			{
				oSrcChapterMapInfoDict[i][j].m_stIDInfo.m_nEpisodeID = a_nDestID;
			}
		}

		oMapInfoWrapper.m_oMapInfoDictContainer.ExReplaceVal(a_nDestID, oSrcChapterMapInfoDict);
	}

	/** 챕터 맵 정보를 이동한다 */
	public void MoveChapterMapInfos(EMapInfoType a_eMapInfoType, int a_nSrcID, int a_nDestID, int a_nEpisodeID)
	{
		this.TryGetEpisodeMapInfos(a_eMapInfoType, a_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> oEpisodeMapInfoDictContainer);

		int nOffset = (a_nSrcID <= a_nDestID) ? 1 : -1;
		var oSrcStageMapInfoDict = oEpisodeMapInfoDictContainer[a_nSrcID];

		oEpisodeMapInfoDictContainer.ExRemoveVal(a_nSrcID);

		for (int i = a_nSrcID + nOffset; i != a_nDestID + nOffset; i += nOffset)
		{
			for (int j = 0; j < oEpisodeMapInfoDictContainer[i].Count; ++j)
			{
				oEpisodeMapInfoDictContainer[i][j].m_stIDInfo.m_nChapterID -= nOffset;
			}

			oEpisodeMapInfoDictContainer.ExReplaceVal(i - nOffset, oEpisodeMapInfoDictContainer[i]);
		}

		for (int i = 0; i < oSrcStageMapInfoDict.Count; ++i)
		{
			oSrcStageMapInfoDict[i].m_stIDInfo.m_nChapterID = a_nDestID;
		}

		oEpisodeMapInfoDictContainer.ExReplaceVal(a_nDestID, oSrcStageMapInfoDict);
	}

	/** 맵 정보를 이동한다 */
	public void MoveMapInfo(EMapInfoType a_eMapInfoType, int a_nSrcID, int a_nDestID, int a_nChapterID, int a_nEpisodeID)
	{
		this.TryGetChapterMapInfos(a_eMapInfoType, a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);

		int nOffset = (a_nSrcID <= a_nDestID) ? 1 : -1;
		var oSrcMapInfo = oChapterMapInfoDict.GetValueOrDefault(a_nSrcID);

		oChapterMapInfoDict.ExRemoveVal(a_nSrcID);

		for (int i = a_nSrcID + nOffset; i != a_nDestID + nOffset; i += nOffset)
		{
			oChapterMapInfoDict[i].m_stIDInfo.m_nStageID -= nOffset;
			oChapterMapInfoDict.ExReplaceVal(i - nOffset, oChapterMapInfoDict[i]);
		}

		oSrcMapInfo.m_stIDInfo.m_nStageID = a_nDestID;
		oChapterMapInfoDict.ExReplaceVal(a_nDestID, oSrcMapInfo);
	}

	/** 맵 정보를 이동한다 */
	public void MoveMapInfo(EMapInfoType a_eMapInfoType, STIDInfo a_stSrcIDInfo, STIDInfo a_stDestIDInfo)
	{
		// 맵 정보가 없을 경우
		if (!this.TryGetMapInfo(a_eMapInfoType, a_stSrcIDInfo.m_nStageID, a_stSrcIDInfo.m_nChapterID, a_stSrcIDInfo.m_nEpisodeID, out CMapInfo oMapInfo))
		{
			return;
		}

		this.RemoveMapInfo(a_eMapInfoType, a_stSrcIDInfo.m_nStageID, a_stSrcIDInfo.m_nChapterID, a_stSrcIDInfo.m_nEpisodeID);

		int nNumEpisodes = this.GetNumEpisodes(a_eMapInfoType);
		int nNumChapters = this.GetNumChapters(a_eMapInfoType, a_stDestIDInfo.m_nEpisodeID);
		int nNumStages = this.GetNumStages(a_eMapInfoType, a_stDestIDInfo.m_nChapterID, a_stDestIDInfo.m_nEpisodeID);

		oMapInfo.m_stIDInfo = new STIDInfo(nNumStages, Mathf.Min(a_stDestIDInfo.m_nChapterID, nNumChapters), Mathf.Min(a_stDestIDInfo.m_nEpisodeID, nNumEpisodes));
		this.AddMapInfo(oMapInfo, a_eMapInfoType);
	}

	/** 다운로드 용 맵 정보를 복사한다 */
	public void CopyMapInfosDownload()
	{
		for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
		{
			// 맵 정보 타입이 유효하지 않을 경우
			if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
			{
				continue;
			}
			
			this.CopyMapInfosDownload((EMapInfoType)i);
		}
	}

	/** 다운로드 용 맵 정보를 복사한다 */
	public void CopyMapInfosDownload(EMapInfoType a_eMapInfoType)
	{
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];

		for (int i = 0; i < oMapInfoWrapper.m_oMapInfoDictContainer.Count; ++i)
		{
			for (int j = 0; j < oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
			{
				this.CopyMapInfosDownload(a_eMapInfoType, oMapInfoWrapper.m_oMapInfoDictContainer[i][j]);
			}
		}
	}

	/** 다운로드 용 맵 정보를 복사한다 */
	private void CopyMapInfosDownload(EMapInfoType a_eMapInfoType, Dictionary<int, CMapInfo> a_oMapInfoDict)
	{
		var stIDInfo = STIDInfo.INVALID;
		var oMapInfoDict = CCollectionPoolManager.Singleton.SpawnDict<int, CMapInfo>();

		try
		{
			foreach (var stKeyVal in a_oMapInfoDict)
			{
				string oFilePath = this.GetMapInfoLoadPath(stKeyVal.Value.m_eMapInfoType,
					stKeyVal.Value.m_stIDInfo.m_nStageID, stKeyVal.Value.m_stIDInfo.m_nChapterID, stKeyVal.Value.m_stIDInfo.m_nEpisodeID);

				stIDInfo = stKeyVal.Value.m_stIDInfo;
				oMapInfoDict.TryAdd(stKeyVal.Key, ComUtil.ReadJSONObj<CMapInfo>(oFilePath, false));
			}

			string oCopyFilePath = this.GetMapInfoCopyPath(a_eMapInfoType, 0, stIDInfo.m_nChapterID, stIDInfo.m_nEpisodeID);
			ComUtil.WriteJSONObj(oCopyFilePath, oMapInfoDict, false, true);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnDict(oMapInfoDict);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 맵 정보 수정 여부를 검사한다 */
	public bool IsModifiedMapInfo(EMapInfoType a_eMapInfoType, CMapInfo a_oMapInfo)
	{
		string oFilePath = this.GetMapInfoSavePath(a_eMapInfoType, a_oMapInfo.m_stIDInfo.m_nStageID, a_oMapInfo.m_stIDInfo.m_nChapterID, a_oMapInfo.m_stIDInfo.m_nEpisodeID);
		string oStoredJSONStr = ComUtil.ReadStr(oFilePath, false, System.Text.Encoding.Default);

		return !a_oMapInfo.ExToJSONStr(false, false).Equals(oStoredJSONStr);
	}

	/** 맵 정보 수정 여부를 검사한다 */
	public bool IsModifiedMapInfosDownload(EMapInfoType a_eMapInfoType, Dictionary<int, CMapInfo> a_oMapInfoDict)
	{
		var stIDInfo = STIDInfo.INVALID;
		var oMapInfoDict = CCollectionPoolManager.Singleton.SpawnDict<int, CMapInfo>();

		try
		{
			foreach (var stKeyVal in a_oMapInfoDict)
			{
				stIDInfo = stKeyVal.Value.m_stIDInfo;
			}

			string oFilePath = this.GetMapInfoCopyPath(a_eMapInfoType, 0, stIDInfo.m_nChapterID, stIDInfo.m_nEpisodeID);
			string oStoredJSONStr = ComUtil.ReadStr(oFilePath, false, System.Text.Encoding.Default);

			return !a_oMapInfoDict.ExToJSONStr(false, false).Equals(oStoredJSONStr);
		}
		finally
		{
			CCollectionPoolManager.Singleton.DespawnDict(oMapInfoDict);
		}
	}

	/** 에피소드 개수를 반환한다 */
	public int GetNumEpisodes(EMapInfoType a_eMapInfoType)
	{
#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oMapInfoDictContainer.Count;
#else
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oNumStagesDictContainer.Count;
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
	}

	/** 챕터 개수를 반화한다 */
	public int GetNumChapters(EMapInfoType a_eMapInfoType, int a_nEpisodeID)
	{
#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oMapInfoDictContainer[a_nEpisodeID].Count;
#else
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oNumStagesDictContainer[a_nEpisodeID].Count;
#endif // #if(UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
	}

	/** 스테이지 개수를 반환한다 */
	public int GetNumStages(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID)
	{
#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oMapInfoDictContainer[a_nEpisodeID][a_nChapterID].Count;
#else
		return this.MapInfoWrapperDict[a_eMapInfoType].m_oNumStagesDictContainer[a_nEpisodeID][a_nChapterID];
#endif // #if(UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
	}

	/** 맵 정보 경로를 반환한다 */
	public string GetMapInfoPath(EMapInfoType a_eMapInfoType, string a_oFilePath)
	{
		string oDirName = Path.GetDirectoryName(a_oFilePath).Replace("\\", "/");
		string oFileName = Path.GetFileName(a_oFilePath).Replace("\\", "/");

		// 디렉토리가 없을 경우
		if (string.IsNullOrEmpty(oDirName))
		{
			return string.Format("{0}/{1}", a_eMapInfoType, oFileName);
		}

		return string.Format("{0}/{1}/{2}", oDirName, a_eMapInfoType, oFileName);
	}

	/** 맵 정보 다운로드 경로를 반환한다 */
	public string GetMapInfoDownloadPath(string a_oFileName)
	{
		return $"{this.MapInfoDownloadDirPath}{a_oFileName}.json";
	}

	/** 맵 정보 다운로드 경로를 반환한다 */
	public string GetMapInfoDownloadPath(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo)
	{
		ulong nMapID = ComUtil.MakeUStageID(1, a_stIDInfo.m_nChapterID + 1, a_stIDInfo.m_nEpisodeID + 1);
		string oPrefix = ComUtil.GetMapInfoPrefix(a_eMapInfoType);

		string oFilePath = string.Format(ComType.G_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD, oPrefix, nMapID);
		return this.GetMapInfoPath(a_eMapInfoType, this.GetMapInfoDownloadPath(oFilePath));
	}

	/** 에피소드 맵 정보를 반환한다 */
	public Dictionary<int, Dictionary<int, CMapInfo>> GetEpisodeMapInfos(EMapInfoType a_eMapInfoType, int a_nEpisodeID)
	{
		return this.TryGetEpisodeMapInfos(a_eMapInfoType, a_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> oEpisodeMapInfoDictContainer) ? oEpisodeMapInfoDictContainer : null;
	}

	/** 챕터 맵 정보를 반환한다 */
	public Dictionary<int, CMapInfo> GetChapterMapInfos(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID)
	{
		return this.TryGetChapterMapInfos(a_eMapInfoType, a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict) ? oChapterMapInfoDict : null;
	}

	/** 맵 정보를 반환한다 */
	public CMapInfo GetMapInfo(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		return this.TryGetMapInfo(a_eMapInfoType, a_nStageID, a_nChapterID, a_nEpisodeID, out CMapInfo oMapInfo) ? oMapInfo : null;
	}

	/** 에피소드 맵 정보를 반환한다 */
	public bool TryGetEpisodeMapInfos(EMapInfoType a_eMapInfoType, int a_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> a_oOutEpisodeMapInfoDictContainer)
	{
		a_oOutEpisodeMapInfoDictContainer = this.MapInfoWrapperDict[a_eMapInfoType].m_oMapInfoDictContainer.GetValueOrDefault(a_nEpisodeID);
		return a_oOutEpisodeMapInfoDictContainer != null;
	}

	/** 챕터 맵 정보를 반환한다 */
	public bool TryGetChapterMapInfos(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID, out Dictionary<int, CMapInfo> a_oOutChapterMapInfoDict)
	{
		this.TryGetEpisodeMapInfos(a_eMapInfoType, a_nEpisodeID, out Dictionary<int, Dictionary<int, CMapInfo>> oEpisodeMapInfoDictContainer);
		a_oOutChapterMapInfoDict = oEpisodeMapInfoDictContainer?.GetValueOrDefault(a_nChapterID);

		return a_oOutChapterMapInfoDict != null;
	}

	/** 맵 정보를 반환한다 */
	public bool TryGetMapInfo(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID, out CMapInfo a_oOutMapInfo)
	{
		this.TryGetChapterMapInfos(a_eMapInfoType, a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);
		a_oOutMapInfo = oChapterMapInfoDict?.GetValueOrDefault(a_nStageID);

		return a_oOutMapInfo != null;
	}

	/** 맵 정보 테이블 로드 경로를 반환한다 */
	public string GetMapInfoTableLoadPath(EMapInfoType a_eMapInfoType)
	{
		string oFilePath = this.GetMapInfoTableSavePath(a_eMapInfoType);
		return File.Exists(oFilePath) ? oFilePath : this.GetMapInfoPath(a_eMapInfoType, ComType.G_TABLE_P_MAP_INFO);
	}

	/** 맵 정보 테이블 저장 경로를 반환한다 */
	public string GetMapInfoTableSavePath(EMapInfoType a_eMapInfoType)
	{
		return this.GetMapInfoPath(a_eMapInfoType, ComType.G_RUNTIME_TABLE_P_MAP_INFO);
	}

	/** 맵 정보 로드 경로를 반환한다 */
	private string GetMapInfoLoadPath(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		ulong nMapID = ComUtil.MakeUStageID(a_nStageID + 1, a_nChapterID + 1, a_nEpisodeID + 1);
		string oPrefix = ComUtil.GetMapInfoPrefix(a_eMapInfoType);

		string oFilePath = this.GetMapInfoSavePath(a_eMapInfoType, a_nStageID, a_nChapterID, a_nEpisodeID);
		return File.Exists(oFilePath) ? oFilePath : this.GetMapInfoPath(a_eMapInfoType, string.Format(ComType.G_DATA_P_FMT_MAP_INFO, oPrefix, nMapID));
	}

	/** 맵 정보 저장 경로를 반환한다 */
	private string GetMapInfoSavePath(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		ulong nMapID = ComUtil.MakeUStageID(a_nStageID + 1, a_nChapterID + 1, a_nEpisodeID + 1);
		string oPrefix = ComUtil.GetMapInfoPrefix(a_eMapInfoType);

		return this.GetMapInfoPath(a_eMapInfoType, string.Format(ComType.G_RUNTIME_DATA_P_FMT_MAP_INFO, oPrefix, nMapID));
	}

	/** 맵 정보 복사 경로를 반환한다 */
	private string GetMapInfoCopyPath(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		ulong nMapID = ComUtil.MakeUStageID(a_nStageID + 1, a_nChapterID + 1, a_nEpisodeID + 1);
		string oPrefix = ComUtil.GetMapInfoPrefix(a_eMapInfoType);

		return this.GetMapInfoPath(a_eMapInfoType, string.Format(ComType.G_RUNTIME_DATA_P_FMT_MAP_INFO_FOR_DOWNLOAD, oPrefix, nMapID));
	}
	#endregion // 접근 함수
}
