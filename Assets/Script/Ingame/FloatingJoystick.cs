using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
	OptionData _optionData;
	[SerializeField] private Image m_oHandleDirectionImg = null;

	protected override void Start()
	{
		base.Start();
		m_oHandleDirectionImg.gameObject.SetActive(false);

		_optionData = GameManager.Singleton.GetOption();
		SetView(_optionData.m_Game.bViewJoystic);
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
		SetView(true);
		base.OnPointerDown(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		base.OnDrag(eventData);
		m_oHandleDirectionImg.gameObject.SetActive(!handle.anchoredPosition.ExIsEquals(Vector3.zero));

		// 위치가 변경 되었을 경우
		if (!handle.anchoredPosition.ExIsEquals(Vector3.zero))
		{
			var stDirection = (Vector2)handle.anchoredPosition;
			m_oHandleDirectionImg.rectTransform.up = stDirection.normalized;
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		SetView(_optionData.m_Game.bViewJoystic);
		base.OnPointerUp(eventData);

		m_oHandleDirectionImg.gameObject.SetActive(false);
	}

	public void SetView(bool state)
	{
		background.gameObject.SetActive(state);
	}
}