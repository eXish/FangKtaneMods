using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Random = UnityEngine.Random;
using TetrisSRS;

public class tetrisSprint : MonoBehaviour {
    public class ModSettingsJSON
    {
        public int linesToClear;
        public string note;
    }

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
    private const int G_WIDTH = 10; // Width of grid
    private const int G_HEIGHT = 20; // Height of grid

    private KMBombModule Module;
    private GameObject[,] ObjectGrid;
    public GameObject[] ScreenGrid;
    private int moduleId = 0;
    private static int moduleIdCounter = 1;
    private float elapsedTime;
    private bool started = false;
    private bool moduleSolved = false;

    //SRS
    private TetrisSRS.TetrisBoard _tetrisBoard;
    private TetrisPiece _currentPiece = null;
    public GameObject[] CellObjects;
    private List<TetrisSRS.Tetromino> _grabBag = new List<TetrisSRS.Tetromino>();
    private TetrisSRS.TetrisBoard _nextPieceBoard;
    private GameObject[,] _screenGrid;
    private TetrisPiece _nextPiece = null;
    void Awake()
    {
        Module = GetComponent<KMBombModule>();
        if (Application.isEditor)
            focused = true;
        ModuleSelectable.OnInteract += delegate () { focused = true; return true; };
        ModuleSelectable.OnDefocus += delegate () { focused = false; };
        ObjectGrid = new GameObject[G_WIDTH, G_HEIGHT];
        _screenGrid = new GameObject[4, 2];
        // Populate the grid
        for (int x = 0; x < G_WIDTH; x++)
        {
            for (int y = 0; y < G_HEIGHT; y++)
            {
                ObjectGrid[x, y] = CellObjects[G_HEIGHT * x + y];
                ObjectGrid[x, y].SetActive(false);
            }
        }
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                _screenGrid[x, y] = ScreenGrid[8 - (4 - x) - 4 * y];
                _screenGrid[x, y].SetActive(false);
            }
        }
        _tetrisBoard = new TetrisSRS.TetrisBoard(G_WIDTH, G_HEIGHT, ObjectGrid, BoxFull, BoxEmpty, BoxError, scoreDisplay, numberDisplay);
        _nextPieceBoard = new TetrisSRS.TetrisBoard(4, 2, _screenGrid, BoxFull, BoxEmpty, BoxError);


        ModuleSelectable.OnInteract += delegate
        {
            if (!started && !moduleSolved)
            {
                GetPiece();
            }
            started = true;
            return true;
        };

    }
    void Update()
    {
        if (started && !moduleSolved && (focused || TwitchPlaysActive))
			UpdateTime(Time.deltaTime);

		if (numberDisplay.text == "0" && !moduleSolved) Solve();

		if (focused)
			for (int i = 0; i < TheKeys.Count(); i++) {
				if (Input.GetKeyDown(TheKeys[i])) {
					handlePress(i);
					buttonHeld[i] = true;
				}
				if (Input.GetKeyUp(TheKeys[i])) 
					buttonHeld[i] = false;
			}
	}
	void UpdateTime(float time)
	{
		elapsedTime += time;
		double second = Math.Round(elapsedTime % 60, 2);
		double minute = Math.Floor(elapsedTime / 60 % 60);
		double hour   = Math.Floor(elapsedTime / 3600);

		timeDisplay.text = "T+" +
			(hour > 0 ?  hour.ToString("00")  + ":" + minute.ToString("00") + ":" + second.ToString("00")
				  	  : minute.ToString("00") + ":" + second.ToString("00.00"));
	}

	void Start()
	{
		moduleId = moduleIdCounter++;
		Module.OnActivate += delegate { int linesNeeded = FindThreshold(); targetDisplay.text = linesNeeded.ToString() + "L"; _tetrisBoard.ShowAndSetLinesCount(linesNeeded); };
		//Module.OnActivate += delegate { _tetrisBoard.ShowAndSetLinesCount(40); };
	}

	int FindThreshold()
	{
		try
		{
			ModSettingsJSON settings = JsonConvert.DeserializeObject<ModSettingsJSON>(modSettings.Settings);
			if (settings != null)
			{
				if (settings.linesToClear < 10)
					return 10;
				else if (settings.linesToClear > 10000)
					return 10000;
				else return settings.linesToClear;
			}
			else return 20;
		}
		catch (JsonReaderException e)
		{
			Debug.LogFormat("[Tetris Sprint #{0}] JSON reading failed with error {1}, using default number.", moduleId, e.Message);
			return 20;
		}
	}

	void GetPiece()
	{
		if (_grabBag.Count == 0)
			_grabBag = ((TetrisSRS.Tetromino[]) Enum.GetValues(typeof(TetrisSRS.Tetromino))).ToList();
		if (_nextPiece != null)
		{
			_currentPiece = new TetrisPiece(_nextPiece.PieceType, _tetrisBoard, 4, 19);
			_nextPieceBoard.Clear();
		}
		else
		{
			int firstPiece = Random.Range(0, _grabBag.Count);
			_currentPiece = new TetrisPiece(_grabBag[firstPiece], _tetrisBoard, 4, 19);
			_grabBag.RemoveAt(firstPiece);
		}
		if (!_currentPiece.CanSpawn)
		{
			UpdateTime(20);
			StartCoroutine(ResetBoard());
			return;
		}
		int newPiece = Random.Range(0, _grabBag.Count);
		_nextPiece = new TetrisPiece(_grabBag[newPiece], _nextPieceBoard, 1, 0);
		_grabBag.RemoveAt(newPiece);
		
	}
	IEnumerator ResetBoard()
	{
		Module.HandleStrike();
		started = false;
		_grabBag.Clear();
		_nextPiece = null;
		_currentPiece = null;
		_tetrisBoard.Clear();
        modFrame.material = strikedMat;
        yield return new WaitForSeconds(1f);
        modFrame.material = normalMat;
    }
	void SoftDrop()
	{
		if (_currentPiece != null) {
			if (_currentPiece.SoftDrop())
				GetPiece();
		}
		//GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		//if (!moduleSolved) _currentPiece.HardDrop();
	}
	void HardDrop()
	{
		if (_currentPiece != null)
		{
			_currentPiece.HardDrop();
			GetPiece();
		}
	}

	public void Solve() 
	{
		Module.HandlePass();
		for (int x = 0; x < 4; x++)
		{
			for (int y = 0; y < 2; y++)
			{
				_screenGrid[x, y] = ScreenGrid[8 - (4 - x) - 4 * y];
				_screenGrid[x, y].SetActive(false);
			}
		}
		modFrame.material = solvedMat;
		moduleSolved = true;
		started = false;
		_grabBag.Clear();
		_nextPiece = null;
		_currentPiece = null;
		_tetrisBoard.Clear();
		timeDisplay.color = new Color (0, 255, 0);
		//Debug.LogFormat("[Tetris Sprint #{0}] {1} is completed with a score of {2}, in {3}.", moduleId, targetDisplay.text, Score, elapsedTimeDisplay);				
	}

	private KeyCode[] TheKeys = {
        KeyCode.LeftArrow,  KeyCode.RightArrow, KeyCode.Z,  KeyCode.X,  KeyCode.UpArrow,  KeyCode.DownArrow,    KeyCode.Space, //KeyCode.C,
        KeyCode.A,          KeyCode.D,          KeyCode.Q,  KeyCode.E,  KeyCode.W,        KeyCode.S
    };
	private bool focused = false;
	private bool[] buttonHeld = new bool[13]; 
	private bool TwitchPlaysActive;

	void handlePress (int keypos) {
		if (_currentPiece != null && focused) {
			if (keypos % 7 == 5 && TwitchPlaysActive) SoftDrop(); 
			else switch (keypos % 7) {         //KeyCode.LeftArrow, KeyCode.RightArrow,KeyCode.Z, KeyCode.X,KeyCode.UpArrow,KeyCode.DownArrow,KeyCode.Space
				case 0: _currentPiece.MoveHorizontal(false); break;
				case 1:	_currentPiece.MoveHorizontal(true);  break;
				case 2:	_currentPiece.Rotate(false);  break;
				case 3:	_currentPiece.Rotate(true);   break;
				case 4:	_currentPiece.Rotate(true); _currentPiece.Rotate(true); break;
				case 5: break;
				case 6:	HardDrop(); break;
			}
			StartCoroutine(handleHeld(keypos));
		}
	}
	IEnumerator handleHeld(int keypos) {
		float heldTime = 0f;
		yield return null;
		while (buttonHeld[keypos] && !TwitchPlaysActive) {
			heldTime += Time.deltaTime;
			yield return null;
			if (keypos % 7 == 5 && !TwitchPlaysActive) {
				yield return new WaitForSeconds(.05f);
				SoftDrop();
			}	
			else if (heldTime >= .3f && !TwitchPlaysActive)
			{
				yield return null;
				switch (keypos % 7) { 
					case 0: _currentPiece.MoveHorizontal(false); break;
					case 1:	_currentPiece.MoveHorizontal(true);  break;
					default: break;
				}
			}
		}
	}
	public readonly string TwitchHelpMessage = "Press keys with !{0} AZ DX; possible keys are WASD, ZX/QE, and _ or / to hard drop.";
	public IEnumerator ProcessTwitchCommand(string command)
    {
		command = command.ToUpperInvariant().Replace(" ", "");
		if (!Regex.IsMatch(command, @"^[ZXQEWASD/_]+$")) yield break;
		else {
			yield return null;
			for (int i = 0; i < command.Length; i++) {
				yield return new WaitForSeconds(.1f);
				switch (command[i]) {
					case 'A': handlePress(0); break;
					case 'D': handlePress(1); break;
					case 'Z': handlePress(2); break;
					case 'Q': handlePress(2); break;
					case 'X': handlePress(3); break;
					case 'E': handlePress(3); break;
					case 'W': handlePress(4); break;
					case 'S': handlePress(5); break;
					case '/': handlePress(6); break;
					case '_': handlePress(6); break;
				}
			}
		}
    }
}
