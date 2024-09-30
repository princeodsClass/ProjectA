using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/** 전투 준비 팝업 로딩 처리자 */
public class PopupLoadingHandlerBattleReady : PopupLoadingHandler
{
	/** 매개 변수 */
	public struct STParams
	{
		public STIDInfo m_stIDInfo;
		public EMapInfoType m_eMapInfoType;
	}

	#region 변수
	private int m_nVer = 0;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
	}

	/** 로딩을 시작한다 */
	public override void StartLoading()
	{
		base.StartLoading();
		this.LoadMapVersion();
	}

	/** 맵 버전을 수신했을 경우 */
	private void OnReceiveMapVersion(JObject a_oResult, bool a_bIsSuccess)
	{
		// 맵 버전 로드에 성공했을 경우
		if (a_bIsSuccess)
		{
			m_nVer = (int)a_oResult[ComType.G_KEY_MAP_VER];
			this.Owner.SetProgress(0.25f);

			this.LoadMapInfos();
		}
		else
		{
			this.LoadMapVersion();
		}
	}

	/** 맵 정보를 수신했을 경우 */
	private void OnReceiveMapInfos(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo, bool a_bIsSuccess)
	{
		// 맵 정보 로드에 성공했을 경우
		if (a_bIsSuccess)
		{
			this.Owner.OnCompleteLoading();
		}
		else
		{
			this.LoadMapInfos();
		}
	}

	/** 맵 버전을 로드한다 */
	private void LoadMapVersion()
	{
		var oEnumerator = GameDataManager.Singleton.GetMapVersion(this.Params.m_eMapInfoType, 
			this.Params.m_stIDInfo.m_nEpisodeID + 1, this.Params.m_stIDInfo.m_nChapterID + 1, this.OnReceiveMapVersion);

		this.Owner.StartCoroutine(oEnumerator);
	}

	/** 맵 정보를 로드한다 */
	private void LoadMapInfos()
	{
		GameDataTableManager.Singleton.LoadMapInfos(this.Params.m_eMapInfoType, 
			this.Params.m_stIDInfo, m_nVer, this.OnReceiveMapInfos);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(EMapInfoType a_eMapInfoType, STIDInfo a_stIDInfo)
	{
		return new STParams()
		{
			m_stIDInfo = a_stIDInfo,
			m_eMapInfoType = a_eMapInfoType
		};
	}
	#endregion // 클래스 팩토리 함수
}
