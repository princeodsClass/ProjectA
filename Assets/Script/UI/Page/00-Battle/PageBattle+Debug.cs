using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 전투 페이지 - 디버그 */
public partial class PageBattle : UIDialog
{
	#region 변수
	[Header("=====> Page Battle (Debug) - Etc <=====")]
	private bool m_bIsShowNPCSightRange = false;

	[Header("=====> Page Battle (Debug) - UIs <=====")]
	[SerializeField] private Text m_oValText = null;
	[SerializeField] private Text m_oWaveText = null;
	[SerializeField] private Text m_oJoystickValText = null;

	[SerializeField] private Slider m_oValSlider = null;
	[SerializeField] private Slider m_oJoystickSlider = null;

	[SerializeField] private Image m_oJoystickImgA = null;
	[SerializeField] private Image m_oJoystickImgB = null;
	[SerializeField] private Image m_oJoystickImgC = null;

	[Header("=====> Page Battle (Debug) - Game Objects <=====")]
	[SerializeField] private GameObject m_oTestUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public Text WaveText => m_oWaveText;
	#endregion // 프로퍼티

	#region 함수
#if UNITY_EDITOR
	/** NPC 시야 범위 버튼을 눌렀을 경우 */
	public void OnTouchNPCSightRangeBtn()
	{
		m_bIsShowNPCSightRange = !m_bIsShowNPCSightRange;

		foreach(var stKeyVal in this.BattleController.UnitControllerDictContainer)
		{
			// 플레이어 그룹 일 경우
			if(stKeyVal.Key == (int)ETargetGroup.PLAYER || stKeyVal.Key == (int)ETargetGroup.COMMON)
			{
				continue;
			}

			for(int i = 0; i < stKeyVal.Value.Count; ++i)
			{
				// 사망 상태 일 경우
				if(!stKeyVal.Value[i].IsSurvive)
				{
					continue;
				}

				(stKeyVal.Value[i] as NonPlayerController).EditorSightRange.SetActive(m_bIsShowNPCSightRange);
			}
		}
	}
#endif // #if UNITY_EDITOR

#if DEBUG || DEVELOPMENT_BUILD
	/** 슬라이더 값이 변경되었을 경우 */
	public void OnChangeSliderVal(float a_fVal)
	{
		m_oValText.text = $"{a_fVal:0.00}";
		m_oValSlider.SetValueWithoutNotify(a_fVal);

		GameDataManager.Singleton.MovingShootSpeedRatio = a_fVal;
	}

	/** 슬라이더 값이 변경되었을 경우 */
	public void OnChangeJoystickSliderVal(float a_fVal)
	{
		m_oJoystickValText.text = $"{a_fVal:0.00}";
		m_oJoystickSlider.SetValueWithoutNotify(a_fVal);

		GameDataManager.Singleton.MovingShootJoystickRatio = a_fVal;

		m_oJoystickImgA.rectTransform.sizeDelta = new Vector2(256.0f, 256.0f) * a_fVal;
		m_oJoystickImgB.rectTransform.sizeDelta = new Vector2(256.0f, 256.0f) * a_fVal;
		m_oJoystickImgC.rectTransform.sizeDelta = new Vector2(256.0f, 256.0f) * a_fVal;
	}

	/** 체력 회복 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsCureBtn()
	{
		this.ApplyTestUIsEffect(EEquipEffectType.CureAfterKill, 0.05f);
	}

	/** 공격력 증가 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsAttackPowerBtn()
	{
		GameManager.Singleton.user.m_nAttackPowerAfterKillMaxStack = 1.0f;
		GameManager.Singleton.user.m_nMsAttackPowerAfterKillDuration = 5000.0f;

		this.ApplyTestUIsEffect(EEquipEffectType.AttackPowerAfterKill, 0.05f);
	}

	/** 이동 속도 증가 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsMoveSpeedUpBtn()
	{
		GameManager.Singleton.user.m_nMoveSpeedRatioAfterKillMaxStack = 1.0f;
		GameManager.Singleton.user.m_nMsMoveSpeedRatioAfterKillDuration = 5000.0f;

		this.ApplyTestUIsEffect(EEquipEffectType.MoveSpeedRatioAfterKill, 0.25f);
	}

	/** 이동 속도 감소 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsMoveSpeedDownBtn()
	{
		GameManager.Singleton.user.m_nMoveSpeedRatioAfterAttackMaxStack = 1.0f;
		GameManager.Singleton.user.m_nMsMoveSpeedRatioAfterAttackDuration = 5000.0f;

		this.ApplyTestUIsEffect(EEquipEffectType.MoveSpeedRatioAfterAttack, -0.7f);
	}

	/** 출혈 효과 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsBleedingBtn()
	{
		GameManager.Singleton.user.m_nBleedingAfterAttackMaxStack = 1.0f;
		GameManager.Singleton.user.m_nMsBleedingAfterAttackInterval = 500.0f;
		GameManager.Singleton.user.m_nMsBleedingAfterAttackDuration = 5000.0f;

		this.ApplyTestUIsEffect(EEquipEffectType.BleedingAfterAttack, -0.005f);
	}

	/** 플레이어 무적 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsPlayerUntouchableBtn()
	{
		// 무적 상태 일 경우
		if (this.PlayerController.IsUntouchable)
		{
			this.PlayerController.SetUntouchableTime(0.0f);
		}
		else
		{
			this.PlayerController.StartUntouchableFX(float.MaxValue / 2.0f);
		}
	}

	/** 스킬 포인트 충전 버튼을 눌렀을 경우 */
	public void OnTouchTestUIsSkillPointChargeBtn()
	{
		this.PlayerController.IncrActiveSkillPoint(int.MaxValue / 2);
	}

	/** 입체 카메라 버튼을 눌렀을 경우 */
	public void OnTouchDimensionalCameraBtn()
	{
		this.BattleController.CameraMove.IsDimensional = !this.BattleController.CameraMove.IsDimensional;
	}

	/** 효과를 적용한다 */
	private void ApplyTestUIsEffect(EEquipEffectType a_eType, float a_fVal)
	{
		for (int i = 0; i < this.PlayerController.StandardAbilityValDicts.Length; ++i)
		{
			// 슬롯이 비어 있을 경우
			if (this.PlayerController.IsEmptySlot(i))
			{
				continue;
			}

			this.PlayerController.StandardAbilityValDicts[i].ExReplaceVal(a_eType, a_fVal);
		}

		this.PlayerController.SetupAbilityValues(true);
	}
#endif // #if DEBUG || DEVELOPMENT_BUILD
	#endregion // 함수
}
