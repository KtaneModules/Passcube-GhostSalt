using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class PasscubeScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public GameObject Cube;
    public KMSelectable[] ArrowSels;
    public KMSelectable[] InputSels;
    public TextMesh[] CubeText;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private Coroutine[] _buttonAnims = new Coroutine[6];

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        for (int i = 0; i < 4; i++)
        {
            ArrowSels[i].OnInteract += ArrowPress(i);
            ArrowSels[i].OnInteractEnded += ArrowRelease(i);
        }
        for (int i = 0; i < 2; i++)
        {
            InputSels[i].OnInteract += InputPress(i);
            InputSels[i].OnInteractEnded += InputRelease(i);
        }
    }

    private KMSelectable.OnInteractHandler ArrowPress(int i)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return false;
            if (_buttonAnims[i] != null)
                StopCoroutine(_buttonAnims[i]);
            _buttonAnims[i] = StartCoroutine(ButtonAnimation(ArrowSels[i].gameObject, true));
            return false;
        };
    }

    private Action ArrowRelease(int i)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return;
            if (_buttonAnims[i] != null)
                StopCoroutine(_buttonAnims[i]);
            _buttonAnims[i] = StartCoroutine(ButtonAnimation(ArrowSels[i].gameObject, false));
            return;
        };
    }

    private KMSelectable.OnInteractHandler InputPress(int i)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return false;
            if (_buttonAnims[i + 4] != null)
                StopCoroutine(_buttonAnims[i + 4]);
            _buttonAnims[i + 4] = StartCoroutine(ButtonAnimation(InputSels[i].gameObject, true));
            return false;
        };
    }

    private Action InputRelease(int i)
    {
        return delegate ()
        {
            if (_moduleSolved)
                return;
            if (_buttonAnims[i + 4] != null)
                StopCoroutine(_buttonAnims[i + 4]);
            _buttonAnims[i + 4] = StartCoroutine(ButtonAnimation(InputSels[i].gameObject, false));
            return;
        };
    }

    private IEnumerator ButtonAnimation(GameObject obj, bool isPress)
    {
        var duration = 0.075f;
        var elapsed = 0f;
        var pos = obj.transform.localPosition;
        var goal = isPress ? 0.0135f : 0.0185f;
        while (elapsed < duration)
        {
            obj.transform.localPosition = new Vector3(pos.x, Easing.InOutQuad(elapsed, pos.y, goal, duration), pos.z);
            yield return null;
            elapsed += Time.deltaTime;
        }
        obj.transform.localPosition = new Vector3(pos.x, goal, pos.z);
    }
}
