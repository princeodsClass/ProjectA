using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RE : MonoBehaviour
{
    public void OnClickReset()
    {
        MenuManager.Singleton.SceneEnd();
        MenuManager.Singleton.SceneNext(ESceneType.Lobby);
        //SceneManager.LoadScene("BattleTest");
    }

    public void OnClickTitle()
    {
        MenuManager.Singleton.SceneEnd();
        MenuManager.Singleton.SceneNext(ESceneType.Title);
    }
}
