using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

/** 맵 정보 테이블 - 로드 */
public partial class CMapInfoTable : SingletonMono<CMapInfoTable>
{
	#region 함수
	/** 맵 정보를 로드한다 */
	public Dictionary<EMapInfoType, CMapInfoWrapper> LoadMapInfos()
	{
		this.Reset();

		for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
		{
			// 맵 정보 타입이 유효하지 않을 경우
			if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
			{
				continue;
			}
			
			this.LoadMapInfos((EMapInfoType)i);
		}

		return this.MapInfoWrapperDict;
	}

	/** 맵 정보를 로드한다 */
	public Dictionary<int, Dictionary<int, Dictionary<int, CMapInfo>>> LoadMapInfos(EMapInfoType a_eMapInfoType)
	{
		return this.LoadMapInfos(a_eMapInfoType, this.GetMapInfoTableLoadPath(a_eMapInfoType));
	}

	/** 맵 정보를 로드한다 */
	public Dictionary<int, Dictionary<int, Dictionary<int, CMapInfo>>> LoadMapInfos(EMapInfoType a_eMapInfoType, string a_oFilePath)
	{
		var oMapIDList = File.Exists(a_oFilePath) ? 
			ComUtil.ReadJSONObj<List<ulong>>(a_oFilePath, false) : ComUtil.ReadJSONObjFromRes<List<ulong>>(a_oFilePath, false);
			
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];
		
		oMapInfoWrapper.m_oMapInfoDictContainer.Clear();
		oMapInfoWrapper.m_oNumStagesDictContainer.Clear();

		for (int i = 0; i < oMapIDList.Count; ++i)
		{
			var oNumEpisodeMapInfosDict = oMapInfoWrapper.m_oNumStagesDictContainer.GetValueOrDefault(oMapIDList[i].ExMapIDToEpisodeID()) ?? new Dictionary<int, int>();
			oNumEpisodeMapInfosDict.ExReplaceVal(oMapIDList[i].ExMapIDToChapterID(), oNumEpisodeMapInfosDict.GetValueOrDefault(oMapIDList[i].ExMapIDToChapterID()) + 1);

			oMapInfoWrapper.m_oNumStagesDictContainer.TryAdd(oMapIDList[i].ExMapIDToEpisodeID(), oNumEpisodeMapInfosDict);
			this.AddMapInfo(this.LoadMapInfo(a_eMapInfoType, oMapIDList[i].ExMapIDToStageID(), oMapIDList[i].ExMapIDToChapterID(), oMapIDList[i].ExMapIDToEpisodeID()), a_eMapInfoType);
		}

		return oMapInfoWrapper.m_oMapInfoDictContainer;
	}

	/** 다운로드 용 맵 정보를 로드한다 */
	public Dictionary<int, CMapInfo> LoadMapInfosDownloaded(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID)
	{
		var stIDInfo = new STIDInfo(0, a_nChapterID, a_nEpisodeID);
		string oFilePath = this.GetMapInfoDownloadPath(a_eMapInfoType, stIDInfo);
		
		// 맵 정보를 로드한다 {
		var oMapInfoDict = ComUtil.ReadJSONObj<Dictionary<int, CMapInfo>>(oFilePath, false, true);

		foreach (var stKeyVal in oMapInfoDict)
		{
			stKeyVal.Value.m_stIDInfo = new STIDInfo(stKeyVal.Key, a_nChapterID, a_nEpisodeID);
		}
		// 맵 정보를 로드한다 }

		return oMapInfoDict;
	}

	/** 맵 정보를 로드한다 */
	public CMapInfo LoadMapInfo(EMapInfoType a_eMapInfoType, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		string oFilePath = this.GetMapInfoLoadPath(a_eMapInfoType, a_nStageID, a_nChapterID, a_nEpisodeID);
		return this.LoadMapInfo(a_eMapInfoType, oFilePath, a_nStageID, a_nChapterID, a_nEpisodeID);
	}

	/** 맵 정보를 로드한다 */
	private CMapInfo LoadMapInfo(EMapInfoType a_eMapInfoType, string a_oFilePath, int a_nStageID, int a_nChapterID, int a_nEpisodeID)
	{
		var oMapInfo = File.Exists(a_oFilePath) ? ComUtil.ReadJSONObj<CMapInfo>(a_oFilePath, false) : ComUtil.ReadJSONObjFromRes<CMapInfo>(a_oFilePath, false);
		oMapInfo.m_stIDInfo = new STIDInfo(a_nStageID, a_nChapterID, a_nEpisodeID);
		oMapInfo.m_eMapInfoType = a_eMapInfoType;

		return oMapInfo;
	}
	#endregion // 함수
}
