using System;
using UnityEngine;

public class CollisionObserver : MonoBehaviour
{
    //this observer checks for all collisions involving the player and does things accordingly
    [SerializeField]
    public GameObject player;
    public static event Action<GameObject, GameObject> OnObjectCollision;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //need to include player condition
        Debug.Log("Collision entered with "  + collision.gameObject.name);
    }

    private void NotifyCollision(GameObject o1, GameObject o2)
    {
        if(o1 == player || o2 == player) // to keep it cleaner, all collisions involving the olayer are organized here 
        {
            if(o1.name == "Hut" || o2.name == "Hut")
            {
                //move player to the inside-hut scene
            }
            if(o1.name == "ArenaEntrance" || o2.name == "ArenaEntrance")
            {
                //change scene here?
            }
            if (o1.name.Contains("Enemy") || o2.name.Contains("Enemy"))
            {
                //
            }
        }
    }
}