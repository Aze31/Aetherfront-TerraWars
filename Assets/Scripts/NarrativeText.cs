using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.InputSystem;

public class NarrativeText : MonoBehaviour
{
    public GameObject canvasObject;
    public TextMeshProUGUI textObject;

    public void startTypingText(string text)
    {
        //add typewriter effect later
        textObject.text = text;
        canvasObject.SetActive(true);
    }

    void Start()
    {
        canvasObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.xKey.isPressed)
        {
            canvasObject.SetActive(false);
        }
    }
}