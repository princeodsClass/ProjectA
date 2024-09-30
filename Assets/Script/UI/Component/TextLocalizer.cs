using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/** 텍스트 지역화 */
public class TextLocalizer : MonoBehaviour
{
	#region 변수
	[SerializeField] private string m_oKey = string.Empty;

	private Text m_oText = null;
	private TMP_Text m_oTMPText = null;
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public void Awake()
	{
		// 텍스트를 설정한다
		m_oText = this.GetComponent<Text>();
		m_oTMPText = this.GetComponent<TMP_Text>();
	}

	/** 초기화 */
	public void Start()
	{
		this.SetupText();
	}

	/** 지역화를 리셋한다 */
	public virtual void ResetLocalize()
	{
		this.SetupText();
	}

	/** 텍스트를 설정한다 */
	private void SetupText()
	{
		// 키가 유효하지 않을 경우
		if (!m_oKey.ExIsValid())
		{
			return;
		}

		string oStr = UIStringTable.GetValue(m_oKey);

		// 텍스트가 존재 할 경우
		if (m_oText != null)
		{
			m_oText.text = oStr;
		}

		// TMP 텍스트가 존재 할 경우
		if (m_oTMPText != null)
		{
			m_oTMPText.text = oStr;
		}
	}
	#endregion // 함수

	#region 접근 함수
	/** 텍스트를 변경한다 */
	public void SetText(string a_oKey)
	{
		m_oKey = a_oKey;
		this.SetupText();
	}
	#endregion // 접근 함수
}
