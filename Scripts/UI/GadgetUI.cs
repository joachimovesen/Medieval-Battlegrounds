using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GadgetUI : MonoBehaviour {

    public GameObject container;
    public GadgetSlot[] gadgetSlots;

    public static GadgetType activeGadget;

    bool active = false;
    int gadgetIndex = 0;

    static GadgetUI instance;

    public static bool isVisible { get{ return instance.active; } }
    public static UnityAction<GadgetType> onWeaponChangeEvent;

    private void Awake()
    {
        onWeaponChangeEvent = null;
    }

    private void Start()
    {
        instance = this;

        activeGadget = GadgetType.Arrow;
        ActivateSlot(gadgetIndex);
    }

    private void Update()
    {
        if (!active)
            return;

        if(Input.GetKeyDown(KeyCode.A))
        {
            if(AnyUsableSlots())
                DecrementAndSelect();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if(AnyUsableSlots())
                IncrementAndSelect();  
        }
    }

    void IncrementAndSelect ()
    {
        gadgetIndex++;
        if (gadgetIndex >= gadgetSlots.Length)
            gadgetIndex = 0;

        if (gadgetSlots[gadgetIndex].canUse)
            ActivateSlot(gadgetIndex);
        else
            IncrementAndSelect();
    }

    void DecrementAndSelect()
    {
        gadgetIndex--;
        if (gadgetIndex < 0)
            gadgetIndex = gadgetSlots.Length - 1;

        if (gadgetSlots[gadgetIndex].canUse)
            ActivateSlot(gadgetIndex);
        else
            DecrementAndSelect();
    }

    bool AnyUsableSlots ()
    {
        foreach (GadgetSlot slot in gadgetSlots)
        {
            if (slot.canUse)
                return true;
        }
        return false;
    }

    void ActivateSlot (int index)
    {
        for(int i = 0; i < gadgetSlots.Length; i++)
        {
            if(i == index)
            {
                gadgetSlots[i].overlay.SetActive(true);
                activeGadget = gadgetSlots[i].gadgetType;
                if(onWeaponChangeEvent != null)
                {
                    onWeaponChangeEvent(activeGadget);
                }
            }
            else
            {
                gadgetSlots[i].overlay.SetActive(false);
            }
        }
    }

    public static void TogglePanel()
    {
        instance.togglePanel();
    }

    void togglePanel ()
    {
        if (active)
        {
            container.SetActive(false);
            active = false;
        }
        else
        {
            container.SetActive(true);
            active = true;
            for(int i = 0; i < gadgetSlots.Length; i++)
            {
                gadgetSlots[i].cross.SetActive(!gadgetSlots[i].canUse);
                gadgetSlots[i].crossText.text = "Disabled";
            }
            if(Gadgets.canUseGrapple && Gadgets.grappleUsed && !Gadgets.infiniteGrapple)
            {
                gadgetSlots[2].cross.SetActive(true);
                gadgetSlots[2].crossText.text = "Wait 1 turn";
            }
        }
    }

}

[System.Serializable]
public struct GadgetSlot
{
    public GameObject overlay;
    public GameObject cross;
    public Text crossText;
    public GadgetType gadgetType;

    public bool canUse { get { return Gadgets.CanUse(gadgetType); } }

    public GadgetSlot (GameObject overlay,GameObject cross, GadgetType gadgetType)
    {
        this.overlay = overlay;
        this.cross = cross;
        this.crossText = cross.GetComponentInChildren<Text>();
        this.gadgetType = gadgetType;
    }
}
