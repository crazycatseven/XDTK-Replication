using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelUIManager : MonoBehaviour
{

    public GameObject panel;
    private Button button;


    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();

        panel.SetActive(false);

        button.onClick.AddListener(TogglePanel);
    }

    private void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
    }
}
