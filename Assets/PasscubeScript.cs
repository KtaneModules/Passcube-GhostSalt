using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Passcube;

public class PasscubeScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public GameObject Cube;
    public KMSelectable[] Arrows;
    public KMSelectable Enter;
    public TextMesh[] Text;

    private int[] PasswordLetterPositions = { 0, 1, 2, 3, 4, 5 };
    private int[] FaceIndexes = { 0, 1, 2, 3, 4, 5 };
    private string Password;
    private string Entry;
    private bool Pressing;

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 4; i++)
        {
            int x = i;
            Arrows[i].OnInteract += delegate { StartCoroutine(ArrowPress(x)); return false; };
        }
        Enter.OnInteract += delegate { StartCoroutine(EnterPress()); return false; };
        Password = Words.WordList[Rnd.Range(0, Words.WordList.Length)];
        PasswordLetterPositions.Shuffle();
        for (int i = 0; i < 6; i++)
            Text[i].text = Password[PasswordLetterPositions[i]].ToString();
        Debug.Log(Password);
        StartCoroutine(TextJitter());
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator EnterPress()
    {
        for (int i = 0; i < 3; i++)
        {
            Enter.transform.localPosition -= new Vector3(0, 0.002f, 0);
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            Enter.transform.localPosition += new Vector3(0, 0.002f, 0);
            yield return null;
        }
    }

    private IEnumerator ArrowPress(int pos)
    {
        if (!Pressing)
        {
            Pressing = true;
            int[] TempIndexes = FaceIndexes;
            for (int i = 0; i < 3; i++)
            {
                Arrows[pos].transform.localPosition -= new Vector3(0, 0.002f, 0);
                yield return null;
            }
            switch (pos)
            {
                case 0:
                    for (int i = 0; i < 15; i++)
                    {
                        Cube.transform.localRotation = Quaternion.Euler(6, 0, 0) * Cube.transform.localRotation;
                        yield return null;
                    }
                    FaceIndexes = new int[] { TempIndexes[1], TempIndexes[5], TempIndexes[2], TempIndexes[0], TempIndexes[4], TempIndexes[3] };
                    break;
                case 1:
                    for (int i = 0; i < 15; i++)
                    {
                        Cube.transform.localRotation = Quaternion.Euler(0, 0, -6) * Cube.transform.localRotation;
                        yield return null;
                    }
                    FaceIndexes = new int[] { TempIndexes[2], TempIndexes[1], TempIndexes[5], TempIndexes[3], TempIndexes[0], TempIndexes[4] };
                    break;
                case 2:
                    for (int i = 0; i < 15; i++)
                    {
                        Cube.transform.localRotation = Quaternion.Euler(-6, 0, 0) * Cube.transform.localRotation;
                        yield return null;
                    }
                    FaceIndexes = new int[] { TempIndexes[3], TempIndexes[0], TempIndexes[2], TempIndexes[5], TempIndexes[4], TempIndexes[1] };
                    break;
                default:
                    for (int i = 0; i < 15; i++)
                    {
                        Cube.transform.localRotation = Quaternion.Euler(0, 0, 6) * Cube.transform.localRotation;
                        yield return null;
                    }
                    FaceIndexes = new int[]{ TempIndexes[4], TempIndexes[1], TempIndexes[0], TempIndexes[3], TempIndexes[5], TempIndexes[2] };
                    break;
            }
            for (int i = 0; i < 3; i++)
            {
                Arrows[pos].transform.localPosition += new Vector3(0, 0.002f, 0);
                yield return null;
            }
            Pressing = false;
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
