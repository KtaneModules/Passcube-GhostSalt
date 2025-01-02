using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Passcube;

public class PasscubeScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public GameObject Cube;
    public KMSelectable[] ArrowSels;
    public KMSelectable EnterSel;
    public TextMesh[] Text;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        for (int i = 0; i < 4; i++)
            ArrowSels[i].OnInteract += ArrowPress(i);
        EnterSel.OnInteract += EnterPress;
        EnterSel.OnInteractEnded += EnterRelease;
    }

    private KMSelectable.OnInteractHandler ArrowPress(int i)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return false;

            return false;
        };
    }

    private bool EnterPress()
    {
        if (_moduleSolved)
            return false;
        
        return false;
    }
    
    private void EnterRelease()
    {
        if (_moduleSolved)
            return;

        return;
    }

    private IEnumerator ButtonAnimation (GameObject obj, bool isPress)
    {
        var duration = 0.2f;
        var elapsed = 0f;
        var posY = obj.transform.localPosition.y;
        var goal = isPress ? 0 : 1;
        while (elapsed < duration)
        {

            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    private IEnumerator TextJitter()
    {
        while (true)
        {
            for (int i = 0; i < 6; i++)
                Text[i].color -= new Color32(0, 0, 0, 150);
            yield return null;
            for (int i = 0; i < 6; i++)
                Text[i].color += new Color32(0, 0, 0, 150);
            yield return null;
        }
    }
}
