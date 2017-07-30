using UnityEngine;
using UnityEngine.UI;

public class GameVersionUI : MonoBehaviour {

    public Text versionText;

	void Start ()
    {
        versionText.text = "Alpha " + GameSettings.gameVersion;
    }
}
