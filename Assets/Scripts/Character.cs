using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    //give this to a character that only talks, and does so once
    [SerializeField] private GameObject player;
    [SerializeField] private float x = 2.5f;
    [SerializeField] private NarrativeText narrativeText;
    [SerializeField] private string narrative = "default - this character's speech has not been set!";
    private bool hasTriggered = false;

    private void Start()
    {
        
    }
    //interesting system: if the player goes away the trigger resets. 
    private void Update()
    {
        if(hasTriggered){
            float dist1 = Vector2.Distance(player.transform.position, gameObject.transform.position);
            if(dist1 >= x)
            {
                hasTriggered = false;
            } else {return;}
            }
        float distance = Vector2.Distance(player.transform.position, gameObject.transform.position);
        if(distance <= x)
        {
            narrativeText.startTypingText(narrative);
            hasTriggered = true;
        }
    }

}