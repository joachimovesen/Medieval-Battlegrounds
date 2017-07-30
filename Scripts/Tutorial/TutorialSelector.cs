using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSelector : MonoBehaviour {

    public bool autoStart = true;
    public Tutorial[] tutorials;
    int index = 0;

    private void Start()
    {
        if (autoStart)
            StartTutorial();
    } 

    public void SelectTutorial (string name)
    {
        int i = 0;
        foreach(Tutorial tut in tutorials)
        {
            if(tut.name == name)
            {
                index = i;
                break;
            }
            i++;
        }
    }

    public void StartNext ()
    {
        index++;
        if (index < tutorials.Length)
            StartTutorial();
    }

    public void StartTutorial ()
    {
        tutorials[index].StartTutorial();
    }

}
