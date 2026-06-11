using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class playerUIManager : MonoBehaviour
{
    public GameObject deckDisplayCanvas;
    public GameObject collectionDisplaycanvas;
    public Button buttonAdd;
    public Button buttonRemove;
    public Button incCopies;
    public Button decCopies;
    public int setCopies;
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
        }
    }
    public void OnButtonPressed(Button button)
    {
        if(button == incCopies)
        {
            setCopies++;
        } else if(button == decCopies)
        {
            setCopies--;
        }
        else if(button == buttonAdd){}
        else if(button == buttonRemove){}
    }
}
