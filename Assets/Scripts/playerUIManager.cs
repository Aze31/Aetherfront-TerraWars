using UnityEngine;
using UnityEngine.InputSystem; 
public class playerUIManager : MonoBehaviour
{
    public GameObject deckDisplayCanvas;
    public GameObject collectionDisplaycanvas;
    void Start() //initialize both canvases to be not active
    {
        deckDisplayCanvas.SetActive(false);
        collectionDisplaycanvas.SetActive(false);
    }
    void Update()
    {
        if(Keyboard.current.bKey.isPressed) deckDisplayCanvas.SetActive(true);
        if(Keyboard.current.cKey.isPressed)collectionDisplaycanvas.SetActive(true);

        if(Keyboard.current.spaceKey.isPressed)
        {
            deckDisplayCanvas.SetActive(false);
            collectionDisplaycanvas.SetActive(true);
        }
    }
}
