using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlotMessage : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _txtTitle, _txtMessage;

    string _sUrl;

    public void InitializeInfo(string title, string message, string url)
    {
        _txtTitle.text = title;
        _txtMessage.text = message;

        _sUrl = url;
    }

    public void OnClick()
    {
        Application.OpenURL(_sUrl);
    }
}
