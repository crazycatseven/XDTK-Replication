using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIManager : MonoBehaviour
{

    public GameObject settingsPanel;
    private Button settingsButton;


    // Start is called before the first frame update
    void Start()
    {
        settingsButton = GetComponent<Button>();
        
        settingsPanel.SetActive(false);

        settingsButton.onClick.AddListener(ToggleSettingsPanel);
    }

    private void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
}
