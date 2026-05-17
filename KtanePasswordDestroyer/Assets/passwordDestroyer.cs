using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text;
using System.Text.RegularExpressions;

public class passwordDestroyer : MonoBehaviour
{
    private Dictionary<string, KMSelectable> btnDict = new Dictionary<string, KMSelectable>();
    private Dictionary<string, KMSelectable> switchesDict = new Dictionary<string, KMSelectable>();
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public KMSelectable[] keypad;
    public KMSelectable clearButton;
    public KMSelectable submitButton;
    public KMSelectable screen;
    public KMSelectable[] Switches;
    public TextMesh[] Screens;
    public Color[] Colours;
    private const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static int[][] DateTable = new int[7][] {
        new int [12] {1, 1, 1, 3, 1, 2, 1, 2, 1, 3, 2, 3}, //7n
		new int [12] {1, 2, 3, 2, 3, 2, 1, 3, 1, 1, 1, 2}, //7n+1
		new int [12] {2, 2, 1, 1, 1, 2, 3, 1, 2, 3, 3, 2}, //7n+2
		new int [12] {2, 1, 3, 2, 1, 2, 2, 3, 3, 3, 1, 3}, //7n+3
		new int [12] {1, 2, 2, 3, 3, 2, 3, 2, 1, 1, 2, 3}, //7n+4
		new int [12] {1, 1, 3, 3, 3, 3, 1, 2, 2, 1, 1, 3}, //7n+5
		new int [12] {1, 1, 3, 2, 2, 2, 1, 3, 1, 1, 3, 2}  //7n+6
    };
    private static int[][] TimeTable = new int[20][] {
        new int [12] {1, 2, 1, 2, 1, 2, 3, 1, 2, 3, 2, 1}, //20n min
		new int [12] {3, 1, 2, 3, 1, 1, 2, 1, 1, 2, 3, 2}, //20n +1 min
		new int [12] {1, 2, 1, 3, 3, 3, 2, 2, 2, 1, 2, 3}, //20n +2 min
		new int [12] {3, 1, 2, 3, 1, 2, 1, 2, 3, 3, 3, 1}, //20n +3 min
		new int [12] {2, 2, 2, 2, 1, 3, 2, 2, 2, 3, 1, 3}, //20n +4 min
		new int [12] {2, 2, 1, 3, 1, 3, 3, 3, 2, 1, 3, 2}, //20n +5 min
		new int [12] {1, 2, 3, 3, 1, 2, 3, 3, 1, 3, 3, 2}, //20n +6 min
		new int [12] {3, 1, 3, 1, 1, 1, 3, 2, 1, 1, 3, 1}, //20n +7 min
		new int [12] {3, 3, 2, 1, 3, 2, 1, 1, 1, 3, 1, 1}, //20n +8 min
		new int [12] {2, 2, 2, 2, 3, 2, 3, 2, 3, 2, 1, 3}, //20n +9 min
		new int [12] {3, 2, 2, 2, 3, 1, 1, 3, 2, 3, 2, 3}, //20n +10 min
		new int [12] {1, 3, 2, 2, 3, 1, 3, 3, 3, 3, 1, 1}, //20n +11 min
		new int [12] {3, 2, 1, 2, 3, 1, 2, 2, 2, 3, 2, 2}, //20n +12 min
		new int [12] {2, 3, 3, 2, 3, 1, 3, 1, 1, 3, 3, 2}, //20n +13 min
		new int [12] {1, 1, 2, 2, 3, 3, 3, 2, 2, 2, 2, 2}, //20n +14 min
		new int [12] {1, 2, 1, 3, 1, 2, 1, 2, 3, 1, 3, 2}, //20n +15 min
		new int [12] {3, 1, 1, 1, 3, 2, 1, 3, 2, 2, 3, 2}, //20n +16 min
		new int [12] {3, 1, 2, 1, 2, 1, 3, 3, 3, 3, 1, 2}, //20n +17 min
		new int [12] {2, 3, 3, 3, 3, 3, 3, 2, 1, 2, 1, 2}, //20n +18 min
		new int [12] {1, 3, 1, 1, 1, 1, 3, 2, 1, 2, 3, 2}  //20n +19 min
    };
    StringBuilder Switches_State = new StringBuilder("000");
    StringBuilder Submission_State = new StringBuilder("000");
    private bool switchesTrue = true, solvedState, erroring = false;
    private int switchToToggle1, switchToToggle2;
    private int pressedNumber = 0;
    private bool inputMode = false, strikedatleastonce = false, forceSolved = false, split = false;
    private string submitKey = "";
    private string currentTimeHourDisplay, currentTimeMinDisplay;
    private int currentTimeMin, currentTimeHour, currentMonth, currentDay;
    private bool timeoutreset, seven;
    // Logging
    static int moduleIdCounter = 1;
    int moduleId;
    int identityDigit, identityDigit1, identityDigit2;
    int CountUpNumber, CountUpBaseNumber, increaseFactor;
    string CountUpNumberDisplay;
    int elapsedTime = 0, elapsedTimeMin, elapsedTimeSec, strikedTime, correctTime;
    double doubleElapsedTime = 0f;
    int bombSerialNumber, moduleSerialNumber, solvePercentage;
    long calculatedValue, finalValue;
    int numofbars;
    string finalAnswer;
    string elapsedTimeDisplay, doubleElapsedTimeDisplay, elapsedTimeMinDisplay, elapsedTimeSecDisplay, twofatimedisplay, strikedTimeDisplay, tempDisp, tempTime;
    bool showing2FA, showingTime, showingStrike, dotUnlit, initiated;

    // Use this for initialization
    void Awake()
    {
        // Assigning buttons
        foreach (KMSelectable key in keypad)
        {
            KMSelectable pressedKey = key;
            key.OnInteract += () => PressKey(pressedKey);
        }
        clearButton.OnInteract += resetInput;
        submitButton.OnInteract += submitInput; 
        screen.OnInteract += SplitSeconds;
        //
        // Assigning switches
        for (int i = 0; i < 3; i++)
        {
            int j = i;
            Switches[j].GetComponent<KMSelectable>().OnInteract += delegate ()
            {
                StartCoroutine(FlipSwitch(j));
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Switches[j].transform);
                Switches[j].GetComponent<KMSelectable>().AddInteractionPunch(.25f);
                return false;
            };
        }
        //Generate Random Switch Position
        int random_flip = 0;
        for (int i = 0; i < 3; i++)
        {
            random_flip = Random.Range(0, 2);
            if (random_flip == 1) { StartCoroutine(FlipSwitch(i)); Switches_State[i] = '1'; }
        }
        //
        //Module ID
        moduleId = moduleIdCounter++;
        //
        //Display Screen
        //
        //Enable inputs
        inputMode = true;
        Module.OnActivate += initiateModule;
        //
        //
    }
    void Update() {
        decimal solvePercentage = Math.Round( Math.Max ((decimal)1, (((decimal) Bomb.GetSolvedModuleNames().Count) * (decimal) 100)/ (decimal) Bomb.GetSolvableModuleNames().Count), 2);

        if (!initiated || erroring) return;
        if (!solvedState) doubleElapsedTime += Time.deltaTime;
        double second = Math.Round(doubleElapsedTime % 60, 2);
        double minute = Math.Floor(doubleElapsedTime / 60 % 60);
        double hour = Math.Floor(doubleElapsedTime / 3600);

		doubleElapsedTimeDisplay =
			(hour > 0 ?  hour.ToString("00")  + ":" + minute.ToString("00") + ":" + second.ToString("00")
				  	  : minute.ToString("00") + ":" + (doubleElapsedTime % 60).ToString("00.00"));

        if (pressedNumber == 0 && (elapsedTime - strikedTime)% 2400 == 0)
            Screens[0].text = "-------";
        else if (pressedNumber == 0)
            Screens[0].text = CountUpNumberDisplay;

        if (showing2FA) Screens[1].text = identityDigit1.ToString() + " " + identityDigit2.ToString() + ".";
        if (showingTime)  Screens[1].text = DateTime.Now.ToString("HH:mm:ss");
        if (showingStrike) {
            Screens[1].text = strikedTimeDisplay;
            Screens[1].color = Colours[1];
        }

        if (split == false) { 
        Screens[2].text = doubleElapsedTimeDisplay;
        }
        
        if ( doubleElapsedTime - elapsedTime > 1 && doubleElapsedTime > 0) {
            TimeDisplay();
        }

        if (pressedNumber == 0 && inputMode == true && second % 1 < 0.5)
            Screens[4].text = ".";
        else 
            Screens[4].text = "";

    }
    void initiateModule() {
        //Generate Numbers, for the first time - Sv, If, 2FAST
        initiated = true;
        Screens[0].text = submitKey;
        CountUpBaseNumber = Random.Range(1000000, 10000000);
        increaseFactor = Random.Range(100000, 1000001);
        int rng = Random.Range(0, 2);
        if (rng == 1) increaseFactor = -increaseFactor;
        Generate2FA();
        TimeDisplay();
        StartCoroutine(display1Cycle());
        Debug.LogFormat("[Password Destroyer #{0}]: Initial base numbers are {1} and {2}, with starting 2FA of {3} {4}.", moduleId, CountUpBaseNumber, increaseFactor, identityDigit1, identityDigit2);
    }
    //
    // Flipping the switch
    IEnumerator FlipSwitch(int selected)
    {
        if (Switches_State[selected] == '0') Switches_State[selected] = '1';
        else Switches_State[selected] = '0';
        const float duration = .3f;
        var startTime = Time.fixedTime;
        if (Switches[selected].transform.localEulerAngles.x >= 50 && Switches[selected].transform.localEulerAngles.x <= 60)
        {
            do
            {
                Switches[selected].transform.localEulerAngles = new Vector3(easeOutSine(Time.fixedTime - startTime, duration, 55f, -55f), 0, 0);
                yield return null;
            }
            while (Time.fixedTime < startTime + duration);
            Switches[selected].transform.localEulerAngles = new Vector3(-55f, 0, 0);
        }
        else
        {
            do
            {
                Switches[selected].transform.localEulerAngles = new Vector3(easeOutSine(Time.fixedTime - startTime, duration, -55f, 55f), 0, 0);
                yield return null;
            }
            while (Time.fixedTime < startTime + duration);
            Switches[selected].transform.localEulerAngles = new Vector3(55f, 0, 0);
        }

    }
    //
    // Switch Animation
    private float easeOutSine(float time, float duration, float from, float to)
        {   return (to - from) * Mathf.Sin(time / duration * (Mathf.PI / 2)) + from; }
    //
    // When a key is pressed
    bool PressKey(KMSelectable key) {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(0.25f);
        if (inputMode == true)
        {
            submitKey += key.GetComponentInChildren<TextMesh>().text.ToString();
            Screens[0].text = submitKey;
            pressedNumber = submitKey.Length;
            checkInput(pressedNumber);
        }
        return false;
    }

    bool SplitSeconds() {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, screen.transform);
        screen.AddInteractionPunch(0.25f);
        if ((elapsedTime - strikedTime)% 2400 > 0) {
            if (inputMode == true) {
                if (split == true) {
                    split = false;
                }
                else if (split == false) {
                    split = true;
                    StartCoroutine(Splitting());
                    tempDisp = CountUpNumberDisplay;
                    tempTime = doubleElapsedTimeDisplay;
                }
            }
        }
        return false;
    }
    void Generate2FA()
    {
        identityDigit1 = Random.Range(100, 1000);
        identityDigit2 = Random.Range(100, 1000);
        identityDigit = identityDigit1 * 1000 + identityDigit2;
        numofbars = 10;
        twofatimedisplay = "█ █ █ █ █ █ ";
        Screens[3].text = twofatimedisplay;
    }

    StringBuilder NumbersToArrows(StringBuilder numbers)
    {
        StringBuilder arrows = new StringBuilder("▲▲▲");
        for (int i = 0; i < 3; i++) { if (numbers[i] == '0') { arrows[i] = '▼'; } }
        return arrows;
    }
    void ResetModule()
    {
        split = false;
        timeoutreset = false;
        strikedatleastonce = true;
        CountUpBaseNumber = Random.Range(1000000, 10000000);
        increaseFactor = Random.Range(100000, 1000001);
        int rng = Random.Range(0,2);
        if (rng == 1) increaseFactor = -increaseFactor;
        strikedTimeDisplay = Screens[2].text;
        strikedTime = elapsedTime;
        Generate2FA();
        identityDigit = identityDigit1 * 1000 + identityDigit2;
        Debug.LogFormat("[Password Destroyer #{0}]: Generated at {1}, the new base numbers are {2} and {3}, with starting 2FA of {4} and starting switches position of {5}.", moduleId, strikedTimeDisplay, CountUpBaseNumber, increaseFactor, identityDigit, NumbersToArrows(Switches_State));
    }
    IEnumerator display1Cycle()
    {
        while (!solvedState)
        {
            if (!solvedState && pressedNumber == 0) showing2FA = true;
            for (int i = 0; i < 5; i++)
            {
                if (pressedNumber == 0) { yield return new WaitForSeconds(1f); };
            }
            showing2FA = false;
            for (int i = 0; i < 5; i++)
            {
                if (!solvedState && pressedNumber == 0) showingTime = true;
                if (pressedNumber == 0) { yield return new WaitForSeconds(1f); };
            }
            showingTime = false;
            if (!solvedState && pressedNumber == 0 && strikedatleastonce == true) showingStrike = true;
            for (int i = 0; i < 5; i++)
            {
                if (!solvedState && pressedNumber == 0 && strikedatleastonce == true) { yield return new WaitForSeconds(1f); };
            }
            showingStrike = false;
            yield return new WaitForSeconds(0.1f);
            Screens[1].color = Colours[0];
            if (!solvedState && pressedNumber != 0) { Screens[1].text = "INPUT   "; }
        }
    }

    IEnumerator Splitting() {
        while (!solvedState) {
            yield return null;
            if (split == true) Screens[2].text = "-SPLIT-";
            if (split == true) yield return new WaitForSeconds(1f);
            while (split == true) {
                Screens[2].text = tempTime;
                if (split == true) yield return new WaitForSeconds(1f);
                Screens[2].text = "";
                if (split == true) yield return new WaitForSeconds(1f);
            }
        }
    }
    void TimeDisplay()
    {
        if (!solvedState)
        {
            if (pressedNumber != 0)
            {
                Screens[1].text = "INPUT   ";
            }
            elapsedTime = (int) doubleElapsedTime;
            elapsedTimeMin = elapsedTime / 60;
            elapsedTimeSec = elapsedTime % 60;
            currentTimeHourDisplay = DateTime.Now.ToString("HH");
            currentTimeMinDisplay = DateTime.Now.ToString("mm");
            currentTimeHour = Convert.ToInt32(currentTimeHourDisplay);
            currentTimeMin = Convert.ToInt32(currentTimeMinDisplay);
            if (elapsedTimeMin < 10) { elapsedTimeMinDisplay = "0" + elapsedTimeMin.ToString(); }
            else { elapsedTimeMinDisplay = elapsedTimeMin.ToString(); }
            if (elapsedTimeSec < 10) { elapsedTimeSecDisplay = "0" + elapsedTimeSec.ToString(); }
            else { elapsedTimeSecDisplay = elapsedTimeSec.ToString(); }
            if (timeoutreset == true) {
                ResetModule();
                timeoutreset = false;
            }
            if ((elapsedTime - strikedTime)% 2400 == 0 && elapsedTime != 0 && elapsedTime != strikedTime)
            {   
                timeoutreset = true;
            }
            else if ((elapsedTime - strikedTime) % 40 == 0 && elapsedTime != 0)
            {
                twofatimedisplay = twofatimedisplay.Remove(numofbars);
                numofbars -= 2;
                Screens[3].text = twofatimedisplay;
            }
            if ((elapsedTime - strikedTime) % 240 == 0 && elapsedTime != 0)
            {
                Generate2FA();
                identityDigit = identityDigit1 * 1000 + identityDigit2;
            }
            CountUpNumber = (((CountUpBaseNumber + (increaseFactor * (elapsedTime - strikedTime))) % 10000000) + 10000000) % 10000000;
            //Prepend zeroes # ### ###
            if (inputMode == true && split == false) {
                if (CountUpNumber < 10) {
                    CountUpNumberDisplay = "000000" + CountUpNumber.ToString();
                }
                else if (CountUpNumber < 100) {
                    CountUpNumberDisplay = "00000" + CountUpNumber.ToString();
                }
                else if (CountUpNumber < 1000) {
                    CountUpNumberDisplay = "0000" + CountUpNumber.ToString();
                }
                else if (CountUpNumber < 10000) {  
                    CountUpNumberDisplay = "000" + CountUpNumber.ToString();
                }
                else if (CountUpNumber < 100000) {
                    CountUpNumberDisplay = "00" + CountUpNumber.ToString();
                }
                else if (CountUpNumber < 1000000) {
                    CountUpNumberDisplay = "0" + CountUpNumber.ToString();
                }
                else {
                    CountUpNumberDisplay = CountUpNumber.ToString();
                }
            }
            elapsedTimeDisplay = elapsedTimeMinDisplay + ":" + elapsedTimeSecDisplay;
            if (split == false) { 
            //Screens[2].text = elapsedTimeDisplay;
            }
        }
    }

    // Reset the input
    private bool resetInput()
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, clearButton.transform);
        clearButton.AddInteractionPunch(0.25f);
        if (inputMode == true && pressedNumber != 0)
        {
            split = false;
            submitKey = "";
            pressedNumber = 0;
            Screens[0].text = submitKey;
        };
        return false;
    }
    // Submit the number
    private bool submitInput()
    {        
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submitButton.transform);
        submitButton.AddInteractionPunch(0.25f);

        if (inputMode == true)
        {
        solvePercentage = Math.Max (1, ((Bomb.GetSolvedModuleNames().Count * 100)/Bomb.GetSolvableModuleNames().Count));
        GenerateCorrectTime();
        GenerateSwitchesAnswer();
        GeneratePINAnswer();
        Debug.LogFormat("[Password Destroyer #{0}]: Submit button was pressed on elapsed time of {1} seconds.", moduleId, elapsedTime);
                if (elapsedTime % 10 != correctTime) {
                    Debug.LogFormat("[Password Destroyer #{0}] Incorrect submission time. Module reset.", moduleId);
                    GetComponent<KMBombModule>().HandleStrike();
                    Audio.PlaySoundAtTransform("wrong", transform);
                    StartCoroutine(DisplayError(submitKey));
                    pressedNumber = 0;
                    Screens[0].text = submitKey;
                    switchesTrue = true;
                    ResetModule();
                }
                else if (submitKey == finalAnswer && switchesTrue == true)
                    {
                        Debug.LogFormat("[Password Destroyer #{0}]: You have inputted correct answer. Module solved.", moduleId);
                        GetComponent<KMBombModule>().HandlePass();
                        submitKey = "SOLVED";
                        Screens[0].text = submitKey;
                        inputMode = false;
                        solvedState = true;
                        Audio.PlaySoundAtTransform("win98start", transform);
                        Screens[2].color = Colours[2];
                        Screens[1].text = "NICE ";
                        Screens[3].text = "";
                    }
                else if (switchesTrue == false) {
                        GetComponent<KMBombModule>().HandleStrike();
                        Audio.PlaySoundAtTransform("wrong", transform);
                        Debug.LogFormat("[Password Destroyer #{0}] Switch State: You submitted {1}, but it was expected to be {2}", moduleId, NumbersToArrows(Switches_State), NumbersToArrows(Submission_State));
                        StartCoroutine(DisplayError(submitKey));
                        pressedNumber = 0;
                        Screens[0].text = submitKey;
                        switchesTrue = true;
                        ResetModule();
                    }
                else
                    {
                        Debug.LogFormat("[Password Destroyer #{0}]: Switch State: You submitted {1}, expected {2}, which is correct.", moduleId, NumbersToArrows(Switches_State), NumbersToArrows(Submission_State));
                        Debug.LogFormat("[Password Destroyer #{0}]: You have inputted {1}, expected {2}, which is the wrong answer. Module striked and reset.", moduleId, submitKey, finalAnswer);
                        GetComponent<KMBombModule>().HandleStrike();
                        Audio.PlaySoundAtTransform("wrong", transform);
                        StartCoroutine(DisplayError(submitKey));
                        pressedNumber = 0;
                        Screens[0].text = submitKey;
                        ResetModule();
                    }
                }
        return false;
    }
    void GenerateCorrectTime() {
        foreach (string idc in Bomb.GetIndicators())
        {
            char[] idcletters = idc.ToCharArray();
            for (int x = 0; x < idcletters.Length; x++)
            {
                if ("SEVEN".Contains(idcletters[x]))
                {
                    seven = true;
                }
            }
        }
        if (Bomb.GetSerialNumberNumbers().Last() == 0) {
            correctTime = 0;
        }
        else if (Bomb.GetSerialNumberNumbers().First() == 1) {
            correctTime = 1;
        }
        else if (containsModule("The Twin", true)) {
            correctTime = 2;
        }
        else if (identityDigit % 3 == 0) {
            correctTime = 3;
        }
        else if (Bomb.GetBatteryCount() > 4) {
            correctTime = 4;
        }
        else if (identityDigit % 5 == 0) {
            correctTime = 5;
        }
        else if (solvePercentage >= 60) {
            correctTime = 6;
        }
        else if (seven == true) {
            correctTime = 7;
        }
        else if (Bomb.GetPortCount() > 8) {
            correctTime = 8;
        }
        else { 
            correctTime = 9; 
        }
        Debug.LogFormat("[Password Destroyer #{0}]: You must input when the last digit of elapsed time is equal to {1}.", moduleId, correctTime);           
    }

    void GenerateSwitchesAnswer() {
        //Section VI - Switch Positions
        currentDay = Convert.ToInt32(DateTime.Now.ToString("dd"));
        currentMonth = Convert.ToInt32(DateTime.Now.ToString("MM"));
        currentTimeHour = Convert.ToInt32(DateTime.Now.ToString("HH"));
        currentTimeMin = Convert.ToInt32(DateTime.Now.ToString("mm"));
        Debug.LogFormat("[Password Destroyer #{0}]: Current Time is {1}/{2}, {3}:{4} hrs.", moduleId, currentDay, currentMonth, currentTimeHourDisplay, currentTimeMinDisplay);

        switchToToggle1 = DateTable[currentDay % 7][currentMonth - 1];
        switchToToggle2 = TimeTable[currentTimeMin % 20][currentTimeHour % 12];
        if (switchToToggle1 == switchToToggle2) {
            if (switchToToggle1 == 1 && switchToToggle2 == 1) {
                Submission_State = new StringBuilder("011");
            }
            else if (switchToToggle1 == 2 && switchToToggle2 == 2) {
                Submission_State = new StringBuilder("101");
            }
            else if (switchToToggle1 == 3 && switchToToggle2 == 3) {
                Submission_State = new StringBuilder("110");
            }
        }
        else {
            if ((switchToToggle1 == 1 && switchToToggle2 == 2) || (switchToToggle1 == 2 && switchToToggle2 == 1)) {
                Submission_State = new StringBuilder("001");
            }
            else if ((switchToToggle1 == 1 && switchToToggle2 == 3) || (switchToToggle1 == 3 && switchToToggle2 == 1)) {
                Submission_State = new StringBuilder("010");
            }
            else if ((switchToToggle1 == 2 && switchToToggle2 == 3) || (switchToToggle1 == 3 && switchToToggle2 == 2)) {
                Submission_State = new StringBuilder("100");
            }
        }
        Debug.LogFormat("[Password Destroyer #{0}]: The answer are {1} and {2}. The final switch positions are {3}.", 
                        moduleId, switchToToggle1, switchToToggle2, NumbersToArrows(Submission_State));
        
        for (int i = 0; i < 3; i++) {
            if ((Submission_State[i] != Switches_State[i]))
            {
                switchesTrue = false;
            }
        }
    }
    private bool containsModule(object Module, bool Exact)
    {
        return Exact ? (Module.GetType().IsArray ? Bomb.GetModuleNames().Any(mod => ((string[])Module).Contains(mod)) : Bomb.GetModuleNames().Contains((string)Module)) : (Module.GetType().IsArray ? Bomb.GetModuleNames().Any(mod => ((string[])Module).Any(param => mod.ToLowerInvariant().Contains(param))) : Bomb.GetModuleNames().Any(mod => mod.ToLowerInvariant().Contains((string)Module)));
    }
    void GeneratePINAnswer()
    {
        //Section I, II, III, IV - Shows the time
        Debug.LogFormat("[Password Destroyer #{0}]: Rv = {1}, 2FAST = {2}.", moduleId, CountUpNumber, identityDigit);

        //Section V - Modifications
        int TFA1 = (identityDigit / 1000) - 100;
        int TFA2 = 1 + (((identityDigit % 1000) - 1) % 9);

        bombSerialNumber = Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[0]), 1) * Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[1]), 1) * Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[2]), 1) * Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[3]), 1) * Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[4]), 1) * Math.Max(digits.IndexOf(Bomb.GetSerialNumber()[5]), 1);		
        moduleSerialNumber = Math.Max(1, (CountUpBaseNumber / 1000000)) * Math.Max(1, ((CountUpBaseNumber / 100000) % 10)) * Math.Max(1, ((CountUpBaseNumber / 10000) % 10)) * Math.Max(1, ((CountUpBaseNumber / 1000) % 10)) * Math.Max(1, ((CountUpBaseNumber / 100) % 10)) * Math.Max(1, ((CountUpBaseNumber / 10) % 10)) * Math.Max(1, (CountUpBaseNumber % 10));
        long TFA1xBSN = (long) TFA1 * (long)bombSerialNumber;
        calculatedValue = (CountUpNumber + TFA1xBSN + (TFA2 * moduleSerialNumber) + elapsedTime) ;
        Debug.LogFormat("[Password Destroyer #{0}]: TFA1 = {1} and TFA2 = {2}.", moduleId, TFA1, TFA2);
        Debug.LogFormat("[Password Destroyer #{0}]: BSN = {1} and MSN = {2}.", moduleId, bombSerialNumber, moduleSerialNumber);
        Debug.LogFormat("[Password Destroyer #{0}]: Cv = {7} + {1}*{2} + {3}*{4} + {5} = {6}.", moduleId, TFA1, bombSerialNumber, TFA2, moduleSerialNumber, elapsedTime, calculatedValue, CountUpNumber);

        //Section VII
        finalValue = (calculatedValue * solvePercentage / 100) % 10000000;
        Debug.LogFormat("[Password Destroyer #{0}]: Solve Percentage = {1}, Final value = {2}.", moduleId, solvePercentage, finalValue);
        
        //Prepend zeroes # ### ###
        if (finalValue < 10) {
            finalAnswer = "000000" + finalValue.ToString();
        }
        else if (finalValue < 100) {
            finalAnswer = "00000" + finalValue.ToString();
        }
        else if (finalValue < 1000) {
            finalAnswer = "0000" + finalValue.ToString();
        }
        else if (finalValue < 10000) {  
            finalAnswer = "000" + finalValue.ToString();
        }
        else if (finalValue < 100000) {
            finalAnswer = "00" + finalValue.ToString();
        }
        else if (finalValue < 1000000) {
            finalAnswer = "0" + finalValue.ToString();
        }
        else {
            finalAnswer = finalValue.ToString();
        }
    }
    // Strike if received too many inputs
    void checkInput(int pressedNumber)
    {
        if (pressedNumber > 7)
        {   
            if (forceSolved == true) {
                Debug.LogFormat("[Password Destroyer #{0}]: Module was force solved.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
                submitKey = "SOLVED";
                Screens[0].text = submitKey;
                inputMode = false;
                solvedState = true;
                Audio.PlaySoundAtTransform("win98start", transform);
                Screens[2].color = Colours[3];
                Screens[2].text = "NO  TIME";
                Screens[3].text = "";
                Screens[1].text = "*FORCED";                
            }
            else {
            GetComponent<KMBombModule>().HandleStrike();
            Audio.PlaySoundAtTransform("badbeep", transform);
            StartCoroutine(DisplayBlink(submitKey));
            Debug.LogFormat("[Password Destroyer #{0}]: You have inputted too many characters. Module striked and reset.", moduleId);
            ResetModule();
            }
        }
    }
    IEnumerator DisplayBlink(string checkInput)
    {
        inputMode = false;
        Screens[2].color = Colours[1];
        for (var i = 0; i < 1; i++)
        {
            submitKey = "-------";
            Screens[0].text = submitKey;
            yield return new WaitForSeconds(0.5f);
            submitKey = "";
            Screens[0].text = submitKey;
            yield return new WaitForSeconds(0.5f);
        }
        Screens[2].color = Colours[0];
        for (var i = 0; i < 2; i++)
        {
            submitKey = "-------";
            Screens[0].text = submitKey;
            yield return new WaitForSeconds(0.5f);
            submitKey = "";
            Screens[0].text = submitKey;
            yield return new WaitForSeconds(0.5f);
        }
        pressedNumber = 0;
        inputMode = true;
    }
    IEnumerator DisplayError(string submitInput)
    {
        erroring = true;
        inputMode = false;
        submitKey = "-WRONG-";
        Screens[0].text = submitKey;
        Screens[2].color = Colours[1];
        yield return new WaitForSeconds(0.3f);
        submitKey = "ANSWER  ";
        Screens[0].text = submitKey;
        yield return new WaitForSeconds(0.3f);
        submitKey = "PLEASE  ";
        Screens[0].text = submitKey;
        Screens[2].color = Colours[0];
        yield return new WaitForSeconds(0.3f);
        submitKey = "-RETRY-";
        Screens[0].text = submitKey;
        submitKey = "";
        yield return new WaitForSeconds(0.5f);
        pressedNumber = 0;
        inputMode = true;
        erroring = false;
    }
    #pragma warning disable 414
    string TwitchHelpMessage = "Use !{0} press <number> // toggle <switches position> // time // clear // split // split/submit (at/on/-) <time> // s <number> <switches (1 as up, 0 as down)> <time>.";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        Match m = Regex.Match(
            command, 
            @"^(?:(press ((?:[0-9 ]+)|(?:clear)))|(toggle ([1-3 ]+))|(time|clear|split)|((submit|split)\s*(?:at|on)?\s*([0-9]+:)?([0-9]+):([0-5][0-9]))|(s\s*([0-9]{7})\s*([01]{3})\s*([0-9]+:)?([0-9]+):([0-5][0-9])))$");
        /*Capture Groups:
            1.  press
            2.  .. what button / clear?
            3.  toggle
            4.  .. what switch
            5.  time/clear/split
            6.  submit/split at
            7.  .. submit or split
            8.  .. what hour, if any
            9.  .. what minute
            10. .. what sec
            11. one command
            12. .. what button
            13. .. what desired switches state
            14. .. what hour, if any
            15. .. what minute
            16. .. what sec
        */
        if (!m.Success || (m.Groups[6].Success && m.Groups[8].Success && int.Parse(m.Groups[9].Value)> 59))
            yield break;
        if (m.Groups[5].Success && m.Groups[5].Value == "time")
        {
            yield return "sendtochat!h Current Time: " + DateTime.Now.ToString("MMMM dd") + ", " + DateTime.Now.ToString("HH:mm:ss") + "; Current Display Time: " + elapsedTimeDisplay;
            yield break;
        }
        yield return null;
        if (m.Groups[11].Success && !(m.Groups[14].Success && int.Parse(m.Groups[15].Value) > 59)) {
            clearButton.OnInteract();
            char[] numbers = m.Groups[12].Value.ToCharArray();
            foreach (char number in numbers) {
                yield return new WaitForSeconds(0.1f);
                keypad[int.Parse(number.ToString())].OnInteract();
            }
            yield return null;
            char[] desState = m.Groups[13].Value.Replace(" ", "").ToCharArray();
            for (int i = 0; i < 3; i++) {
                yield return null;
                if (Switches_State[i] != desState[i]) Switches[i].OnInteract();
            }

            int commandSeconds = (!m.Groups[14].Success ? 0 : int.Parse(m.Groups[14].Value.Replace(":", ""))) * 3600 + int.Parse(m.Groups[15].Value) * 60 + int.Parse(m.Groups[16].Value);

            while (elapsedTime != commandSeconds) 
                yield return "trycancel Button wasn't pressed due to request to cancel.";
            submitButton.OnInteract();
        }
        else if (m.Groups[1].Success) {
            if (m.Groups[2].Value == "clear")
            {
                clearButton.OnInteract();
                yield break;
            }
            char[] numbers = m.Groups[2].Value.Replace(" ", "").ToCharArray();
            foreach (char number in numbers) {
                yield return new WaitForSeconds(0.1f);
                keypad[int.Parse(number.ToString())].OnInteract();
            }
        }
        else if (m.Groups[3].Success) {
            char[] numbers = m.Groups[4].Value.Replace(" ", "").ToCharArray();
            foreach (char number in numbers) {
                yield return new WaitForSeconds(0.1f);
                Switches[int.Parse(number.ToString()) - 1].OnInteract();
            }
        }
        else if (m.Groups[5].Success) {
            switch (m.Groups[5].Value) {
                case "clear":
                    clearButton.OnInteract();
                    break;
                case "split":
                    screen.OnInteract();
                    break;
                default:
                    break;
            }
        }
        else if (m.Groups[6].Success) {
            int commandSeconds = (!m.Groups[8].Success ? 0 : int.Parse(m.Groups[8].Value.Replace(":", ""))) * 3600 + int.Parse(m.Groups[9].Value) * 60 + int.Parse(m.Groups[10].Value);

            while (elapsedTime != commandSeconds) 
                yield return "trycancel Button wasn't pressed due to request to cancel.";
            if (m.Groups[7].Value == "submit") submitButton.OnInteract();
            else screen.OnInteract();
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        forceSolved = true;
        while (pressedNumber != 8) {
            int rng = Random.Range(0, 10);
            keypad[rng].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
     }
}
