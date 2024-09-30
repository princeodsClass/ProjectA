using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testdata : MonoBehaviour
{
    void SetTable()
    {
        TextAsset text = Resources.Load<TextAsset>("testdata");
        string content = text.text;
    }
}
