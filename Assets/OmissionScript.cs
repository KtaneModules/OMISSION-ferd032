using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;
using KModkit;

public class OmissionScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMBossModule Boss;

    public KMSelectable[] Buttons;
    public GameObject[] ButtonObjs;
    public TextMesh ScreenText;
    public TextMesh[] ButtonTexts;

    public GameObject ModuleBackground;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    public class OmissionStage : IEquatable<OmissionStage>
    {
        public int? DigitA;
        public int? DigitB;
        public bool? Operator;
        public string Display;

        public OmissionStage(int? a, int? b, bool? op, string disp = "")
        {
            DigitA = a;
            DigitB = b;
            Operator = op;
            Display = disp;
        }

        public bool Equals(OmissionStage other)
        {
            return
                other != null &&
                (other.DigitA == null || DigitA == null || other.DigitA == DigitA) &&
                (other.DigitB == null || DigitB == null || other.DigitB == DigitB) &&
                (other.Operator == null || Operator == null || other.Operator == Operator);
        }
    }

    private string[] _ignoredModules;
    private int _stageCount;
    private bool _readyToAdvance;
    private int _currentSolves;
    private int _currentStage = -1;

    private Coroutine[] _buttonAnimation = new Coroutine[12];
    private static readonly string[] _emojis = new string[30] { ":)", "|=", "8|", "=(", "=|", "8]", "(:", ":]", "[8", ")=", "=[", "8[", ":(", "[:", "]8", "):", "]=", "8)", "=)", ":[", "(8", "(=", "]:", "8(", "|:", "=]", ")8", ":|", "[=", "|8" };
    private static readonly OmissionStage[][] _omissionPhrases = new OmissionStage[][]
    {
        new OmissionStage[] { new OmissionStage(null, 7, false), new OmissionStage(0, 0, null) },
        new OmissionStage[] { new OmissionStage(0, 2, null), new OmissionStage(null, 3, false) },
        new OmissionStage[] { new OmissionStage(null, 5, null), new OmissionStage(0, 6, false) },
        new OmissionStage[] { new OmissionStage(6, null, false), new OmissionStage(1, 3, null) },
        new OmissionStage[] { new OmissionStage(1, 5, null), new OmissionStage(null, 4, true) },
        new OmissionStage[] { new OmissionStage(1, 8, true), new OmissionStage(null, 3, null) },
        new OmissionStage[] { new OmissionStage(2, 2, false), new OmissionStage(4, null, null) },
        new OmissionStage[] { new OmissionStage(2, 3, null), new OmissionStage(7, null, false) },
        new OmissionStage[] { new OmissionStage(null, 6, false), new OmissionStage(2, 4, null) },
        new OmissionStage[] { new OmissionStage(null, 1, null), new OmissionStage(2, 5, true) },
        new OmissionStage[] { new OmissionStage(2, 8, null), new OmissionStage(1, null, false) },
        new OmissionStage[] { new OmissionStage(3, 0, null), new OmissionStage(null, 2, false) },
        new OmissionStage[] { new OmissionStage(3, 2, null), new OmissionStage(7, null, false) },
        new OmissionStage[] { new OmissionStage(1, null, null), new OmissionStage(3, 3, true) },
        new OmissionStage[] { new OmissionStage(3, 4, false), new OmissionStage(null, 2, null) },
        new OmissionStage[] { new OmissionStage(3, 6, null), new OmissionStage(null, 6, false) },
        new OmissionStage[] { new OmissionStage(8, null, false), new OmissionStage(3, 7, null) },
        new OmissionStage[] { new OmissionStage(2, null, null), new OmissionStage(3, 8, true) },
        new OmissionStage[] { new OmissionStage(null, 2, true), new OmissionStage(4, 0, null) },
        new OmissionStage[] { new OmissionStage(null, 4, null), new OmissionStage(4, 1, true) },
        new OmissionStage[] { new OmissionStage(null, 5, true), new OmissionStage(4, 6, null) },
        new OmissionStage[] { new OmissionStage(4, null, false), new OmissionStage(4, 9, null) },
        new OmissionStage[] { new OmissionStage(5, 3, false), new OmissionStage(3, null, null) },
        new OmissionStage[] { new OmissionStage(5, 7, true), new OmissionStage(0, null, null) },
        new OmissionStage[] { new OmissionStage(6, 2, true), new OmissionStage(4, null, null) },
        new OmissionStage[] { new OmissionStage(3, null, false), new OmissionStage(6, 3, null) },
        new OmissionStage[] { new OmissionStage(6, 6, null), new OmissionStage(6, null, false) },
        new OmissionStage[] { new OmissionStage(null, 3, true), new OmissionStage(6, 7, null) },
        new OmissionStage[] { new OmissionStage(0, null, false), new OmissionStage(7, 4, null) },
        new OmissionStage[] { new OmissionStage(7, 7, null), new OmissionStage(9, null, false) },
        new OmissionStage[] { new OmissionStage(7, 8, null), new OmissionStage(null, 7, true) },
        new OmissionStage[] { new OmissionStage(8, 1, true), new OmissionStage(null, 4, null) },
        new OmissionStage[] { new OmissionStage(2, null, null), new OmissionStage(8, 3, true) },
        new OmissionStage[] { new OmissionStage(3, null, true), new OmissionStage(8, 8, null) },
        new OmissionStage[] { new OmissionStage(5, null, null), new OmissionStage(9, 0, true) },
        new OmissionStage[] { new OmissionStage(9, 1, false), new OmissionStage(null, 3, null) },
        new OmissionStage[] { new OmissionStage(9, null, null), new OmissionStage(1, 2, null), new OmissionStage(5, null, true) },
        new OmissionStage[] { new OmissionStage(5, 9, false), new OmissionStage(null, null, null), new OmissionStage(4, null, false) },
        new OmissionStage[] { new OmissionStage(3, null, null), new OmissionStage(6, null, null), new OmissionStage(6, 5, true) },
        new OmissionStage[] { new OmissionStage(7, 2, null), new OmissionStage(7, null, null), new OmissionStage(4, null, false) },
        new OmissionStage[] { new OmissionStage(3, null, null), new OmissionStage(3, null, false), new OmissionStage(8, 2, null) },
        new OmissionStage[] { new OmissionStage(null, 5, null), new OmissionStage(4, null, null), new OmissionStage(9, 4, false) },
        new OmissionStage[] { new OmissionStage(0, 4, null), new OmissionStage(null, null, true), new OmissionStage(null, 0, null), new OmissionStage(0, 9, null) },
        new OmissionStage[] { new OmissionStage(3, null, true), new OmissionStage(0, null, null), new OmissionStage(7, null, null), new OmissionStage(6, 4, null) },
        new OmissionStage[] { new OmissionStage(null, null, true), new OmissionStage(null, null, false), new OmissionStage(null, null, true), new OmissionStage(null, null, false), new OmissionStage(null, null, true) }
    };
    private List<OmissionStage> _stageInfo = new List<OmissionStage>();
    private int _solution;
    private string _currentInput = "";
    private float _heldTime;
    private Coroutine _holdTimer;
    private bool _heldButton;

    private bool _activatedRecovery;
    private int _recoveryIx;
    private Coroutine _cycleRecovery;
    private bool _canShowCycle = true;
    private int _cycleIx;
    private bool _oopsing;
    private bool _submitted;

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        ScreenText.text = _emojis[12 + Rnd.Range(0, 3)] + _emojis[21 + Rnd.Range(0, 3)]; // 47
        for (int i = 0; i < Buttons.Length; i++)
        {
            Buttons[i].OnInteract += ButtonPress(i);
            Buttons[i].OnInteractEnded += ButtonRelease(i);
        }
        Module.OnActivate += Activate;
        StartCoroutine(Init());
    }

    private void Activate()
    {
        _readyToAdvance = true;
    }

    private KMSelectable.OnInteractHandler ButtonPress(int i)
    {
        return delegate ()
        {
            if (_buttonAnimation[i] != null)
                StopCoroutine(_buttonAnimation[i]);
            _buttonAnimation[i] = StartCoroutine(ButtonAnimation(i, true));
            if (_moduleSolved || _oopsing || _submitted)
                return false;
            if (i == 11)
            {
                if (_currentStage != _stageCount)
                {
                    if (_activatedRecovery)
                    {
                        bool a = int.TryParse(_currentInput, out _recoveryIx);
                        if (!a || (_recoveryIx - 1) >= _currentStage || _recoveryIx < 1)
                        {
                            StartCoroutine(Oops());
                            _activatedRecovery = false;
                            return false;
                        }
                        _recoveryIx--;
                        Debug.LogFormat("[OMISSION #{0}] Regenerating stage{3} {1}{2}.", _moduleId, _recoveryIx + 1, (_recoveryIx == _currentStage) ? "" : " to " + (_currentStage + 1).ToString(), (_recoveryIx == _currentStage) ? "" : "s");
                        while (_stageInfo.Count != _recoveryIx)
                            _stageInfo.RemoveAt(_stageInfo.Count - 1);
                        while (_stageInfo.Count != _currentStage + 1)
                        {
                            _stageInfo.Add(GenerateStage(_stageInfo.Count));
                            Debug.LogFormat("[OMISSION #{0}] Stage {1}: {2}{3}{4}", _moduleId, _stageInfo.Count, _stageInfo[_stageInfo.Count - 1].DigitA, _stageInfo[_stageInfo.Count - 1].DigitB, _stageInfo[_stageInfo.Count - 1].Operator == null ? "" : _stageInfo[_stageInfo.Count - 1].Operator.Value ? "+" : "-");
                        }
                        _canShowCycle = true;
                        ScreenText.text = _stageInfo[_cycleIx].Display;
                        if (_cycleRecovery != null)
                            StopCoroutine(_cycleRecovery);
                        _cycleRecovery = StartCoroutine(CycleRecovery(_recoveryIx, _currentStage + 1));
                        _currentInput = "";
                        _activatedRecovery = false;
                    }
                    else
                    {
                        _canShowCycle = false;
                        ScreenText.text = (_currentStage + 1).ToString();
                    }
                }
                else
                {
                    _heldButton = false;
                    _holdTimer = StartCoroutine(HoldTimer());
                }
            }
            else if (i == 10)
            {
                if (_currentStage != _stageCount)
                {
                    if (_activatedRecovery)
                    {
                        _canShowCycle = true;
                        _activatedRecovery = false;
                        if (_cycleRecovery != null)
                        {
                            if (_cycleIx > _currentStage)
                                ScreenText.text = "";
                            else
                                ScreenText.text = _stageInfo[_cycleIx].Display;
                        }
                        else
                            ScreenText.text = _stageInfo[_currentStage].Display;
                    }
                    else
                    {
                        _activatedRecovery = true;
                        _canShowCycle = false;
                        _currentInput = "";
                        ScreenText.text = _currentInput;
                    }
                }
                else
                {
                    _heldButton = false;
                    _holdTimer = StartCoroutine(HoldTimer());
                }
            }
            else
            {
                if (_activatedRecovery)
                {
                    _currentInput += i.ToString();
                    ScreenText.text = _currentInput;
                }
                else if (_currentStage == _stageCount)
                {
                    _currentInput += i.ToString();
                    ScreenText.text = _currentInput;
                }
            }
            return false;
        };
    }

    private Action ButtonRelease(int i)
    {
        return delegate ()
        {
            if (_buttonAnimation[i] != null)
                StopCoroutine(_buttonAnimation[i]);
            _buttonAnimation[i] = StartCoroutine(ButtonAnimation(i, false));
            if (_moduleSolved || _oopsing || _submitted)
                return;
            if (_holdTimer != null)
                StopCoroutine(_holdTimer);
            if (i == 11)
            {
                if (_currentStage != _stageCount)
                {
                    _canShowCycle = true;
                    if (_cycleRecovery != null)
                    {
                        if (_cycleIx > _currentStage)
                            ScreenText.text = "";
                        else
                            ScreenText.text = _stageInfo[_cycleIx].Display;
                    }
                    else
                        ScreenText.text = _stageInfo[_currentStage].Display;
                }
                else
                {
                    if (_heldButton)
                    {
                        _currentInput = "";
                        ScreenText.text = _currentInput;
                    }
                    else
                    {
                        SubmitInput();
                    }
                }
            }
            if (i == 10 && _currentStage == _stageCount)
            {
                if (!_heldButton)
                {
                    if (_currentInput.Length != 0 && _currentInput.Substring(0, 1) == "-")
                        _currentInput = _currentInput.Substring(1);
                    else
                        _currentInput = "-" + _currentInput;
                }
                else
                    _currentInput = "";
                ScreenText.text = _currentInput;
            }
            _heldButton = false;
        };
    }

    private IEnumerator HoldTimer()
    {
        _heldTime = 0f;
        while (_heldTime < 1f)
        {
            yield return null;
            _heldTime += Time.deltaTime;
        }
        _heldButton = true;
        yield break;
    }

    private IEnumerator CycleRecovery(int start, int end)
    {
        while (true)
        {
            for (_cycleIx = start; _cycleIx < end; _cycleIx++)
            {
                if (_canShowCycle)
                    ScreenText.text = _stageInfo[_cycleIx].Display;
                yield return new WaitForSeconds(1.5f);
            }
            if (_canShowCycle)
                ScreenText.text = "";
            yield return new WaitForSeconds(1.5f);
        }
    }

    private IEnumerator Oops()
    {
        _oopsing = true;
        _canShowCycle = false;
        ScreenText.text = "?????";
        yield return new WaitForSeconds(0.7f);
        ScreenText.fontSize = 80;
        ScreenText.text = "TRY AGAIN";
        yield return new WaitForSeconds(1.5f);
        ScreenText.fontSize = 120;
        _canShowCycle = true;
        if (_cycleRecovery != null)
        {
            if (_cycleIx > _currentStage)
                ScreenText.text = "";
            else
                ScreenText.text = _stageInfo[_cycleIx].Display;
        }
        else
            ScreenText.text = _stageInfo[_currentStage].Display;
        _oopsing = false;
    }

    private void SubmitInput()
    {
        _submitted = true;
        int input;
        bool a = int.TryParse(_currentInput, out input);
        StartCoroutine(SubmissionAnimation(a, input));
    }

    private IEnumerator Init()
    {
        yield return null;
        if (_ignoredModules == null)
            _ignoredModules = GetComponent<KMBossModule>().GetIgnoredModules("OMISSION", new string[] { "OMISSION" });
        _stageCount = BombInfo.GetSolvableModuleNames().Count(i => !_ignoredModules.Contains(i));

    }

    private void Update()
    {
        if (!_readyToAdvance || _moduleSolved)
            return;
        _currentSolves = BombInfo.GetSolvedModuleNames().Count(i => !_ignoredModules.Contains(i));
        if (_currentStage == _currentSolves)
            return;
        if (_currentSolves <= _stageCount)
            Advance();
    }

    private void Advance()
    {
        if (_cycleRecovery != null)
            StopCoroutine(_cycleRecovery);
        _currentInput = "";
        _currentStage++;
        if (_currentStage != _stageCount)
        {
            _stageInfo.Add(GenerateStage(_currentStage));
            if (_canShowCycle)
                ScreenText.text = _stageInfo[_currentStage].Display;
            Debug.LogFormat("[OMISSION #{0}] Stage {1}: {2}{3}{4}", _moduleId, _currentStage + 1, _stageInfo[_currentStage].DigitA, _stageInfo[_currentStage].DigitB, _stageInfo[_currentStage].Operator == null ? "" : _stageInfo[_currentStage].Operator.Value ? "+" : "-");
        }
        else
        {
            if (_canShowCycle)
                ScreenText.text = "";
            _solution = CalculateOmission();
            Debug.LogFormat("[OMISSION #{0}] Solution: {1}.", _moduleId, _solution);
        }
    }

    private OmissionStage GenerateStage(int stage)
    {
        int a = Rnd.Range(0, 10);
        int b = Rnd.Range(0, 10);
        bool? op = null;
        if (stage != _stageCount - 1)
            op = Rnd.Range(0, 2) == 0;
        int ixa = Rnd.Range(0, 3);
        int ixb = Rnd.Range(0, 3);
        string display = _emojis[a * 3 + ixa] + _emojis[b * 3 + ixb];
        if (op != null)
            display += op.Value ? "+" : "-";
        return new OmissionStage(a, b, op, display);
    }

    private int CalculateOmission()
    {
        Debug.LogFormat("[OMISSION #{0}] Current equation: {1}", _moduleId, _stageInfo.Select(i => GetLogOmission(i)).Join(""));
        for (int seqLen = 2; seqLen <= 5; seqLen++)
        {
            for (int st = 0; st < _stageInfo.Count; st++)
            {
                for (int om = 0; om < _omissionPhrases.Length; om++)
                {
                    if (_omissionPhrases[om].Length != seqLen || _stageInfo.Count - st <= _omissionPhrases[om].Length)
                        continue;
                    bool needsOmission = true;
                    var list = new List<OmissionStage>();
                    for (int omIx = 0; omIx < _omissionPhrases[om].Length; omIx++)
                    {
                        list.Add(_stageInfo[st + omIx]);
                        if (_stageInfo[st + omIx].Equals(_omissionPhrases[om][omIx]) == false)
                            needsOmission = false;
                    }
                    if (needsOmission)
                    {
                        Debug.LogFormat("[OMISSION #{0}] Found omission! {1} ⇒ {2}", _moduleId, list.Select(k => GetLogOmission(k, true)).Join(""), _omissionPhrases[om].Select(k => GetLogOmission(k, true)).Join(""));
                        for (int omIx = 0; omIx < _omissionPhrases[om].Length; omIx++)
                            _stageInfo.RemoveAt(st);
                        CalculateOmission();
                    }
                }
            }
        }
        int value = 0;
        for (int i = 0; i < _stageInfo.Count; i++)
        {
            int num = _stageInfo[i].DigitA.Value * 10 + _stageInfo[i].DigitB.Value;
            if (i != 0 && !_stageInfo[i - 1].Operator.Value)
                num *= -1;
            value += num;
        }
        return value;
    }

    private string GetLogOmission(OmissionStage info, bool fromOmission = false)
    {
        return (info.DigitA == null ? "#" : info.DigitA.Value.ToString()) + (info.DigitB == null ? "#" : info.DigitB.Value.ToString()) + (info.Operator == null ? (fromOmission ? "±" : "") : info.Operator.Value ? "+" : "-");
    }

    private IEnumerator ButtonAnimation(int i, bool press)
    {
        var elapsed = 0f;
        var duration = 0.1f;
        var curPos = ButtonObjs[i].transform.localPosition.y;
        while (elapsed < duration)
        {
            ButtonObjs[i].transform.localPosition = new Vector3(ButtonObjs[i].transform.localPosition.x, Easing.InOutQuad(elapsed, curPos, press ? -0.0025f : 0f, duration), ButtonObjs[i].transform.localPosition.z);
            yield return null;
            elapsed += Time.deltaTime;
        }
        ButtonObjs[i].transform.localPosition = new Vector3(ButtonObjs[i].transform.localPosition.x, press ? -0.0025f : 0f, ButtonObjs[i].transform.localPosition.z);
    }

    private IEnumerator SubmissionAnimation(bool valid, int input)
    {
        _oopsing = true;
        ScreenText.text = "";
        int submission;
        if (!valid)
            submission = _solution + 201;
        else
            submission = input;
        int strikeCount;
        int diff = Math.Abs(_solution - input);
        if (!valid)
        {
            Debug.LogFormat("[OMISSION #{0}] Submitted an answer that was too large or small. Initiating four strikes.", _moduleId);
            strikeCount = 4;
        }
        else if (diff == 0)
        {
            Debug.LogFormat("[OMISSION #{0}] Correctly submitted {1}. Module solved.", _moduleId, submission);
            strikeCount = 0;
        }
        else if (diff <= 10)
        {
            Debug.LogFormat("[OMISSION #{0}] Submitted {1} instead of {2}, which was off by {3}. Initiating one strike.", _moduleId, submission, _solution, diff);
            strikeCount = 1;
        }
        else if (diff <= 50)
        {
            Debug.LogFormat("[OMISSION #{0}] Submitted {1} instead of {2}, which was off by {3}. Initiating two strikes.", _moduleId, submission, _solution, diff);
            strikeCount = 2;
        }
        else if (diff <= 250)
        {
            Debug.LogFormat("[OMISSION #{0}] Submitted {1} instead of {2}, which was off by {3}. Initiating three strikes.", _moduleId, submission, _solution, diff);
            strikeCount = 3;
        }
        else
        {
            Debug.LogFormat("[OMISSION #{0}] Submitted {1} instead of {2}, which was off by {3}. Initiating four strikes.", _moduleId, submission, _solution, diff);
            strikeCount = 4;
        }

        for (int i = 0; i < 12; i++)
        {
            var arr = new string[] { ".", "..", "..." };
            ScreenText.text = arr[i % 3];
            yield return new WaitForSeconds(0.35f);
        }
        yield return new WaitForSeconds(0.5f);
        ScreenText.text = "";
        if (strikeCount != 0)
            StartCoroutine(StrikeAnimation(strikeCount));
        else
            SolveMethod();
    }

    private IEnumerator StrikeAnimation(int count)
    {
        var duration = 0.5f;
        var elapsed = 0f;
        Audio.PlaySoundAtTransform("failed", transform);
        StartCoroutine(FailedText());
        while (elapsed < duration)
        {
            ModuleBackground.GetComponent<MeshRenderer>().material.color = new Color32((byte)Easing.InOutQuad(elapsed, 255, 140, duration), (byte)Easing.InOutQuad(elapsed, 255, 50, duration), (byte)Easing.InOutQuad(elapsed, 255, 50, duration), 255);
            yield return null;
            elapsed += Time.deltaTime;
        }
        yield return new WaitForSeconds(2f);
        var mainTextureOffset = ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureOffset;
        var mainTextureScale = ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureScale;
        var btnScale = Buttons[0].transform.localScale;
        for (int i = 0; i < count * 15; i++)
        {
            if (i % 15 == 5)
                Module.HandleStrike();
            ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(Rnd.Range(-0.2f, 0.2f), Rnd.Range(-0.2f, 0.2f));
            ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(Rnd.Range(0.7f, 1.3f), Rnd.Range(0.7f, 1.3f));
            for (int btn = 0; btn < Buttons.Length; btn++)
            {
                ButtonTexts[btn].text = ((char)Rnd.Range(0, 1000)).ToString();
                ButtonObjs[btn].transform.localScale = new Vector3(Rnd.Range(0.4f, 0.6f), 1f, Rnd.Range(0.4f, 0.6f));
            }
            ScreenText.text = _emojis[Rnd.Range(0, 30)] + _emojis[Rnd.Range(0, 30)] + _emojis[Rnd.Range(0, 30)] + _emojis[Rnd.Range(0, 30)];
            yield return new WaitForSeconds(0.2f);
        }
        SolveMethod();
        yield break;
    }

    private IEnumerator FailedText()
    {
        yield return new WaitForSeconds(0.1f);
        ScreenText.text = "YOU";
        yield return new WaitForSeconds(0.65f);
        ScreenText.text = "HAVE";
        yield return new WaitForSeconds(0.5f);
        ScreenText.text = "FAILED";
    }

    private void SolveMethod()
    {
        ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(0, 0);
        ModuleBackground.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1, 1);
        ModuleBackground.GetComponent<MeshRenderer>().material.color = new Color32(255, 255, 255, 255);
        ScreenText.text = _solution.ToString();
        string buttons = "0123456789-=";
        for (int btn = 0; btn < Buttons.Length; btn++)
        {
            ButtonTexts[btn].text = buttons[btn].ToString();
            ButtonObjs[btn].transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        }
        _moduleSolved = true;
        Module.HandlePass();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} recover 6 [Recover all stages starting from Stage 6 to the current stage.] | !{0} submit -47 [Submit -47 as your answer.] | !{0} stage [Show the current stage.]";
#pragma warning restore 0414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        var parameters = command.Split(' ');
        var m = Regex.Match(parameters[0], @"^\s*recover\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success && parameters.Length == 2)
        {
            if (_currentStage == _stageCount)
            {
                yield return "sendtochaterror You cannot recover a stage during input mode! Command ignored.";
                yield break;
            }
            int val;
            string str = parameters[1];
            if (!int.TryParse(str, out val))
                yield break;
            if (val < 1)
            {
                yield return "sendtochaterror You cannot recover a stage less than 1! Command ignored.";
                yield break;
            }
            yield return null;
            while (_oopsing)
                yield return null;
            if (!_activatedRecovery)
                yield return new[] { Buttons[10] };
            for (int i = 0; i < parameters[1].Length; i++)
                yield return new[] { Buttons[parameters[1][i] - '0'] };
            yield return new[] { Buttons[11] };
            yield break;
        }
        m = Regex.Match(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success && parameters.Length == 2)
        {
            if (_currentStage != _stageCount)
            {
                yield return "sendtochaterror You cannot submit an answer right now! Command ignored.";
                yield break;
            }
            int val;
            string str = parameters[1];
            if (!int.TryParse(str, out val))
                yield break;
            yield return null;
            yield return "solve";
            yield return "strike";
            while (_oopsing)
                yield return null;
            for (int i = 0; i < parameters[1].Length; i++)
            {
                if (parameters[1][i] == '-')
                    yield return new[] { Buttons[10] };
                else
                    yield return new[] { Buttons[parameters[1][i] - '0'] };
            }
            yield return new[] { Buttons[11] };
            yield break;
        }
        m = Regex.Match(parameters[0], @"^\s*stage\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success && parameters.Length == 1)
        {
            yield return null;
            Buttons[11].OnInteract();
            yield return new WaitForSeconds(2f);
            Buttons[11].OnInteractEnded();
            yield break;
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (_currentStage != _stageCount)
            yield return true;
        string s = _solution.ToString();
        int index = 0;
        if (_currentInput.Length > s.Length)
        {
            Buttons[11].OnInteract();
            yield return new WaitForSeconds(2f);
            Buttons[11].OnInteractEnded();
            yield return new WaitForSeconds(0.1f);
            goto done;
        }
        for (int i = 0; i < _currentInput.Length; i++)
        {
            if (_currentInput[i] == s[i])
                index++;
            else
            {
                index = 0;
                Buttons[11].OnInteract();
                yield return new WaitForSeconds(2f);
                Buttons[11].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
                goto done;
            }
        }
        done:
        for (int i = index; i < s.Length; i++)
        {
            if (s[i] == '-')
            {
                Buttons[10].OnInteract();
                yield return new WaitForSeconds(0.1f);
                Buttons[10].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                Buttons[s[i] - '0'].OnInteract();
                yield return new WaitForSeconds(0.1f);
                Buttons[s[i] - '0'].OnInteractEnded();
                yield return new WaitForSeconds(0.1f);
            }
        }
        Buttons[11].OnInteract();
        yield return new WaitForSeconds(0.1f);
        Buttons[11].OnInteractEnded();
        while (!_moduleSolved)
            yield return true;
    }
}