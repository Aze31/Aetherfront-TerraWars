using UnityEngine;
using UnityEngine.UI;

public class helpUI : MonoBehaviour
{
    public Button helpButton;
    public GameObject infoPanel;

    void Start()
    {
        infoPanel.SetActive(false);
        helpButton.onClick.AddListener(helpPressed);
    }
    void helpPressed()
    {
        infoPanel.SetActive(true);
    }
    public void closePanel()
    {
        infoPanel.SetActive(false);
    }
    
}