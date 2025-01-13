﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class PasscubeScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public GameObject Cube;
    public KMSelectable[] ArrowSels;
    public KMSelectable[] InputSels;
    public TextMesh[] CubeText;
    public TextMesh ScreenText;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private static readonly string[] _wordList = new string[] { "ADVERB", "ATRIUM", "BOWLER", "BRIDAL", "CINEMA", "COMEDY", "DANGER", "DEPUTY", "EDITOR", "EMBRYO", "FIGURE", "FRANCS", "GASPED", "GOALIE", "HARDLY", "HURDLE", "IMPORT", "INCOME", "JACKET", "JUMBLE", "KIDNAP", "KLAXON", "LAWYER", "LENGTH", "MISERY", "MYSELF", "NATURE", "NOTICE", "OCTAVE", "ORANGE", "PLEASE", "POCKET", "QUARTZ", "QUIVER", "RESULT", "REVOTE", "SPIDER", "SWITCH", "TECHNO", "TRANCE", "UNABLE", "USEFUL", "VISAGE", "VORTEX", "WISDOM", "WRITES", "XENIAS", "XYLOSE", "YACHTS", "YIELDS", "ZEBRAS", "ZODIAC" };

    private readonly Coroutine[] _buttonAnims = new Coroutine[6];
    private bool _isCubeAnimating;
    private int[][] _map = new int[4][];
    private string _solutionWord;
    private int _currentPosition;
    private bool _inSubmissionMode;
    private string _input = "";

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

        GenerateMap();
        _currentPosition = Rnd.Range(0, 26);
        CubeText[0].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[_currentPosition].ToString();
        for (int i = 0; i < 4; i++)
            CubeText[i + 1].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[_map[i][_currentPosition]].ToString();

        Debug.LogFormat("[Passcube #{0}] Cube map: {1}", _moduleId, _map.Select(i => i.Select(j => "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[j]).Join("")).Join(", "));
        Debug.LogFormat("[Passcube #{0}] The solution word is {1}.", _moduleId, _solutionWord);
    }

    private void GenerateMap()
    {
        newMap:
        int[][] map = new int[4][];
        for (int i = 0; i < 4; i++)
            map[i] = Enumerable.Range(0, 26).ToArray().Shuffle();

        // Check if each letter maps to 4 different letters
        if (Enumerable.Range(0, 26).Select(i => new[] { map[0][i], map[1][i], map[2][i], map[3][i] }.Distinct().Count()).Any(i => i != 4))
            goto newMap;

        // Check if it's possible to go from every letter to every letter
        for (int i = 0; i < 26; i++)
            for (int j = 0; j < 26; j++)
                if (i != j && FindPath(i, j, map) == null)
                    goto newMap;

        // Check if there is exactly one valid word
        var list = new List<string>();
        for (int i = 0; i < _wordList.Length; i++)
        {
            var word = _wordList[i];
            for (int j = 0; j < 5; j++)
            {
                var path = FindPath(word[j] - 'A', word[j + 1] - 'A', map);
                if (path == null || path.Length != 1)
                    goto nextIter;
            }
            list.Add(word);
            if (list.Count > 1)
                goto newMap;
            nextIter:;
        }
        if (list.Count != 1)
            goto newMap;

        _solutionWord = list[0];
        _map = map.ToArray();
    }

    struct QueueItem
    {
        public int Letter;
        public int Parent;
        public int Direction;
        public QueueItem(int letter, int parent, int dir)
        {
            Letter = letter;
            Parent = parent;
            Direction = dir;
        }
    }

    private int[] FindPath(int start, int goal, int[][] map)
    {
        var visited = new Dictionary<int, QueueItem>();
        var q = new Queue<QueueItem>();
        q.Enqueue(new QueueItem(start, -1, 0));
        while (q.Count > 0)
        {
            var qi = q.Dequeue();
            if (visited.ContainsKey(qi.Letter))
                continue;
            visited[qi.Letter] = qi;
            if (qi.Letter == goal)
                goto done;
            q.Enqueue(new QueueItem(map[0][qi.Letter], qi.Letter, 0));
            q.Enqueue(new QueueItem(map[1][qi.Letter], qi.Letter, 1));
            q.Enqueue(new QueueItem(map[2][qi.Letter], qi.Letter, 2));
            q.Enqueue(new QueueItem(map[3][qi.Letter], qi.Letter, 3));
        }
        return null;
        done:
        var r = goal;
        var path = new List<int>();
        while (true)
        {
            var nr = visited[r];
            if (nr.Parent == -1)
                break;
            path.Add(nr.Direction);
            r = nr.Parent;
        }
        path.Reverse();
        return path.ToArray();
    }

    private KMSelectable.OnInteractHandler ArrowPress(int i)
    {
        return delegate ()
        {
            if (_buttonAnims[i] != null)
                StopCoroutine(_buttonAnims[i]);
            _buttonAnims[i] = StartCoroutine(ButtonAnimation(ArrowSels[i].gameObject, true));
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (_moduleSolved || _isCubeAnimating)
                return false;
            var oldPos = _currentPosition;
            _currentPosition = _map[i][oldPos];
            StartCoroutine(RotateCube(oldPos, _currentPosition, i));
            return false;
        };
    }

    private Action ArrowRelease(int i)
    {
        return delegate ()
        {
            if (_buttonAnims[i] != null)
                StopCoroutine(_buttonAnims[i]);
            _buttonAnims[i] = StartCoroutine(ButtonAnimation(ArrowSels[i].gameObject, false));
            if (_moduleSolved)
                return;
            return;
        };
    }

    private KMSelectable.OnInteractHandler InputPress(int i)
    {
        return delegate ()
        {
            if (_buttonAnims[i + 4] != null)
                StopCoroutine(_buttonAnims[i + 4]);
            _buttonAnims[i + 4] = StartCoroutine(ButtonAnimation(InputSels[i].gameObject, true));
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (_moduleSolved)
                return false;
            if (i == 1)
            {
                if (_inSubmissionMode)
                {
                    Debug.LogFormat("[Passcube #{0}] Pressed the submit button while already in submission mode. Strike.", _moduleId);
                    Module.HandleStrike();
                    return false;
                }
                _inSubmissionMode = true;
                _input += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[_currentPosition].ToString();
                ScreenText.text = _input;
            }
            else
            {
                _input = "";
                ScreenText.text = "";
                _inSubmissionMode = false;
                return false;
            }
            return false;
        };
    }

    private Action InputRelease(int i)
    {
        return delegate ()
        {
            if (_buttonAnims[i + 4] != null)
                StopCoroutine(_buttonAnims[i + 4]);
            _buttonAnims[i + 4] = StartCoroutine(ButtonAnimation(InputSels[i].gameObject, false));
            if (_moduleSolved)
                return;
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

    private IEnumerator RotateCube(int oldPos, int newPos, int dir)
    {
        _isCubeAnimating = true;
        if (_inSubmissionMode)
        {
            _input += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[newPos].ToString();
            ScreenText.text = _input;
        }

        var duration = 0.4f;
        var elapsed = 0f;

        var curPos = Cube.transform.localEulerAngles;
        var goal = new Vector3((dir + 1) % 2 * (dir - 1) * 45, 0, dir % 2 * (2 - dir) * 45);
        while (elapsed < duration)
        {
            Cube.transform.localEulerAngles = new Vector3(Mathf.Lerp(0, goal.x, elapsed / duration), 0, Mathf.Lerp(0, goal.z, elapsed / duration));
            for (int i = 0; i < 5; i++)
                if (i != dir + 1)
                    CubeText[i].color = new Color32(255, 255, 135, (byte)Mathf.Lerp(255, 0, elapsed / duration));
            yield return null;
            elapsed += Time.deltaTime;
        }

        CubeText[0].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[newPos].ToString();
        for (int i = 0; i < 4; i++)
            CubeText[i + 1].text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[_map[i][newPos]].ToString();

        for (int i = 0; i < 5; i++)
            CubeText[i].color = new Color32(255, 255, 135, (byte)(i == 0 ? 255 : 0));

        elapsed = 0f;
        var start = new Vector3((dir + 1) % 2 * (1 - dir) * 45, 0, dir % 2 * (dir - 2) * 45);
        while (elapsed < duration)
        {
            Cube.transform.localEulerAngles = new Vector3(Mathf.Lerp(start.x, 0, elapsed / duration), 0, Mathf.Lerp(start.z, 0, elapsed / duration));
            for (int i = 1; i < 5; i++)
                CubeText[i].color = new Color32(255, 255, 135, (byte)Mathf.Lerp(0, 255, elapsed / duration));
            yield return null;
            elapsed += Time.deltaTime;
        }
        Cube.transform.localEulerAngles = new Vector3(0, 0, 0);
        for (int i = 0; i < 5; i++)
            CubeText[i].color = new Color32(255, 255, 135, 255);
        if (_inSubmissionMode)
        {
            if (_input.Length == 6)
            {
                if (_input == _solutionWord)
                {
                    Debug.LogFormat("[Passcube #{0}] Correctly submitted {1}. Module solved.", _moduleId, _solutionWord);
                    _moduleSolved = true;
                    Module.HandlePass();
                    ScreenText.color = new Color32(0, 255, 100, 255);
                    for (int i = 0; i < 5; i++)
                        CubeText[i].color = new Color32(0, 255, 100, 255);
                    yield break;
                }
                else
                {
                    Debug.LogFormat("[Passcube #{0}] Incorrectly submitted {1}. Strike.", _moduleId, _input);
                    ScreenText.text = "";
                    _input = "";
                    _inSubmissionMode = false;
                    Module.HandleStrike();
                }
            }
        }
        _isCubeAnimating = false;
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} move u r d l [Press the up, right, down, or left arrows] | !{0} reset [Press the reset button] | !{0} start [Press the start button]";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        Match m;
        m = Regex.Match(command, @"^\s*reset|red\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            yield return new[] { InputSels[0] };
            yield break;
        }
        m = Regex.Match(command, @"^\s*start|green|submi|go\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            yield return new[] { InputSels[1] };
            yield break;
        }
        m = Regex.Match(command, @"^\s*move\s+(?<dirs>[urdl;, ]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            var str = m.Groups["dirs"].Value;
            var list = new List<int>();
            for (int i = 0; i < str.Length; i++)
            {
                int ix = "urdl;, ".IndexOf(str[i]);
                if (ix > 3)
                    continue;
                if (ix == -1)
                    yield break;
                list.Add(ix);
            }
            yield return null;
            yield return "solve";
            yield return "strike";
            for (int i = 0; i < list.Count; i++)
            {
                while (_isCubeAnimating)
                    yield return null;
                ArrowSels[list[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
                ArrowSels[list[i]].OnInteractEnded();
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (!_solutionWord.StartsWith(_input))
        {
            InputSels[0].OnInteract();
            yield return new WaitForSeconds(0.1f);
            InputSels[0].OnInteractEnded();
            yield return new WaitForSeconds(0.1f);
        }
        if (_input.Length > 0 && _solutionWord.StartsWith(_input))
            goto correctInputSoFar;
        if (_input.Length == 0)
        {
            var path = FindPath(_currentPosition, _solutionWord[0] - 'A', _map);
            for (int i = 0; i < path.Length; i++)
            {
                while (_isCubeAnimating)
                    yield return null;
                ArrowSels[path[i]].OnInteract();
                yield return new WaitForSeconds(0.1f);
                ArrowSels[path[i]].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
            }
        }
        while (_isCubeAnimating)
            yield return null;
        correctInputSoFar:;
        for (int i = _input.Length; i < 6; i++)
        {
            if (i == 0)
            {
                InputSels[1].OnInteract();
                yield return new WaitForSeconds(0.1f);
                InputSels[1].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                while (_isCubeAnimating)
                    yield return null;
                var path = FindPath(_currentPosition, _solutionWord[i] - 'A', _map).First();
                ArrowSels[path].OnInteract();
                yield return new WaitForSeconds(0.1f);
                ArrowSels[path].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
