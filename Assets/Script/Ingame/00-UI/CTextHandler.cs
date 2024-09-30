using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 텍스트 처리자 */
public class CTextHandler : MonoBehaviour
{
	#region 변수
	private Vector3 m_stPos = Vector3.negativeInfinity;

	private Tween m_oAni = null;
	private TMP_Text m_oTMPText = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		m_oTMPText = this.GetComponentInChildren<TMP_Text>();
	}

	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oAni, null);
	}

	/** 상태를 갱신한다 */
	public void LateUpdate()
	{
		this.SetupPos();
	}

	/** 애니메이션을 시작한다 */
	public void StartAni(Vector3 a_stPos, string a_oStr, EHitType a_eHitType)
	{
		float fScale = 1.0f;
		float fAngle = Random.Range(-ComType.G_ANGLE_TEXT_ANI, ComType.G_ANGLE_TEXT_ANI);

		var stColor = Color.white;

		switch (a_eHitType)
		{
			case EHitType.WEAK: {
				fScale = 0.75f;
				stColor = Color.gray * 1.5f;

				break;
			}
			case EHitType.CRITICAL: {
				fScale = 1.5f;
				stColor = Color.yellow;

				break;
			}
		}

		m_stPos = a_stPos;
		m_stPos += Vector3.up * Random.Range(-0.5f, 0.5f);
		m_stPos += Vector3.right * Random.Range(-1.0f, 1.0f);

		m_oTMPText.text = a_oStr;
		m_oTMPText.color = stColor;
		m_oTMPText.transform.localScale = Vector3.one * (fScale * 2.0f);
		m_oTMPText.transform.localEulerAngles = new Vector3(0.0f, 0.0f, fAngle * 1.5f);

		var oSequence = DOTween.Sequence();
		oSequence.Join(m_oTMPText.transform.DOScale(fScale, ComType.G_DURATION_TEXT_ANI));
		oSequence.Join(m_oTMPText.transform.DORotate(new Vector3(0.0f, 0.0f, fAngle), ComType.G_DURATION_TEXT_ANI)).AppendInterval(ComType.G_DELTA_T_TEXT_ANI).AppendCallback(() => this.OnCompleteAni(oSequence));

		this.SetupPos();
		ComUtil.AssignVal(ref m_oAni, oSequence);
	}

	/** 위치를 설정한다 */
	private void SetupPos()
	{
		var stScreenPos = Camera.main.WorldToScreenPoint(m_stPos + ComType.G_OFFSET_BATTLE_TEXT);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MenuManager.Singleton.GetUICanvas().transform as RectTransform, stScreenPos, null, out Vector2 stAnchorPos);

		(this.transform as RectTransform).anchoredPosition = stAnchorPos;
	}

	/** 애니메이션이 완료 되었을 경우 */
	private void OnCompleteAni(Tween a_oAni)
	{
		ComUtil.AssignVal(ref a_oAni, null);
		GameResourceManager.Singleton.ReleaseObject(this.gameObject);
	}
	#endregion // 함수
}
