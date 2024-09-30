using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupSysMessage : UIDialog
{
	[SerializeField] TextMeshProUGUI _txtTitle = null;
	[SerializeField] TextMeshProUGUI _txtTopMessage = null;
	[SerializeField] TextMeshProUGUI _txtMessage = null;
	[SerializeField] TextMeshProUGUI _txtConfirm = null;
	[SerializeField] Transform _tOutline = null;

	[SerializeField] RectTransform[] _rtResize;

	protected Action m_OnCallback = null;

	private void Awake()
	{
		Initialize();
	}

    private void OnEnable()
    {
		_txtTopMessage.text = string.Empty;
		_txtMessage.text = string.Empty;
	}

    public override void Initialize()
	{
		base.Initialize();
	}

	public override void Open()
	{
		base.Open();
	}

	public override void Close()
	{
        if (null != m_OnCallback) return;
        base.Close();
	}

	public override void Escape()
	{
		Close();
	}

	public void InvokeCallback()
    {
		if (null != m_OnCallback)
		{
			m_OnCallback.Invoke();
			m_OnCallback = null;
		}
	}

	public void InitializeInfo(string strTitle, string strMessage, string strButton, Action callback = null, string goName = null)
	{
		if (!string.IsNullOrEmpty(strTitle))
		{
			if (UIStringTable.IsContainsKey(strTitle))
				_txtTitle.text = UIStringTable.GetValue(strTitle);
			else
				_txtTitle.text = strTitle;
		}

		if (!string.IsNullOrEmpty(strButton))
		{
			if (UIStringTable.IsContainsKey(strButton))
				_txtConfirm.text = UIStringTable.GetValue(strButton);
			else
				_txtConfirm.text = strButton;
		}

		if (!string.IsNullOrEmpty(strMessage))
		{
			_txtMessage.gameObject.SetActive(true);

			if (UIStringTable.IsContainsKey(strMessage))
				_txtMessage.text = UIStringTable.GetValue(strMessage);
			else
				_txtMessage.text = strMessage;
		}
		else
		{
			_txtMessage.gameObject.SetActive(false);
		}

		ComUtil.DestroyChildren(_tOutline);

		if (  goName == null)
		{
            _tOutline.gameObject.SetActive(false);
		}
		else
		{
			m_ResourceMgr.CreateObject(EResourceType.UIETC, goName, _tOutline);
            _tOutline.gameObject.SetActive(true);

            /*
			RectTransform rt = _tOutline.GetComponent<RectTransform>();

			_imgImage.gameObject.SetActive(true);
			_imgImage.sprite = image;

			float aspectRatio = (float)image.rect.width / image.rect.height;
			float tWidth = rt.rect.width;
			float tHeight = tWidth / aspectRatio;

			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tWidth);
			rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tHeight);
			*/
        }

		SetCallback(callback);

		StartCoroutine(Resize());
	}

	public void SetTopMessage(string message)
	{
		if (!string.IsNullOrEmpty(message))
		{
			if (UIStringTable.IsContainsKey(message))
				_txtTopMessage.text = UIStringTable.GetValue(message);
			else
				_txtTopMessage.text = message;
		}
	}

	/// <summary>
	/// type
	/// 0 : 연결 / 1 : 줄바꿈 / 2 : 줄 띄우기
	/// </summary>
	/// <param name="message"></param>
	/// <param name="type"></param>
	public void AddMessage(string message, int type = 0)
	{
		if (!string.IsNullOrEmpty(message))
		{
			if (UIStringTable.IsContainsKey(message))
			{
				string gap = string.Empty;

				switch (type)
				{
					case 1:
						gap = "\n";
						break;
					case 2:
						gap = "\n\n";
						break;
				}

				_txtMessage.text = _txtMessage.text + gap + UIStringTable.GetValue(message);
			}
		}
	}

	public void SetCallback(Action callback)
    {
		m_OnCallback = callback;
	}

	IEnumerator Resize()
	{
		yield return null;

		Array.ForEach(_rtResize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
	}

	public void OnClickConfirm()
	{
		Close();
	}

	#region 클래스 팩토리 함수
	/** 보상 광고 보기 실패 메세지를 출력한다 */
	public static void ShowWatchRewardAdsFailMsg() {
		string oMsg = UIStringTable.GetValue("ui_error_null_ad");

		PopupSysMessage me = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage, true);
		me.InitializeInfo("ui_hint_title", oMsg, "ui_popup_button_confirm");
	}

	/** 탐험 미션 종료 메세지를 출력한다 */
	public static void ShowMissionAdventureClearMsg()
	{
		int nResetCycle = GlobalTable.GetData<int>(ComType.G_TIME_ADVENTURE_RESET);
		GameObject.Find("PageLobby")?.GetComponentInChildren<PageLobbyMission>().ReSize();

		var stTime = System.DateTime.UtcNow;
		var stFinishTime = GameManager.Singleton.user.m_dtStartAdventure.AddMilliseconds(nResetCycle);

		var stDeltaTime = stFinishTime - stTime;
		string oDeltaTimeStr = stDeltaTime.TotalSeconds.ExIsLessEquals(0.0f) ? default(System.TimeSpan).ExGetTimeStr() : stDeltaTime.ExGetTimeStr();

		string oMsg = string.Format(UIStringTable.GetValue("ui_error_adventure_end"), oDeltaTimeStr);

		PopupSysMessage me = MenuManager.Singleton.OpenPopup<PopupSysMessage>(EUIPopup.PopupSysMessage);
		me.InitializeInfo("ui_hint_title", oMsg, "ui_popup_button_confirm");
	}
	#endregion // 클래스 팩토리 함수
}
