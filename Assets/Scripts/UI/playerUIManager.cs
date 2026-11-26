using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class playerUIManager : MonoBehaviour
{
    //manages canvas screens for the player
    //make two canvas screens: one for deck editing and one for card inspection

    public GameObject deckDisplayCanvas;
    public GameObject collectionDisplaycanvas;
    [SerializeField] private helpUI helpUi;

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
            collectionDisplaycanvas.SetActive(false);
            helpUi.closePanel();
        }
    }
}
