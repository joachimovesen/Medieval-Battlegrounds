using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mute : MonoBehaviour {

	public void Toggle()
    {
        if (AudioListener.volume > 0)
            AudioListener.volume = 0;
        else
            AudioListener.volume = 1;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            Toggle();
        }
    }
}
