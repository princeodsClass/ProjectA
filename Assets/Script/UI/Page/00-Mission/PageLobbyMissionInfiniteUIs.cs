using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 무한 미션 로비 페이지 UI */
public class PageLobbyMissionInfiniteUIs : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		// Do Something
	}

	#region 변수
	[Header("=====> Page Lobby Mission Infinite UIs - UIs <=====")]
	[SerializeField] private TMP_Text m_oOpenLVText = null;
	[SerializeField] private TMP_Text m_oNumKillsText = null;

	[SerializeField] private Image m_oTagImg = null;

	[Header("=====> Page Lobby Mission Infinite UIs - Game Objects <=====")]
	[SerializeField] private GameObject m_oCoverUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	public Image TagImg => m_oTagImg;
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.UpdateUIsState();
	}

	/** UI 상태를 갱신한다 */
	public void UpdateUIsState()
	{
		int nOpenLV = GlobalTable.GetData<int>("valueMissionZombieOpenLevel");
		m_oOpenLVText.text = $"Lv.{nOpenLV}";
		
		string oBestNumKillsStr = UIStringTable.GetValue("ui_component_mission_zombie_max_count");
		m_oNumKillsText.text = $"{oBestNumKillsStr} : {GameManager.Singleton.user.m_nMaxDefeatZombie}";

		m_oCoverUIs.SetActive(GameManager.Singleton.user.m_nLevel < nOpenLV);
	}
	#endregion // 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams()
	{
		return new STParams();
	}
	#endregion // 클래스 팩토리 함수
}
