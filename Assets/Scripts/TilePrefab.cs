using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

public class TilePrefab : MonoBehaviour
{
    //make it so each tile has its own given x and y, and let the battlefieldmanager do the rest

    public int x;
    public int y;

    public GameObject sprite;
    public GameObject tileSquare;
    private BattlefieldManager manager;

    private void Start()
    {
        //wait until start because the instance only works on awake just in case
        manager = BattlefieldManager.getInstance();
    }

    //take care of adding sprites to this tile later

    private bool isMouseOver = false; // state to check if the mosue is over for lightning and size purposes

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }
    private void OnMouseExit()
    {
        isMouseOver = false;
    }

    private void Update()
    {
        if (isMouseOver && manager != null && !manager.hasTerrain(this.x, this.y))
        {
            //increase size and change color when the mouse is over it if the tile is available
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1.5f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1.5f);
        }
    }

}