using UnityEngine;
using UnityEngine.UI;

public class AimUI : MonoBehaviour {

    public Transform target;

    Image image;
    static AimUI instance;

    private void Start()
    {
        instance = this;
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if(target != null)
        {
            image.enabled = true;
            transform.position = Camera.main.WorldToScreenPoint(target.position);
        }
        else
        {
            image.enabled = false;
        }
    }

    public static void SetTarget (Transform target)
    {
        instance.target = target;
    }

}
