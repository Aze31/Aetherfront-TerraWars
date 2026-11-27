using UnityEngine;
public class GoblinAction : MonoBehaviour
{
    public int goblinNum = 1;
    public Opponent opponent; //i hate this game
    void Update()
    {
        if(Player.GetInstance().collectionLevel == goblinNum && opponent.Health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void gameLost(int num)
    {
        
    }
}