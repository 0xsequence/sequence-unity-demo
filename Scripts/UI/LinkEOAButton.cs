using System.Collections;
using System.Collections.Generic;
using Game.Scripts;
using UnityEngine;

public class LinkEOAButton : MonoBehaviour
{
    public void LinkEOA()
    {
        SequenceConnector.Instance.LinkEOA();
    }
}
