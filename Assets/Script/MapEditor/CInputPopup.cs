using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** 입력 팝업 */
public partial class CInputPopup : MonoBehaviour {
	#region 변수
	[Header("=====> UIs <=====")]
	[SerializeField] InputField m_oStageInput = null;
	[SerializeField] InputField m_oChapterInput = null;
	[SerializeField] InputField m_oEpisodeInput = null;

	[Header("=====> Game Objects <=====")]
	[SerializeField] private GameObject m_oVContentsUIs = null;
	[SerializeField] private GameObject m_oHContentsUIs = null;
	#endregion // 변수

	#region 프로퍼티
	public InputField StageInput => m_oStageInput;
	public InputField ChapterInput => m_oChapterInput;
	public InputField EpisodeInput => m_oEpisodeInput;

	public GameObject VContentsUIs => m_oVContentsUIs;
	public GameObject HContentsUIs => m_oHContentsUIs;
	#endregion // 프로퍼티
}
