using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class AboveQuestCard : UIDialog
{
	[SerializeField]
	TextMeshProUGUI _txtTitle, _txtGauge;

	[SerializeField]
	Slider _slGauge;

	Animator _animator;
	WaitForSecondsRealtime _fInactiveTime;
	bool _isActive = false;

    private void Awake()
    {
		_animator = GetComponent<Animator>();
		_fInactiveTime = new WaitForSecondsRealtime(GlobalTable.GetData<float>("timeQuestCardInactivate") / 1000f);
	}

    public IEnumerator Activate()
    {
		QuestTable quest = null;
		int order = -1;

		for ( int i = 0; i < GameManager.Singleton.user.m_nQuestKey.Length; i++ )
        {
			quest = QuestTable.GetData(GameManager.Singleton.user.m_nQuestKey[i]);

			if ( quest.RequireCount <= GameManager.Singleton.user.m_nQuestCount[i] &&
				 false == GameManager.Singleton.user.m_bUseQuestCard[i] &&
				 false == GameManager.Singleton.user.m_bQuestIsComplete[i] )
            {
				order = i;
				break;
            }
        }

		if ( order > -1 && false == _isActive )
        {
			_isActive = true;
			_txtTitle.text = NameTable.GetValue(quest.TitleKey);
			_txtGauge.text = $"{quest.RequireCount} / {quest.RequireCount}";
			_slGauge.value = 1f;
			_animator.SetTrigger("Active");

			GameManager.Singleton.user.m_bUseQuestCard[order] = true;

			yield return _fInactiveTime;

			InActivate();
		}
	}

	void InActivate()
    {
		_animator.SetTrigger("Inactive");
		_isActive = false;
	}
}
