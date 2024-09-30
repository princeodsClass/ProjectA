using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/** 맵 정보 테이블 - 저장 */
public partial class CMapInfoTable : SingletonMono<CMapInfoTable>
{
	#region 함수
	/** 맵 정보를 로드한다 */
	public void SaveMapInfos(bool a_bIsCleanDir = false)
	{
		for (int i = (int)EMapInfoType.CAMPAIGN; i < (int)EMapInfoType.MAX_VAL; ++i)
		{
			// 맵 정보 타입이 유효하지 않을 경우
			if (!System.Enum.IsDefined(typeof(EMapInfoType), i))
			{
				continue;
			}

			this.SaveMapInfos((EMapInfoType)i, a_bIsCleanDir);
		}
	}

	/** 맵 정보 디렉토리를 정리한다 */
	private void CleanMapInfoDir(EMapInfoType a_eMapInfoType, string a_oDirPath)
	{
		var oFilePathList = Directory.GetFiles(a_oDirPath).ToList();
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];
		
		string oMapInfoTablePath = this.GetMapInfoTableSavePath(a_eMapInfoType);

		for (int i = 0; i < oMapInfoWrapper.m_oMapInfoDictContainer.Count; ++i)
		{
			for (int j = 0; j < oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
			{
				for (int k = 0; k < oMapInfoWrapper.m_oMapInfoDictContainer[i][j].Count; ++k)
				{
					var stIDInfo = oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k].m_stIDInfo;
					string oFilePath = this.GetMapInfoSavePath(a_eMapInfoType, stIDInfo.m_nStageID, stIDInfo.m_nChapterID, stIDInfo.m_nEpisodeID);

					oFilePathList.ExRemoveVal((a_oFilePath) => a_oFilePath.Equals(oMapInfoTablePath) || a_oFilePath.Contains(Path.GetFileName(oFilePath)));
				}
			}
		}

		for (int i = 0; i < oFilePathList.Count; ++i)
		{
			File.Delete(oFilePathList[i]);
		}
	}

	/** 맵 정보를 저장한다 */
	public void SaveMapInfos(EMapInfoType a_eMapInfoType, bool a_bIsCleanDir = false)
	{
		var oMapIDList = new List<ulong>();
		var oMapInfoWrapper = this.MapInfoWrapperDict[a_eMapInfoType];

		for (int i = 0; i < oMapInfoWrapper.m_oMapInfoDictContainer.Count; ++i)
		{
			for (int j = 0; j < oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
			{
				for (int k = 0; k < oMapInfoWrapper.m_oMapInfoDictContainer[i][j].Count; ++k)
				{
					oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k].m_stIDInfo = new STIDInfo(k, j, i);
					this.SaveMapInfo(oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k], oMapIDList);
				}
			}
		}

		ComUtil.WriteJSONObj(this.GetMapInfoTableSavePath(a_eMapInfoType), oMapIDList, false, false);

#if UNITY_EDITOR
		// 디렉토리 청소 모드 일 경우
		if (a_bIsCleanDir)
		{
			string oDirPath = Path.GetDirectoryName(this.GetMapInfoTableLoadPath(a_eMapInfoType)).Replace("\\", "/");
			this.CleanMapInfoDir(a_eMapInfoType, oDirPath);
		}
#endif // #if UNITY_EDITOR
	}

	/** 맵 정보를 저장한다 */
	private void SaveMapInfo(CMapInfo a_oMapInfo, List<ulong> a_oOutMapIDList)
	{
		a_oOutMapIDList.Add(a_oMapInfo.m_stIDInfo.UniqueStageID);

		// 변경 사항이 없을 경우
		if (!this.IsModifiedMapInfo(a_oMapInfo.m_eMapInfoType, a_oMapInfo))
		{
			return;
		}

		string oFilePath = this.GetMapInfoSavePath(a_oMapInfo.m_eMapInfoType, a_oMapInfo.m_stIDInfo.m_nStageID, a_oMapInfo.m_stIDInfo.m_nChapterID, a_oMapInfo.m_stIDInfo.m_nEpisodeID);
		ComUtil.WriteJSONObj(oFilePath, a_oMapInfo, false, false);
	}
	#endregion // 함수
}
