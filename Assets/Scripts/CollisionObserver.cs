using System;
using UnityEngine;

public class CollisionObserver : MonoBehaviour
{
    //checks collisions
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
        if(o1 == player || o1 == player) // to keep it cleaner, all collisions involving the olayer are organized here 
        {
            
        }
    }
        

}