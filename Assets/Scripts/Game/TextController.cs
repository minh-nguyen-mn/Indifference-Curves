using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{

    TextMeshProUGUI currentText;

    // Start is called before the first frame update
    void Start()
    {
        currentText=GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame

    public void NewText(string t)
    {
        currentText.text = t;
    }

    public void AddText(string t)
    {
        currentText.text = currentText.text + t;
    }
}