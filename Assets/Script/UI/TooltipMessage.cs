using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipMessage : TooltipMenu
{
    [SerializeField] Text m_txtMessage = null;

    public void Initialize(string strMessage)
    {
        m_txtMessage.text = strMessage;
    }
}
