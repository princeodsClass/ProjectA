using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestButton : MonoBehaviour
{
    [SerializeField] GameObject _goLock, _goNewQuest;

    private void Awake()
    {
        SetNewQuestMark(false);
    }

    public void InitializeInfo()
    {
        if ( _goLock.activeSelf ) return;

        GameManager.Singleton.StartCoroutine(SetQuest());
    }

    IEnumerator SetQuest()
    {
        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.GetQuestInfo());

        if (Array.Exists(GameManager.Singleton.user.m_nQuestKey, x => x == 0))
            yield return GameManager.Singleton.StartCoroutine(CreateQuestList());
    }

    IEnumerator CreateQuestList()
    {
        List<QuestTable> list = QuestTable.GetList(GameManager.Singleton.user.m_nLevel, 6);

        for (int i = 0; i < GameManager.Singleton.user.m_nQuestKey.Length; i++)
            yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.CreateQuest(i, list[i].PrimaryKey));

        yield return GameManager.Singleton.StartCoroutine(GameDataManager.Singleton.GetQuestInfo());

        GameManager.Singleton.user.m_bIsRecieveQuestAccouuntRewardsMiddle = false;
        GameManager.Singleton.user.m_bIsRecieveQuestAccouuntRewardsFull = false;

        Array.ForEach(GameManager.Singleton.user.m_bUseQuestCard, isUsed => isUsed = false);
        SetNewQuestMark(true);
    }

    public void SetNewQuestMark(bool state)
    {
        _goNewQuest.SetActive(state);
    }
}
