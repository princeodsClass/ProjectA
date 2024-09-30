using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/** 인스턴스 효과 UI 처리자 */
public class InstEffectUIsHandler : MonoBehaviour
{
	#region 변수
	[Header("=====> Inst Effect UIs Handler - Etc <=====")]
	private Tween m_oShowAnim = null;
	private List<EffectGroupTable> m_oEffectGroupTableList = new List<EffectGroupTable>();

	[Header("=====> Inst Effect UIs Handler - UIs <=====")]
	[SerializeField] private TMP_Text m_oNameText = null;
	[SerializeField] private TMP_Text m_oDescText = null;
	[SerializeField] private TMP_Text m_oDetailDescText = null;

	[SerializeField] private Image m_oIconImg = null;
	[SerializeField] private SoundButton m_oSelBtn = null;

	[Header("=====> Inst Effect UIs Handler - Game Objects <=====")]
	[SerializeField] private GameObject m_oIconUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public TMP_Text NameText => m_oNameText;
	public TMP_Text DescText => m_oDescText;
	public TMP_Text DetailDescText => m_oDetailDescText;

	public Image IconImg => m_oIconImg;
	public SoundButton SelBtn => m_oSelBtn;

	public GameObject IconUIs => m_oIconUIs;
	#endregion // 프로퍼티

	#region 함수
	/** 제거 되었을 경우 */
	public void OnDestroy()
	{
		ComUtil.AssignVal(ref m_oShowAnim, null);
	}

	/** 출력 애니메이션을 시작한다 */
	public void StartShowAnim(int a_nIdx, AudioClip a_oAudioClip, List<EffectGroupTable> a_oEffectGroupTableList, bool a_bIsRespin)
	{
		m_oIconUIs.SetActive(false);
		m_oNameText.gameObject.SetActive(false);
		m_oDescText.gameObject.SetActive(false);
		m_oDetailDescText.gameObject.SetActive(false);

		a_oEffectGroupTableList.ExCopyTo(m_oEffectGroupTableList, (a_oEffectGroupTable) => a_oEffectGroupTable);
		m_oEffectGroupTableList.ExShuffle();

		int nMaxTimes = 7;
		var stPos = (this.transform as RectTransform).anchoredPosition;

		float fVal = 0.0f;
		float fDuration = 2.0f;

		// 재 회전이 아닐 경우
		if (!a_bIsRespin)
		{
			(this.transform as RectTransform).anchoredPosition = new Vector2(stPos.x, stPos.y + 50.0f);
			this.gameObject.SetActive(false);
		}

		var oShowAnim = DOTween.Sequence().SetUpdate(true);
		oShowAnim.AppendInterval((a_nIdx + 1) * 0.25f);
		
		oShowAnim.AppendCallback(() => 
		{
			m_oIconUIs.SetActive(true);
			m_oNameText.gameObject.SetActive(true);

			this.gameObject.SetActive(true);
			GameAudioManager.PlaySFX(a_oAudioClip, mixerGroup: "Master/SFX/ETC");
		});

		var oJoinAnim = DOTween.Sequence().SetUpdate(true);
		oJoinAnim.Join(IconUIs.transform.DORotate(Vector3.up * 360.0f * nMaxTimes, fDuration, RotateMode.LocalAxisAdd).SetUpdate(true));

		oJoinAnim.Join(DOTween.To(() => fVal, (a_fVal) =>
		{
			fVal = a_fVal;
			int nIdx = Mathf.FloorToInt(a_fVal / 36.0f) % a_oEffectGroupTableList.Count;

			this.UpdateIconImg(a_oEffectGroupTableList[nIdx]);
		}, nMaxTimes * 360.0f, fDuration).SetUpdate(true));

		// 재 회전이 아닐 경우
		if (!a_bIsRespin)
		{
			oJoinAnim.Join((this.transform as RectTransform).DOAnchorPos(stPos, 1.5f).SetUpdate(true).SetEase(Ease.OutElastic));
		}

		oShowAnim.Append(oJoinAnim);
		oShowAnim.AppendCallback(() => this.OnCompleteShowAnim(oShowAnim));

		ComUtil.AssignVal(ref m_oShowAnim, oShowAnim);
	}

	/** 아이콘 이미지를 갱신한다 */
	private void UpdateIconImg(EffectGroupTable a_oGroupTable)
	{
		var oEffectTable = EffectTable.GetData(a_oGroupTable.EffectKey);
		m_oNameText.text = NameTable.GetValue(oEffectTable.NameKey);

		m_oIconImg.sprite = GameResourceManager.Singleton.LoadSprite(EAtlasType.Common, oEffectTable.Icon);
	}

	/** 출력 애니메이션이 완료되었을 경우 */
	private void OnCompleteShowAnim(Sequence a_oAnim)
	{
		m_oDescText.gameObject.SetActive(true);
		m_oDetailDescText.gameObject.SetActive(true);

		a_oAnim?.Kill();
		this.GetComponentInParent<PopupBattleInstEffect>()?.UpdateUIsState();
	}
	#endregion // 함수
}
