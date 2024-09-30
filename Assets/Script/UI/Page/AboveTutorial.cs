using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Xml.Linq;
using Castle.Core.Internal;

public class AboveTutorial : UIDialog
{
	[SerializeField]
	GameObject _goObjectsRoot, _goMessage, _goButtonRoot, _goButtonSkip, _goButtonNext;

	[SerializeField]
	Transform _tFinger, _tUnmask, _tEdge, _tOutline;

	[SerializeField]
	TextMeshProUGUI _txtMassage, _txtButtonCaption, _txtPButtonCaption;

	[SerializeField]
	RectTransform[] _rt4Resize;

	Action<bool> _action;
    Tween aniMask, aniEdge;

	float timeScale = 0.35f;
	float transformScale = 5f;

	bool _isExistMessage = false;

	public void SetFinger(GameObject go, Action<bool> action, float x, float y, float delayMS = 0, float ox = 0, float oy = 0)
	{
		if (false == _goObjectsRoot.activeSelf) Activate(true);
		StartCoroutine(SetPosition(go, action, x, y, delayMS));

		_action = action;
        _goMessage.SetActive(false);

		_tFinger.gameObject.SetActive(false);
		_tUnmask.gameObject.SetActive(false);
		_tEdge.gameObject.SetActive(false);

		_tEdge.transform.localScale = _tUnmask.transform.localScale = Vector3.one;
	}

	IEnumerator SetPosition(GameObject go, Action<bool> action = null, float x = 0, float y = 0, float delayMS = 0, float ox = 0, float oy = 0)
	{
		if ( null != aniEdge || null != aniMask )
        {
			aniEdge.Kill();
			aniMask.Kill();
		}

		yield return new WaitForSeconds(delayMS / 1000f);

		_tUnmask.gameObject.SetActive(true);
		_tEdge.gameObject.SetActive(true);

		_tFinger.position = _tUnmask.position = _tEdge.position = go.transform.position;

		Vector2 tar;

		if ( x == 0 || y == 0 )
			tar = go.GetComponent<RectTransform>().sizeDelta;
		else
			tar = new Vector2(x, y);

		_tEdge.GetComponent<RectTransform>().sizeDelta = tar * transformScale;
		_tUnmask.GetComponent<RectTransform>().sizeDelta = tar * transformScale;
		_tFinger.GetComponent<RectTransform>().sizeDelta = tar;

		aniMask = _tUnmask.transform.DOScale(1 / transformScale, timeScale);
		aniEdge = _tEdge.transform.DOScale(1 / transformScale, timeScale).OnComplete(() =>
		{
			_tFinger.gameObject.SetActive(true);
			_tFinger.GetComponent<SoundButton>().onClick.RemoveAllListeners();
			_tFinger.GetComponent<SoundButton>().onClick.AddListener(() => action(true));

			if ( _isExistMessage )
			{
                _goMessage.SetActive(true);
				_isExistMessage = false;
                StartCoroutine(ResizeMessage());
            }
        });
	}

	public void SetMessage(string uiKey, int positionY, string bName = "ui_popup_button_skip",
														string pbName = "ui_popup_button_next",
														string pName = default)
	{
		// _goMessage.SetActive(true);
		_isExistMessage = true;

        _goMessage.transform.localPosition = new Vector3(0, positionY, 0); // Vector3(Screen.width / 2, positionY, 0);

        _txtMassage.text = UIStringTable.GetValue(uiKey);
		_txtButtonCaption.text = UIStringTable.GetValue(bName);
        _txtPButtonCaption.text = UIStringTable.GetValue(pbName);

        _goButtonNext.GetComponent<SoundButton>().onClick.RemoveAllListeners();
		_goButtonNext.GetComponent<SoundButton>().onClick.AddListener(() => _action(true));

        ComUtil.DestroyChildren(_tOutline);

        if ( pName.IsNullOrEmpty() )
        {
            _tOutline.gameObject.SetActive(false);
        }
        else
        {
            m_ResourceMgr.CreateObject(EResourceType.UIETC, pName, _tOutline);
            _tOutline.gameObject.SetActive(true);
        }
    }

	IEnumerator ResizeMessage()
	{
		yield return null;

        Array.ForEach(_rt4Resize, rt => LayoutRebuilder.ForceRebuildLayoutImmediate(rt));
    }

	public void Skip()
	{
		Activate(false);
	}

	public void Activate(bool state)
    {
		_goObjectsRoot.SetActive(state);

        _goMessage.SetActive(state); 
		_goButtonRoot.SetActive(state); 
		_goButtonSkip.SetActive(state);

		_tFinger.gameObject.SetActive(state);

	}
}
