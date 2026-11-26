using UnityEngine;

public class goblinFOV : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private BattlefieldManager manager;
    [SerializeField] private GameObject goblin;
    [SerializeField] private NarrativeText narrativeText;
    [SerializeField] private float x = 5f;
    private string narrative = "Goblin: 'What are you doing on my turf?'";

    private bool hasTriggered;

    private void Start()
    {
        manager = BattlefieldManager.getInstance();
    }
    private void Update()
    {
        if (hasTriggered)
        {
            return;
        }

        GameObject playerObject = player;
        GameObject goblinObject = goblin != null ? goblin : gameObject;

        if (playerObject == null || goblinObject == null || narrativeText == null)
        {
            return;
        }

        float distance = Vector2.Distance(
            playerObject.transform.position,
            goblinObject.transform.position);

        if (distance <= x)
        {
            narrativeText.startTypingText(narrative);
            hasTriggered = true;
            if(manager == null)
            {
                manager = BattlefieldManager.getInstance();
            }
            manager.setUpBattlefield(1);

        }
    }
}