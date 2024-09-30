using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageTitleCI : MonoBehaviour
{
    [SerializeField]
    PageTitle pageTitle;

    void Initialize()
    {
        pageTitle.CompleteCI();
    }    

}
