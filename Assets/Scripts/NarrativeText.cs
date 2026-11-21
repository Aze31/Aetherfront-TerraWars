using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class NarrativeText : MonoBehaviour
{
    public GameObject canvasObject;
    public TextMeshProUGUI textObject;
    

    public void startTypingText(string text)
    {
        //add typewriter effect later
        textObject.text = text;
    }
    void Start()
    {
        canvasObject.SetActive(false);
    }
}