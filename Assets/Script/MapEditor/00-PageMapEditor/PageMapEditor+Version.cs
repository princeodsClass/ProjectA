using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapEditor
{
	/** 맵 에디터 페이지 - 버전 */
	public partial class PageMapEditor : UIDialog
	{
		private int m_nIndicatorRefCount = 0;

		#region 함수
		/** 맵 버전을 로드했을 경우 */
		private void OnLoadMapVer(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo, int a_nVer, bool a_bIsSuccess)
		{
			m_bIsDirtyUpdateUIsState = true;
			this.SetMapVer(a_eMapInfoType, a_stIDInfo.m_nChapterID, a_stIDInfo.m_nEpisodeID, a_nVer);
		}

		/** 맵 버전을 저장했을 경우 */
		private void OnSaveMapVer(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo, int a_nVer, bool a_bIsSuccess)
		{
			// Do Something
		}

		/** 인디케이터를 출력한다 */
		private void ShowIndicator()
		{
			m_nIndicatorRefCount += 1;
			m_oIndicatorUIs.SetActive(m_nIndicatorRefCount >= 1);
		}

		/** 인디케이터를 닫는다 */
		private void CloseIndicator()
		{
			m_nIndicatorRefCount = Mathf.Max(0, m_nIndicatorRefCount - 1);
			m_oIndicatorUIs.SetActive(m_nIndicatorRefCount >= 1);
		}

		/** 맵 버전을 증가시킨다 */
		private void IncrMapVersions()
		{
			foreach (var stKeyVal in CMapInfoTable.Singleton.MapInfoWrapperDict)
			{
				this.IncrMapVersion(stKeyVal.Key, stKeyVal.Value);
			}
		}

		/** 맵 버전을 증가시킨다 */
		private void IncrMapVersion(EMapInfoType a_eMapInfoType, CMapInfoWrapper a_oMapInfoWrapper)
		{
			for (int i = 0; i < a_oMapInfoWrapper.m_oMapInfoDictContainer.Count; ++i)
			{
				for (int j = 0; j < a_oMapInfoWrapper.m_oMapInfoDictContainer[i].Count; ++j)
				{
					// 수정 된 맵 정보가 아닐 경우
					if (!CMapInfoTable.Singleton.IsModifiedMapInfosDownload(a_eMapInfoType, a_oMapInfoWrapper.m_oMapInfoDictContainer[i][j]))
					{
						continue;
					}

					int nVer = this.GetMapVer(a_eMapInfoType, j, i);

					for (int k = 0; k < a_oMapInfoWrapper.m_oMapInfoDictContainer[i][j].Count; ++k)
					{
						a_oMapInfoWrapper.m_oMapInfoDictContainer[i][j][k].m_nVer = nVer + 1;
					}
				}
			}
		}

		/** 맵 버전을 로드한다 */
		private IEnumerator CoLoadMapVersions()
		{
			foreach (var stKeyVal in CMapInfoTable.Singleton.MapInfoWrapperDict)
			{
				yield return StartCoroutine(this.CoLoadMapVersions(stKeyVal.Key));
			}
		}

		/** 맵 버전을 로드한다 */
		private IEnumerator CoLoadMapVersions(EMapInfoType a_eMapInfoType)
		{
			var oMapInfoWrapper = CMapInfoTable.Singleton.MapInfoWrapperDict[a_eMapInfoType];

			foreach (var stKeyVal in oMapInfoWrapper.m_oMapInfoDictContainer)
			{
				foreach (var stChapterKeyVal in stKeyVal.Value)
				{
					foreach (var stStageKeyVal in stChapterKeyVal.Value)
					{
						stStageKeyVal.Value.m_eMapInfoType = a_eMapInfoType;
						yield return this.CoLoadMapVer(a_eMapInfoType, stStageKeyVal.Value.m_stIDInfo, this.OnLoadMapVer);
					}
				}
			}
		}


		/** 맵 버전을 저장한다 */
		private IEnumerator CoSaveMapVersions()
		{
			foreach (var stKeyVal in CMapInfoTable.Singleton.MapInfoWrapperDict)
			{
				yield return StartCoroutine(this.CoSaveMapVersions(stKeyVal.Key));
			}
		}

		/** 맵 정보를 저장한다 */
		private IEnumerator CoSaveMapVersions(EMapInfoType a_eMapInfoType)
		{
			var oMapInfoWrapper = CMapInfoTable.Singleton.MapInfoWrapperDict[a_eMapInfoType];

			foreach (var stKeyVal in oMapInfoWrapper.m_oMapInfoDictContainer)
			{
				foreach (var stChapterKeyVal in stKeyVal.Value)
				{
					foreach (var stStageKeyVal in stChapterKeyVal.Value)
					{
						yield return this.CoSaveMapVer(a_eMapInfoType,
							stStageKeyVal.Value.m_stIDInfo, stStageKeyVal.Value.m_nVer, this.OnSaveMapVer);
					}
				}
			}
		}

		/** 맵 버전을 로드한다 */
		private IEnumerator CoLoadMapVer(EMapInfoType a_eMapInfoType,
			STIDInfo a_stIDInfo, System.Action<EMapInfoType, STIDInfo, int, bool> a_oCallback)
		{
			this.ShowIndicator();

			yield return GameDataManager.Singleton.GetMapVersion(a_eMapInfoType, a_stIDInfo.m_nEpisodeID + 1, a_stIDInfo.m_nChapterID + 1, (a_oResult, a_bIsSuccess) =>
			{
				this.CloseIndicator();
				a_oCallback?.Invoke(a_eMapInfoType, a_stIDInfo, (int)a_oResult[ComType.G_KEY_MAP_VER], a_bIsSuccess);
			});
		}

		/** 맵 버전을 저장한다 */
		private IEnumerator CoSaveMapVer(EMapInfoType a_eMapInfoType,
			STIDInfo a_stIDInfo, int a_nVer, System.Action<EMapInfoType, STIDInfo, int, bool> a_oCallback)
		{
			this.ShowIndicator();

			yield return GameDataManager.Singleton.SetMapVersion(a_eMapInfoType, a_stIDInfo.m_nEpisodeID + 1, a_stIDInfo.m_nChapterID + 1, a_nVer, (a_oResult, a_bIsSuccess) =>
			{
				this.CloseIndicator();
				a_oCallback?.Invoke(a_eMapInfoType, a_stIDInfo, (int)a_oResult[ComType.G_KEY_MAP_VER], a_bIsSuccess);
			});
		}
		#endregion // 함수

		#region 접근 함수
		/** 맵 버전을 반환한다 */
		private int GetMapVer(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID)
		{
			int nVer = 1;
			
			CMapInfoTable.Singleton.TryGetChapterMapInfos(a_eMapInfoType, 
				a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);

			foreach (var stKeyVal in oChapterMapInfoDict)
			{
				nVer = Mathf.Max(nVer, stKeyVal.Value.m_nVer);
			}

			return nVer;
		}

		/** 맵 버전을 변경한다 */
		private void SetMapVer(EMapInfoType a_eMapInfoType, int a_nChapterID, int a_nEpisodeID, int a_nVer)
		{
			CMapInfoTable.Singleton.TryGetChapterMapInfos(a_eMapInfoType,
				a_nChapterID, a_nEpisodeID, out Dictionary<int, CMapInfo> oChapterMapInfoDict);

			foreach (var stKeyVal in oChapterMapInfoDict)
			{
				stKeyVal.Value.m_nVer = a_nVer;
			}
		}
		#endregion // 접근 함수
	}
}
#endif // #if (UNITY_EDITOR || UNITY_STANDALONE) && (DEBUG || DEVELOPMENT_BUILD)
