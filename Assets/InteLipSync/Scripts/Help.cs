using UnityEngine;
using System.Collections;

public class Help : MonoBehaviour {

    bool isPanelVisible = false;
    public GameObject help;
    public GameObject panel;
	// Use this for initialization
	void Start () {
        panel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

   public  void onHelpClicked()
    {
        panel.SetActive(!isPanelVisible);
        isPanelVisible = !isPanelVisible;
    }
}
