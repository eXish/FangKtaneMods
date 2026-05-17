using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;

public class pentrisSprint : MonoBehaviour {
    public class ModSettingsJSON
    {
        public int linesToClear;
        public string note;
    }
	public GameObject Board;

	public Material BoxFull;
	public Material BoxEmpty;
	public Material BoxError;

    public Material solvedMat;
    public Material strikedMat;
    public Material normalMat;
	public Renderer modFrame;

    public KMSelectable ModuleSelectable;
	public TextMesh numberDisplay;
	public TextMesh scoreDisplay;
	public TextMesh timeDisplay;
	public TextMesh targetDisplay;
	public KMModSettings modSettings;
	
	private PentrisBoard GameBoard;

	private const int G_WIDTH = 10; // Width of grid
	private const int G_HEIGHT = 20; // Width of grid

	private KMBombModule Module;
	private GameObject[,] ObjectGrid;
    public GameObject[] CellObjects;
    public GameObject[] ScreenGrid;
	private Pentomino tetr;
	private int upNext;
	private int Score;
	private int linesLeft;
	private int activation = 0;
	private int moduleId = 0;
	private static int moduleIdCounter = 1;
	private int[] PentoDisplay;
	private float elapsedTime;
	private string elapsedTimeDisplay;
	private bool started = false;
	private bool moduleSolved = false;
	private bool holdingleft, holdingright;

    private List<int> grabBag = new List<int>();


	void SetMaterial(GameObject go, Material mat)
	{
		go.GetComponent<MeshRenderer> ().material = mat;
	}

	void UpdateGrid()
	{
		for (int y = 0; y < G_HEIGHT; y++) {
			for (int x = 0; x < G_WIDTH; x++) {
				GameObject go = ObjectGrid [x, y];

				if (GameBoard.get (x, y) != 0) {
					go.SetActive (true);
					SetMaterial (go, BoxFull);
				} else {
					go.SetActive (false);
				}
			}
		}

		if (tetr != null) {
			List<IntPair> list = tetr.GetTileCoordinates ();
			foreach (IntPair p in list) {
				GameObject go = ObjectGrid [p.x, p.y];
				go.SetActive (true);
				if (tetr.isValid ()) {
					SetMaterial (go, BoxEmpty);
				} else {
					SetMaterial (go, BoxError);
				}
			}
		}
		scoreDisplay.text = Score.ToString();
		numberDisplay.text = linesLeft.ToString();
		if (moduleSolved) for (int i = 0; i < 17; i++) ScreenGrid[i].SetActive(false);
		else 
		{
			switch (upNext) {
				case 0:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0,
						1, 1, 0, 0,
						0, 1, 0, 0, 0
					};
					break;
				case 1:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						1, 1, 0, 0,
						0, 1, 1, 0,
						0, 1, 0, 0, 0
					};
					break;
				case 2:
					PentoDisplay = new int[] {
						0, 0, 1, 0, 
						0, 0, 1, 0, 
						0, 0, 1, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 3:
					PentoDisplay = new int[] {
						0, 1, 0, 0, 
						0, 1, 0, 0,
						0, 1, 0, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 4:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 1, 1, 0,
						0, 1, 1, 0, 
						0, 0, 1, 0, 0
					};
					break;
				case 5:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0,
						0, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 6:
					PentoDisplay = new int[] {
						0, 0, 1, 0, 
						0, 0, 1, 0,  
						0, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 7:
					PentoDisplay = new int[] {
						0, 1, 0, 0,
						0, 1, 0, 0, 
						0, 1, 1, 0,
						0, 0, 1, 0, 0
					};
					break;
				case 8:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 0, 0, 
						1, 1, 1, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 9:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						1, 1, 1, 0,
						0, 1, 0, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 10:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 0, 0,
						1, 0, 1, 0,
						1, 1, 1, 0, 0 
					};
					break;
				case 11:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 1, 0, 
						0, 0, 1, 0,
						1, 1, 1, 0, 0
					};
					break;
				case 12:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						0, 0, 1, 0,
						0, 1, 1, 0,
						1, 1, 0, 0, 0
					};
					break;
				case 13:
					PentoDisplay = new int[] {
						0, 0, 1, 0,
						0, 1, 1, 0, 
						0, 0, 1, 0,
						0, 0, 1, 0, 0
					};
					break;
				case 14:
					PentoDisplay = new int[] {
						0, 1, 0, 0, 
						0, 1, 1, 0,  
						0, 1, 0, 0, 
						0, 1, 0, 0, 0
					};
					break;
				case 15:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 1, 1, 0, 
						0, 1, 0, 0, 
						1, 1, 0, 0, 0
					};
					break;
				case 16:
					PentoDisplay = new int[] {
						0, 0, 0, 0,
						1, 1, 0, 0,
						0, 1, 0, 0,
						0, 1, 1, 0, 0
					};
					break;
				case 17:
					PentoDisplay = new int[] {
						0, 0, 0, 0, 
						0, 0, 0, 0,
						1, 1, 1, 1,
						0, 0, 0, 0, 1
					};
					break;
			}
			for (int i = 0; i < 17; i++) {
				if (PentoDisplay[i] != 0) {
					ScreenGrid[i].SetActive (true);
				} else {
					ScreenGrid[i].SetActive(false);
				}
			}
		}
	}

    int GetPiece()
    {
        if (grabBag.Count == 0)
            grabBag = Enumerable.Range(0, 18).ToList();

        int index = Random.Range(0, grabBag.Count);
        int pieceType = upNext;
		upNext = grabBag[index]; 
        grabBag.RemoveAt(index);
        return pieceType;
    }

	void ApplyPentomino() {
		if (tetr != null) {
			List<IntPair> list = tetr.GetTileCoordinates ();
			if (tetr.isValid ()) {
				Score += 10;
				foreach (IntPair p in list) {
					GameBoard.set (p.x, p.y, 1);
				}

				List<int> rows = GameBoard.getCompletedRows ();
				if (rows.Count > 0) {
					GameBoard.deleteRows (rows);
				}
				linesLeft = linesLeft - rows.Count;
				if (linesLeft < 0) linesLeft = 0;
				switch (rows.Count) {
					case 1:
						Score += 50;
						break;
					case 2:
						Score += 150;
						break;
					case 3:
						Score += 350;
						break;
					case 4:
						Score += 1000;
						break;
				}
				if (linesLeft > 0) {
					tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
				} else {
					UpdateGrid ();
					Module.HandlePass ();
					modFrame.material = solvedMat;
					moduleSolved = true;
					tetr = null;
					timeDisplay.color = new Color (0, 255, 0);
					Debug.LogFormat("[Pentris Sprint #{0}] {1} is completed with a score of {2}, in {3}.", moduleId, targetDisplay.text, Score, elapsedTimeDisplay);
				}
				UpdateGrid ();
			}
            else
            {
                //Module.OnStrike();
                Score -= 200;
				elapsedTime += 20;
				if (Score < 0) Score = 0;
				GameBoard = new PentrisBoard(G_WIDTH, G_HEIGHT);
				tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
                UpdateGrid();
            }
		}
	}

	void Awake()
	{
		Module = GetComponent<KMBombModule> ();
        ModuleSelectable.OnInteract += delegate () { focused = true; return true; };
        ModuleSelectable.OnDefocus += delegate () { focused = false; };
        this.ModuleSelectable.OnInteract += delegate
        {
            if (!started)
            {
                OnActivation();
            }
            started = true;
            return true;
        };

		//Module.OnActivate += OnActivation;

		/*MoveLeftButton.OnInteract += delegate() { return MoveLeft (); };
		MoveRightButton.OnInteract += delegate() { return MoveRight (); };
		TurnLeftButton.OnInteract += delegate() { return TurnLeft (); };
		TurnRightButton.OnInteract += delegate() { return TurnRight (); };
		DownButton.OnInteract += delegate() { return Down (); };*/

		ObjectGrid = new GameObject[G_WIDTH, G_HEIGHT];

		GameBoard = new PentrisBoard (G_WIDTH, G_HEIGHT);

		// Populate the grid
		for (int x = 0; x < G_WIDTH; x++) {
			for (int y = 0; y < G_HEIGHT; y++) {
				CellObjects[x * 20 + y].SetActive (false);
				ObjectGrid [x, G_HEIGHT - y - 1] = CellObjects[x * 20 + y];
			}
		}

		tetr = null;
		upNext = Random.Range(0, 7);
		

		UpdateGrid ();

	}

	void Start()
	{
		moduleId = moduleIdCounter++;
	    linesLeft = FindThreshold(); 
		targetDisplay.text = linesLeft.ToString() + "L"; 
	}

	protected void OnActivation()
	{
		tetr = new Pentomino (G_WIDTH, GameBoard, GetPiece());
		UpdateGrid ();
	}

	void MoveLeft()
	{
		if (tetr != null) {
			tetr.MoveLeft ();
			UpdateGrid ();
		}
	}
	void MoveRight()
	{
		if (tetr != null) {
			tetr.MoveRight ();
			UpdateGrid ();
		}
	}

	bool TurnLeft()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (tetr != null) {
			tetr.TurnLeft ();
			UpdateGrid ();
		}
		return false;
	}

	bool TurnRight()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (tetr != null) {
			tetr.TurnRight ();
			UpdateGrid ();
		}
		return false;
	}

	bool Down()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		ApplyPentomino ();
		return false;
	}
	void Update() {
		if (started && !moduleSolved) {
			elapsedTime += Time.deltaTime;
			double milisecond = Math.Floor(Math.Round(elapsedTime % 60, 2) * 100 % 100);
			string milisecondDisp;
			if (milisecond < 10) milisecondDisp = "0" + milisecond.ToString();
			else milisecondDisp = milisecond.ToString();

			double second = Math.Floor(elapsedTime % 60);
			double minute = Math.Floor(elapsedTime / 60 % 60);
			double hour = Math.Floor(elapsedTime / 3600);

			if (hour != 0 && hour < 10)
			{
				if (minute < 10)
				{
					if (second < 10) elapsedTimeDisplay = "0" + hour + ":0" + minute + ":0" + second;
					else elapsedTimeDisplay = "0" + hour + ":0" + minute + ":" + second;
				}
				else
				{
					if (second < 10) elapsedTimeDisplay = "0" + hour + ":" + minute + ":0" + second;
					else elapsedTimeDisplay = "0" + hour + ":" + minute + ":" + second;
				}
			}
			else if (hour != 0)
			{
				if (minute < 10)
				{
					if (second < 10) elapsedTimeDisplay = hour + ":0" + minute + ":0" + second;
					else elapsedTimeDisplay = hour + ":0" + minute + ":" + second;
				}
				else
				{
					if (second < 10) elapsedTimeDisplay = hour + ":" + minute + ":0" + second;
					else elapsedTimeDisplay = hour + ":" + minute + ":" + second;
				}
			}
			else
			{
				if (minute < 10)
				{
					if (second < 10) elapsedTimeDisplay = "0" + minute + ":0" + second + "." + milisecondDisp;
					else elapsedTimeDisplay = "0" + minute + ":" + second + "." + milisecondDisp;
				}
				else
				{
					if (second < 10) elapsedTimeDisplay = minute + ":0" + second + "." + milisecondDisp;
					else elapsedTimeDisplay = minute + ":" + second + "." + milisecondDisp;
				}
			}
			timeDisplay.text = "T+" + elapsedTimeDisplay;
		}
        if (focused)
            for (int i = 0; i < TheKeys.Count(); i++)
            {
                if (Input.GetKeyDown(TheKeys[i]))
                {
                    handlePress(i);
                    buttonHeld[i] = true;
                }
                if (Input.GetKeyUp(TheKeys[i]))
                    buttonHeld[i] = false;
            }
    }


    private KeyCode[] TheKeys = {
        KeyCode.LeftArrow,  KeyCode.RightArrow, KeyCode.Z,  KeyCode.X,  KeyCode.UpArrow,  KeyCode.DownArrow,    KeyCode.Space, //KeyCode.C,
        KeyCode.A,          KeyCode.D,          KeyCode.Q,  KeyCode.E,  KeyCode.W,        KeyCode.S
    };
    private bool focused = false;
    private bool[] buttonHeld = new bool[13];

    void handlePress(int keypos)
    {
        if (tetr != null && focused)
        {
            if (keypos % 7 == 5 && TwitchPlaysActive) Down();
            else switch (keypos % 7)
                {         //KeyCode.LeftArrow, KeyCode.RightArrow,KeyCode.Z, KeyCode.X,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.Space
                    case 0: MoveLeft(); break;
                    case 1: MoveRight(); break;
                    case 2: TurnLeft(); break;
                    case 3: TurnRight(); break;
                    case 4: TurnRight(); break;
                    case 5: break;
                    case 6: Down(); break;
                }
            StartCoroutine(handleHeld(keypos));
        }
    }
    IEnumerator handleHeld(int keypos)
    {
        float heldTime = 0f;
        yield return null;
        while (buttonHeld[keypos] && !TwitchPlaysActive)
        {
            heldTime += Time.deltaTime;
            yield return null;
			if (heldTime >= .3f && !TwitchPlaysActive)
            {
                yield return null;
                switch (keypos % 7)
                {
                    case 0: MoveLeft(); break;
                    case 1: MoveRight(); break;
                    default: break;
                }
            }
        }
    }

    int FindThreshold()
    {
        try
        {
            ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(modSettings.Settings);
            if (settings != null)
            {
                if (settings.linesToClear < 5)
                    return 5;
                else if (settings.linesToClear > 10000)
                    return 10000;
                else return settings.linesToClear;
            }
            else return 20;
        }
        catch (JsonReaderException e)
        {
            Debug.LogFormat("[Pentris Sprint #{0}] JSON reading failed with error {1}, using default number.", moduleId, e.Message);
            return 20;
        }
    }
    bool TwitchPlaysActive;
    public readonly string TwitchHelpMessage = "Press keys with !{0} AZ DX; possible keys are WASD, ZX/QE, and S or _ or / to hard drop.";

    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToUpperInvariant().Replace(" ", "");
        if (!Regex.IsMatch(command, @"^[ZXQEWASD/_]+$")) yield break;
        else
        {
            yield return null;
            for (int i = 0; i < command.Length; i++)
            {
                yield return new WaitForSeconds(.1f);
                switch (command[i])
                {
                    case 'A': handlePress(0); break;
                    case 'D': handlePress(1); break;
                    case 'Z': handlePress(2); break;
                    case 'Q': handlePress(2); break;
                    case 'X': handlePress(3); break;
                    case 'E': handlePress(3); break;
                    case 'W': handlePress(4); break;
                    case 'S': handlePress(6); break;
                    case '/': handlePress(6); break;
                    case '_': handlePress(6); break;
                }
            }
        }
    }


}
