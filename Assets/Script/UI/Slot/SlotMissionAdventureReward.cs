using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 탐험 미션 보상 슬롯 */
public class SlotMissionAdventureReward : MonoBehaviour
{
	/** 매개 변수 */
	public struct STParams
	{
		public uint m_nKey;
		public RewardListTable m_oRewardListTable;
	}

	#region 변수
	[SerializeField] private TMP_Text m_oNumText = null;

	[SerializeField] private Image m_oIconImg = null;
	[SerializeField] private Image m_oScrapIconImg = null;
	#endregion // 변수

	#region 프로퍼티
	public STParams Params { get; private set; }
	#endregion // 프로퍼티

	#region 함수
	/** 초기화 */
	public virtual void Init(STParams a_stParams)
	{
		this.Params = a_stParams;
		this.UpdateUIsState();
	}

	/** 클릭 되었을 경우 */
	public void OnClick()
	{
		switch (this.Params.m_nKey.ToString("X").Substring(0, 2))
		{
			case "22":
				PopupMaterial ma = MenuManager.Singleton.OpenPopup<PopupMaterial>(EUIPopup.PopupMaterial, true);
				ma.InitializeInfo(new ItemMaterial(0, this.Params.m_nKey, 0));
				break;

			case "24":
				PopupBoxNormal bo = MenuManager.Singleton.OpenPopup<PopupBoxNormal>(EUIPopup.PopupBoxNormal, true);
				bo.InitializeInfo(new ItemBox(0, this.Params.m_nKey, 0), false, false);
				break;

			case "25":
				PopupDiceReward dr = MenuManager.Singleton.OpenPopup<PopupDiceReward>(EUIPopup.PopupDiceReward, true);
				dr.Init(PopupDiceReward.MakeParams(DiceTable.GetData(this.Params.m_nKey), null));
				break;
		}
	}

	/** UI 상태를 갱신한다 */
	private void UpdateUIsState()
	{
		// 보상 테이블이 존재 할 경우
		if (this.Params.m_oRewardListTable != null)
		{
			m_oNumText.text = (this.Params.m_oRewardListTable.RewardCountMin == this.Params.m_oRewardListTable.RewardCountMax) ?
				$"{this.Params.m_oRewardListTable.RewardCountMin}" : $"{this.Params.m_oRewardListTable.RewardCountMin} ~ {this.Params.m_oRewardListTable.RewardCountMax}";
		}
		else
		{
			m_oNumText.text = "1";
		}

		m_oIconImg.sprite = this.GetIcon();

		// 부품 아이콘 설정이 가능 할 경우
		if ("22" == this.Params.m_nKey.ToString("X").Substring(0, 2))
		{
			string sd = this.Params.m_nKey.ToString("X").Substring(3, 1);
			EItemType type = (EItemType)System.Convert.ToInt32(sd, 16);

			m_oScrapIconImg.gameObject.SetActive(type == EItemType.Material || type == EItemType.MaterialG);
		}
		else
		{
			m_oScrapIconImg.gameObject.SetActive(false);
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 아이콘을 반환한다 */
	private Sprite GetIcon()
	{
		uint nKey = this.Params.m_nKey;

		switch (nKey.ToString("X").Substring(0, 2))
		{
			case "22": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, MaterialTable.GetData(nKey).Icon);
			case "24": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, BoxTable.GetData(nKey).Icon);
			case "25": return GameResourceManager.Singleton.LoadSprite(EAtlasType.Icons, DiceTable.GetData(nKey).Icon);
		}

		return null;
	}
	#endregion // 접근 함수

	#region 클래스 팩토리 함수
	/** 매개 변수를 생성한다 */
	public static STParams MakeParams(uint a_nKey, RewardListTable a_oRewardListTable)
	{
		return new STParams()
		{
			m_nKey = a_nKey,
			m_oRewardListTable = a_oRewardListTable
		};
	}
	#endregion // 클래스 팩토리 함수
}
