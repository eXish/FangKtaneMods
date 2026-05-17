using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;
using System.Text;
using System.Text.RegularExpressions;

// PS: Looking back at this 2 years later (of procrastination), yes it's just spaghetti codes.

#pragma warning disable IDE1006
public class pwMutilatorEX : MonoBehaviour
#pragma warning restore IDE1006
{
    public KMAudio sound;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] keyboard;
    public KMSelectable[] functionKeys;
    public KMSelectable[] timeBtns;
    public TextMesh[] displayTextsLeft;
    public TextMesh[] displayTextsRight;
    public TextMesh[] timeTexts;
    public Renderer[] modFrames;
    public Renderer   modBackground;
    public Material[] Materials;

    private readonly string[] abc = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

    static string[] ignoredModules;
    bool moduleSolved = false;
    bool moduleActivated = false;
    bool moduleWaiting = false;
    bool moduleStriking = false;
    bool moduleStrikedOnce = false;
    bool nextPhase = false;
    bool showPrev = false;
    bool[] phases = new bool[5];
        
    int currStage;
    int totalStage;
    int currSolved;
    int inputPosition = 1;

    char[] finalAnswer;
    string finalAnswerString;
    string inputAnswerString = "";

    float[,] stageTimes;
    int[] twoFactor, startingNumber, increaseFactorAverage, radix;
    int[] stageAnswer;

    float h = 0f;
    float v = 0f;
    float rememberedET;
    float rememberedBRT;


    private Dictionary<string, string> altKeysDict = new Dictionary<string, string>();
    

    // Logging
    static int moduleIdCounter = 1;
    int moduleId;

    float[] times = { 0.00f, 0.00f, 10.00f, 0f }; //Elapsed, bomb RT, Countdown timer, Strike timer 
    float[] countdownTimer = { 0, 0, 0 }; //Delayer, or Reset timer or Input phase >> times[2];
    float[] defaultTimes = { 5.00f, 120.00f, 180.00f, 5.00f }; //Next stage, Stage reset, Input phase, Timer freeze time

    void Awake()
        //Initializations
    {
        h = Random.Range(0f, 1f);
        v = Random.Range(-0.5f, 0.5f);
        altKeysDict = new Dictionary<string, string>()
        {
            {"``","~"},
            {"11","!"},
            {"22","@"},
            {"33","#"},
            {"44","$"},
            {"55","%"},
            {"66","^"},
            {"77","&"},
            {"88","*"},
            {"99","("},
            {"00",")"},
            {"--","_"},
            {"==","+"},
            {"QQ","q"},
            {"WW","w"},
            {"EE","e"},
            {"RR","r"},
            {"TT","t"},
            {"YY","y"},
            {"UU","u"},
            {"II","i"},
            {"OO","o"},
            {"PP","p"},
            {"AA","a"},
            {"SS","s"},
            {"DD","d"},
            {"FF","f"},
            {"GG","g"},
            {"HH","h"},
            {"JJ","j"},
            {"KK","k"},
            {"LL","l"},
            {"ZZ","z"},
            {"XX","x"},
            {"CC","c"},
            {"VV","v"},
            {"BB","b"},
            {"NN","n"},
            {"MM","m"},
            {"[[","{"},
            {"]]","}"},
            {"\\\\","|"},
            {";;",":"},
            {"\'\'","\""},
            {",,","<"},
            {"..",">"},
            {"//","?"},
        };
        // Assigning buttons
        foreach (KMSelectable key in keyboard)
        {
            KMSelectable pressedKey = key;
            key.OnInteract += () => HandlePress(pressedKey);
        }

        //Module ID
        moduleId = moduleIdCounter++;
        module.OnActivate += Activate;
    }

    void Start()
    {
        StartCoroutine(ColorCycle());
    }

    void Activate()
    {

        totalStage = Math.Max(bomb.GetSolvableModuleNames().Count()/2, 5);

        stageAnswer = new int[totalStage];
        twoFactor = new int[totalStage];
        startingNumber = new int[totalStage];
        increaseFactorAverage = new int[totalStage];
        radix = new int[totalStage];
        stageTimes = new float[totalStage, 2];
        finalAnswer = new char[totalStage];
        InitModule();
    }
    private IEnumerator ColorCycle()
    {
        Material tempMat = Materials[0];
        while (true)
        {
            h = (h + 0.0005f) % 1f;
            v = (v + 0.0005f) % 1f;
            tempMat.color = Color.HSVToRGB(h, 1f, Mathf.Abs(v - 0.5f));
            foreach (Renderer modFrame in modFrames)
            {
                if (moduleSolved) modFrame.material = Materials[1];
                else if (moduleStriking) modFrame.material = Materials[2];
                else modFrame.material = tempMat;
            }
            modBackground.material = tempMat;
            yield return new WaitForSeconds(.01f);
        }
    }
 
    void InitModule()
        //Initiate stage phase. (Pre-stage phase not yet implemented)
    {
        moduleActivated = true;
        phases[0] = true;
    }


    bool HandlePress(KMSelectable key)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(0.25f);
        switch (key.GetComponentInChildren<TextMesh>().text)
        {
            case "F1": 
                if (moduleSolved) break;
                else if (phases[1]) currSolved++;
                else if (phases[2] && countdownTimer[1] > 0.1f) countdownTimer[2] = 0.01f;
                else if (phases[3] && moduleStrikedOnce && !showPrev && inputAnswerString == "") showPrev = true;
                else if (showPrev) showPrev = false;
                else SubmitAns(); 
                break;
            case "F2":
                if (inputAnswerString.Length > 0 && inputPosition > 1)
                {
                    inputAnswerString = "";
                    inputPosition = 1;
                }
                break;
            case "<":
                if (!moduleSolved && inputPosition > 1) inputPosition -= 1;
                else if (!moduleSolved && phases[4] && moduleStrikedOnce && showPrev && currStage > 0) currStage--; break;
            case "v": inputPosition = Math.Min(inputPosition + 20, inputAnswerString.Length + 1); break;
            case "^": inputPosition = Math.Max(inputPosition - 20, 1); break;
            case ">": 
                if (!moduleSolved && inputPosition < inputAnswerString.Length + 1) inputPosition++; 
                else if (!moduleSolved && phases[4] && moduleStrikedOnce && showPrev && currStage < totalStage-1) currStage++; break;
            case "Backspace": 
                if (inputAnswerString.Length > 0 && inputPosition > 1) {
                    inputAnswerString = inputAnswerString.Remove(inputPosition - 2, 1);
                    inputPosition--;
                } 
                break;
            default: 
                if (!moduleSolved && phases[0] || phases[3]) {
                    inputAnswerString = inputAnswerString.Insert(inputPosition - 1, key.GetComponentInChildren<TextMesh>().text);
                    inputPosition += 1; 
                }
                break;
        }

        return false;   
    } 
    void Update()
    {
        if (!moduleActivated || moduleSolved) return;

        //Timer Multiplier for bomb RT
        double multiplier;
        switch (bomb.GetStrikes())
        {
            case 0:  multiplier = 1; break;
            case 1:  multiplier = 1.25; break;
            case 2:  multiplier = 1.5; break;
            case 3:  multiplier = 1.75; break;
            default: multiplier = 2; break;
        }
        if (phases[3] && moduleStrikedOnce && moduleStrikedOnce && showPrev)
        {
            phases[3] = false;
            phases[4] = true;
            currStage = 0;
            StartCoroutine(DisplayStagePostStrike());
        }
        else if (phases[4] && moduleStrikedOnce && !showPrev)
        {
            phases[4] = false;
            phases[3] = true;
            inputStage();
        }
        else if (phases[4])
        {
        }
        else if (phases[3])
        {   //Input phase, receiving inputs to solve answer.
            displayTextsLeft[4].text = "        Input";
            displayTextsRight[1].text = "";
            displayTextsLeft[0].text = "Input password:";
            inputStage();
        } 
        else if (phases[2] && currStage == totalStage && countdownTimer[2]<= 0)
            //Input phase commenced: note down elpTime, calculate final answer.
        {
            phases[2] = false;
            phases[3] = true;
            InputStage();
        }
        else if (phases[2] && currStage == totalStage && countdownTimer[2] > 0 )
        {   //Input phase commencing.
            displayTextsLeft[0].text = "--- ---";
            displayTextsLeft[1].text = "----";
            displayTextsLeft[4].text = "      Pre-input";
            displayTextsRight[0].text = "Next phase:";
            displayTextsRight[0].color = Color.yellow;
            countdownTimer[0] -= Time.deltaTime;
            countdownTimer[2] -= Time.deltaTime;
        }
        else if (currStage == totalStage) 
        {
            phases[1] = false;
            phases[2] = true;
            countdownTimer[0] = defaultTimes[0];
            countdownTimer[2] = defaultTimes[2];
        }
        else if (phases[1] && countdownTimer[0] <= 0f && currSolved > currStage)
        {   //Stage phase - Solved something AND time depleted.
            CalStage();
            currStage++;
            GenStage();
            countdownTimer[0] = defaultTimes[0];
            countdownTimer[1] = defaultTimes[1];

        }
        else if (phases[1] && currSolved > currStage)
        {   //Stage phase - Solved something before time depleted.
            if (countdownTimer[0] <= 0f) countdownTimer[0] = defaultTimes[0];
            moduleWaiting = true;
            countdownTimer[0] -= Time.deltaTime;
            countdownTimer[1] = defaultTimes[1];
            displayTextsRight[0].text = "Next in:";
            displayTextsRight[0].color = Color.yellow;
        }
        else if (phases[1])
        {   //Stage phase - Not yet solved.
            moduleWaiting = false;
            if (countdownTimer[0] > 0f) countdownTimer[0] -= Time.deltaTime;
            countdownTimer[1] -= Time.deltaTime;
            displayTextsRight[0].text = "Reset in:";
            displayTextsRight[0].color = Color.white;
            if (countdownTimer[1] <= 0f)
            {
                GenStage();
                countdownTimer[1] = defaultTimes[1];
            }
        }
        else if (phases[0] && currSolved > 0)
        {
            if (times[3] <= 0f) { StartCoroutine(giveStrike()); times[3] = 30f; };
            times[3] -= Time.deltaTime;

            displayTextsLeft[0].text = "Activation code:";
            displayTextsLeft[4].text = "      Pre-stage";
            inputStage();

        }
        else if (phases[0])
        {
            displayTextsLeft[0].text = "Activation code:";
            displayTextsLeft[4].text = "      Pre-stage";
            inputStage();
        }
        else {
            displayTextsLeft[0].text = "Password Mutilator\nEX";
            displayTextsLeft[4].text = "By Fang ._.";            
        }
        //Displaying times
        if(!phases[3] && !phases[4]) times[0] += Time.deltaTime; //bottom left time disp
        times[1]  = Convert.ToSingle(bomb.GetTime() / multiplier); //bottom right time disp
        times[2]  = phases[2] ? countdownTimer[2]: moduleWaiting ? countdownTimer[0] : countdownTimer[1]; //top right time disp
        for (int i = (phases[3] || phases[4] || phases[0] ? 1 : 2), j = 3; i >= 0; i--)
        {
            if ((currStage != 0 && (phases[1] || phases[2]) && (defaultTimes[0] - countdownTimer[0] <= defaultTimes[3]) && i != 2))
            //animation for temporary stop
            {
                timeTexts[0].text = getFormattedTime(rememberedET);
                timeTexts[1].text = (!ZenModeActive ? "-" : "") + getFormattedTime(rememberedBRT);
                return;
            }

            if (i == 0 && phases[0] && (currSolved > 0)) i = j;
            if (i == j && phases[0] && (currSolved > 0)) i = 0;
            timeTexts[i].text = getFormattedTime(times[i]);
            if (i == 1 && !ZenModeActive || (i == 0 && phases[0] && (currSolved > 0))) timeTexts[i].text = "-" + timeTexts[i].text;
        }   
    }
    string getFormattedTime(float time)
    {
        double millisecond = Math.Floor((time * 100) % 100);
        double second = Math.Floor(time % 60);
        double minute = Math.Floor(time / 60 % 60);
        double hour = Math.Floor(time / 3600);
        return (hour > 0 ? hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00")
                                 : minute.ToString("00") + ":" + second.ToString("00") + "." + millisecond.ToString("00"));
    }

    void inputStage() {                 
        try {
            foreach (string key in altKeysDict.Keys) 
            if (inputAnswerString.Contains(key))  {
                inputAnswerString = inputAnswerString.Replace(key, altKeysDict[key]);
                inputPosition -= 1;
                }
            }
        catch (NullReferenceException) { };

        //display input
        string temp = "";
        if (inputAnswerString.Length < 20) {
            for (int i = 0; i < inputAnswerString.Length; i++) temp += "*";
            displayTextsLeft[5].text = (inputPosition > 1 ? (new string(' ', inputPosition - 1)) : "") + '_';
        }
        else {
            temp = (inputPosition-1).ToString("000") + "/" + inputAnswerString.Length.ToString("000");
            displayTextsLeft[5].text = "";
        }
        displayTextsLeft[1].text = temp;
    }
    void InitStage()
    {
        finalAnswerString = "";
    }
    void GenStage()
        //Generate stage when next stage/timer runs out.
    {
        StopAllCoroutines();
        StartCoroutine(ColorCycle());
        if (currStage >= totalStage) return;

        twoFactor[currStage] = Random.Range(100000, 1000000);

        radix[currStage] = Random.Range(5, 17);
        int r = radix[currStage];
        startingNumber[currStage] = Random.Range(r*r*r*r, r*r*r*r*r);
        increaseFactorAverage[currStage] = Random.Range((r*r) +1, (r*r*r)-1);

        StartCoroutine(DisplayStage());
    }
                                
    IEnumerator DisplayStage()
        //Display the stage after generating its information.
    {
        if (!phases[1]) yield break;
             
        displayTextsLeft[0].text = (twoFactor[currStage] / 1000).ToString("000") + " " + (twoFactor[currStage] % 1000).ToString("000");
        displayTextsLeft[4].text = "  Stage  " + currStage.ToString("000") + " / " + (totalStage-1).ToString("000");     //"  Stage  000 / 000"
        displayTextsLeft[5].text = "";
        
        List<int> increaseFactorPool = new int[] { increaseFactorAverage[currStage] - 1, increaseFactorAverage[currStage], increaseFactorAverage[currStage] + 1}.ToList();
        int rawValue = startingNumber[currStage];
        int r = radix[currStage];
        int rng = 0;
        while (phases[1])
        {
            if (increaseFactorPool.Count() == 0)
                increaseFactorPool = new int[] { increaseFactorAverage[currStage] - 1, increaseFactorAverage[currStage], increaseFactorAverage[currStage] + 1 }.ToList();
            rng = Random.Range(0, increaseFactorPool.Count());
            rawValue = (rawValue + increaseFactorPool[rng]) % (r*r*r*r);
            displayTextsLeft[1].text = DecimalToArbitrarySystem(rawValue, r);
            increaseFactorPool.RemoveAt(rng);
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator DisplayStagePostStrike()
    {
        while (phases[4] && showPrev)
        {
            int currStageDisp = currStage;
            displayTextsLeft[0].text = (twoFactor[currStage] / 1000).ToString("000") + " " + (twoFactor[currStage] % 1000).ToString("000");
            displayTextsLeft[4].text = "  Stage  " + currStage.ToString("000") + " / " + (totalStage - 1).ToString("000");     //"  Stage  000 / 000"
            displayTextsLeft[5].text = "";
            displayTextsRight[0].color = Color.white;
            displayTextsRight[0].text = getFormattedTime(stageTimes[currStage, 0]);
            displayTextsRight[1].text = (!ZenModeActive ? "-" : "") + getFormattedTime(stageTimes[currStage, 1]);
            List<int> increaseFactorPool = new int[] { increaseFactorAverage[currStage] - 1, increaseFactorAverage[currStage], increaseFactorAverage[currStage] + 1 }.ToList();
            int rawValue = startingNumber[currStage];
            int r = radix[currStage];
            int rng = 0;
            while (phases[4] && currStage == currStageDisp && showPrev)
            {
                if (increaseFactorPool.Count() == 0)
                    increaseFactorPool = new int[] { increaseFactorAverage[currStage] - 1, increaseFactorAverage[currStage], increaseFactorAverage[currStage] + 1}.ToList();
                rng = Random.Range(0, increaseFactorPool.Count());
                rawValue = (rawValue + increaseFactorPool[rng]) % (r * r * r * r);
                displayTextsLeft[1].text = DecimalToArbitrarySystem(rawValue, r);
                increaseFactorPool.RemoveAt(rng);
                for (int i = 0; i < 10; i++)
                {
                    if (!phases[4] && !showPrev) yield break;
                    yield return new WaitForSeconds(.1f);
                }
            }
        }
    }

    public static string DecimalToArbitrarySystem(int decimalNumber, int radix)
    {
        const int BitsInLong = 64;
        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (decimalNumber == 0)
            return "0";

        int index = BitsInLong - 1;
        long currentNumber = Math.Abs(decimalNumber);
        char[] charArray = new char[BitsInLong];

        while (currentNumber != 0)
        {
            int remainder = (int)(currentNumber % radix);
            charArray[index--] = Digits[remainder];
            currentNumber = currentNumber / radix;
        }

        string result = new String(charArray, index + 1, BitsInLong - index - 1);
        if (decimalNumber < 0)
        {
            result = "-" + result;
        }

        return result;
    }

    void CalStage()
        //Calculate stage when next stage is shown, but prior to input phase.

        /*
            0: Stage
            1: 2FAST
            2: Increase factor (decimal)
            3: startingNumber, 
            4: Radix
        */
    {
        stageTimes[currStage, 0] = times[0];
        stageTimes[currStage, 1] = times[1];
        rememberedET = times[0];
        rememberedBRT = times[1];

        double ET = (Math.Floor((rememberedET % 60) * 100) / 100);
        double BRT = (Math.Floor((rememberedBRT % 60) * 100) / 100);
        
        stageAnswer[currStage] = twoFactor[currStage] % 9 + increaseFactorAverage[currStage] + Convert.ToInt32(Math.Floor(ET)) + Convert.ToInt32(Math.Floor(BRT));
        
        Debug.LogFormat
            ("[Password Mutilator EX #{0}]: Stage {1}: 2FAST: {2}, If = {3} ({4} in Base {5}), CT = {6}+{7}, Cv = {3}+{9}+{10}+{11} = {8}", 
            moduleId, currStage, twoFactor[currStage], increaseFactorAverage[currStage],
            DecimalToArbitrarySystem(increaseFactorAverage[currStage], radix[currStage]), radix[currStage],
            ET.ToString("00.00"), BRT.ToString("00.00"), 
            stageAnswer[currStage],
            twoFactor[currStage] % 9,
            Math.Floor(ET), Math.Floor(BRT));
    }
    
    void InputStage()
    {   //Calculate final answer, note down phase start time.
        inputAnswerString = "";
        inputPosition = 1;
        for (int i = 0; i < stageAnswer.Length; i++)
        {
            stageAnswer[i] = (((stageAnswer[i] + Convert.ToInt32(Math.Floor(times[0]))) % 94) + 33);
            if (i != 0 && stageAnswer[i] == stageAnswer[i - 1]) stageAnswer[i] = stageAnswer[i] == 126 ? stageAnswer[i] - 1 : stageAnswer[i] + 1;
            finalAnswer[i] = Convert.ToChar(stageAnswer[i]);
        }
        finalAnswerString = new string(finalAnswer);
        Debug.LogFormat("[Password Mutilator EX #{0}]: Input phase started at {1} seconds.", moduleId, Math.Round(times[0], 2));
        Debug.LogFormat("[Password Mutilator EX #{0}]: Final answer string: {1} ", moduleId, finalAnswerString);

        displayTextsRight[0].text = "";
        displayTextsRight[1].text = ""; 
    }
    
    void SubmitAns()
    {
        Debug.LogFormat("[Password Mutilator EX #{0}]: Submitted input: {1}", moduleId, inputAnswerString);
        if (phases[0])
        {
            finalAnswerString = pwGeneratorAnswer();
            Debug.LogFormat("[Password Mutilator EX #{0}]: Expected input: {1}", moduleId, finalAnswerString);

            if (finalAnswerString == inputAnswerString) {
                phases[1] = true;
                phases[0] = false;              
                countdownTimer[0] = defaultTimes[0];
                GenStage();
                return;
            }  
        }
        else if (finalAnswerString == inputAnswerString && phases[3])
        {
            module.HandlePass();
            sound.PlaySoundAtTransform("Windows NT Startup", transform);
            moduleSolved = true;
            return;
            // !! missing solve disp transition
        }
        inputAnswerString = "";
        inputPosition = 1;
        if (!moduleStrikedOnce && phases[3]) moduleStrikedOnce = true;
        StartCoroutine(giveStrike());
    }
    
    IEnumerator giveStrike() {
        module.HandleStrike();
        moduleStriking = true;
        yield return new WaitForSeconds(1f);
        moduleStriking = false;
    }

    private string pwGeneratorAnswer ()
	{
        string generatedInput = "";

        if (bomb.GetModuleNames().Contains("Bamboozled Again") || bomb.GetModuleNames().Contains("Ultimate Cycle") || bomb.GetModuleNames().Contains("UltraStores"))
        {
            generatedInput = "*DEAD*";
        }
        else
        {
            foreach (char d in bomb.GetSerialNumber())
                if (Char.IsDigit(d)) generatedInput += abc[int.Parse(d.ToString())];
                else generatedInput += d;
        }
        return generatedInput.ToLowerInvariant();
    }

    private bool containsModule(object Module, bool Exact)
    {
        return Exact ? (Module.GetType().IsArray ? 
        bomb.GetModuleNames().Any(mod => ((string[])Module).Contains(mod)) : 
        bomb.GetModuleNames().Contains((string)Module)) : 
        
        (Module.GetType().IsArray ? 
            bomb.GetModuleNames().Any(mod => ((string[])Module).Any(param => mod.ToLowerInvariant().Contains(param))) : 
            bomb.GetModuleNames().Any(mod => mod.ToLowerInvariant().Contains((string)Module)));
    }

    #pragma warning disable 414
    bool ZenModeActive;
    readonly string TwitchHelpMessage = "Use !{0} AZC-,... (for inputs) | (l)eft/(u)p/(d)own/(r)ight/(b)ackspace/(c)lear/(s)ubmit (for actions, first letter only). Note that the command is CAPS SENSITIVE and inputs must be same as keyboard labels.";
    #pragma warning restore 414 

    IEnumerator ProcessTwitchCommand(string command)
        //Twitch Plays.
    {
        command = command.Trim().Replace(" ", "");
        string validInputs = "`sldurb1234567890-=\\QWERTYUIOP[]ASDFGHJKL;'ZXCVBNM,./c";

        Match m = Regex.Match(command, @"^([ludrbscA-Z0-9\`\-\=\[\]\\\;\'\,\.\/]+)$");
        if (!m.Success)
            yield break;
        yield return null;
        foreach (char cmd in command) //execute;
        {
            yield return new WaitForSeconds(.1f);
            keyboard[(validInputs.IndexOf(cmd))].OnInteract();
        }
    }

}
