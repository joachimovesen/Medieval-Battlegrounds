using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gadgets {

    public static bool canUseArrow = true;
    public static bool canUseSword = true;
    public static bool canUseGrapple = true;

    public static bool infiniteGrapple = false;
    public static bool grappleUsed = false;

    public static bool CanUse (GadgetType gadgetType)
    {
        switch(gadgetType)
        {
            case GadgetType.Arrow:
                return canUseArrow;
            case GadgetType.Sword:
                return canUseSword;
            case GadgetType.Grapple:
                return canUseGrapple; ;
                //return canUseGrapple ? (infiniteGrapple ? true : !grappleUsed) : false;
        }
        return false;
    }
}
