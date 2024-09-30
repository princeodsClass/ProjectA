using UnityEngine;
using TMPro;

public class PageBattleTutorial : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI[] _txtTutorial;

    void Awake()
    {
        SetText();
    }

    void SetText()
    {
        for( int i = 0; i < _txtTutorial.Length; i++ )
            _txtTutorial[i].text = UIStringTable.GetValue($"ui_tutorial_desc_0{i}");
    }
}
