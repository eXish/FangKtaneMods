using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class TetType {
	public int[] map;
	public int width;

	public TetType(int[] map, int width)
	{
		this.map = map;
		this.width = width;
	}
}

public class Pentomino {

	private static int[] FLEFT_MAP = {
		0, 1, 1,
		1, 1, 0,
		0, 1, 0
	};

	private static int[] FRIGHT_MAP = {
		1, 1, 0,
		0, 1, 1,
		0, 1, 0
	};

	private static int[] LLEFT_MAP = {
		0, 1, 0, 0,
		0, 1, 0, 0,
		0, 1, 0, 0,
		1, 1, 0, 0
	};

	private static int[] LRIGHT_MAP = {
		0, 1, 0, 0,
		0, 1, 0, 0,
		0, 1, 0, 0,
		0, 1, 1, 0
	};
	private static int[] PLEFT_MAP = {
		1, 1, 0,
		1, 1, 0,
		0, 1, 0
	};

	private static int[] PRIGHT_MAP = {
		0, 1, 1,
		0, 1, 1,
		0, 1, 0
	};

	private static int[] NLEFT_MAP = {
		0, 1, 0, 0,
		0, 1, 0, 0,
		1, 1, 0, 0,
		1, 0, 0, 0
	};

	private static int[] NRIGHT_MAP = {
		0, 1, 0, 0,
		0, 1, 0, 0,
		0, 1, 1, 0,
		0, 0, 1, 0
	};

	private static int[] CROSS_MAP = {
		0, 1, 0,
		1, 1, 1,
		0, 1, 0
	};
	private static int[] T_MAP = {
		1, 1, 1,
		0, 1, 0,
		0, 1, 0
	};
	private static int[] U_MAP = {
		0, 0, 0,
		1, 0, 1,
		1, 1, 1
	};
	private static int[] V_MAP = {
		0, 0, 1,
		0, 0, 1,
		1, 1, 1
	};
	private static int[] W_MAP = {
		0, 0, 1,
		0, 1, 1,
		1, 1, 0
	};
	private static int[] YLEFT_MAP = {
		0, 1, 0, 0,
		1, 1, 0, 0,
		0, 1, 0, 0,
		0, 1, 0, 0
	};

	private static int[] YRIGHT_MAP = {
		0, 1, 0, 0,
		0, 1, 1, 0,
		0, 1, 0, 0,
		0, 1, 0, 0
	};
	private static int[] ZLEFT_MAP = {
		0, 1, 1,
		0, 1, 0,
		1, 1, 0
	};

	private static int[] ZRIGHT_MAP = {
		1, 1, 0,
		0, 1, 0,
		0, 1, 1
	};

	private static int[] STRAIGHT_MAP = {
		0, 0, 0, 0, 0,
		1, 1, 1, 1, 1,
		0, 0, 0, 0, 0,
		0, 0, 0, 0, 0,
		0, 0, 0, 0, 0
	};


	private static TetType FLEFT = new TetType (FLEFT_MAP, 3);
	private static TetType FRIGHT = new TetType (FRIGHT_MAP, 3);
	private static TetType LLEFT = new TetType (LLEFT_MAP, 4);
	private static TetType LRIGHT = new TetType (LRIGHT_MAP, 4);
	private static TetType PLEFT = new TetType (PLEFT_MAP, 3);
	private static TetType PRIGHT = new TetType (PRIGHT_MAP, 3);
	private static TetType NLEFT = new TetType (NLEFT_MAP, 4);
	private static TetType NRIGHT = new TetType (NRIGHT_MAP, 4);
	private static TetType CROSS = new TetType (CROSS_MAP, 3);
	private static TetType T = new TetType (T_MAP, 3);
	private static TetType U = new TetType (U_MAP, 3);
	private static TetType V = new TetType (V_MAP, 3);
	private static TetType W = new TetType (W_MAP, 3);
	private static TetType YLEFT = new TetType (YLEFT_MAP, 4);
	private static TetType YRIGHT = new TetType (YRIGHT_MAP, 4);
	private static TetType ZLEFT = new TetType (ZLEFT_MAP, 3);
	private static TetType ZRIGHT = new TetType (ZRIGHT_MAP, 3);
	private static TetType STRAIGHT = new TetType (STRAIGHT_MAP, 5);

	private TetType type;
	private int rotation = 0;
	private int column = 0;
	private int height;
	private int gridWidth;
	private PentrisBoard board;
	private bool valid;

	public Pentomino(int gridWidth, PentrisBoard board, int i)
	{
		switch (i) {
		case 0:
			type = FLEFT;
			break;
		case 1:
			type = FRIGHT;
			break;
		case 2:
			type = LLEFT;
			break;
		case 3:
			type = LRIGHT;
			break;
		case 4:
			type = PLEFT;
			break;
		case 5:
			type = PRIGHT;
			break;
		case 6:
			type = NLEFT;
			break;
		case 7:
			type = NRIGHT;
			break;
		case 8:
			type = CROSS;
			break;
		case 9:
			type = T;
			break;
		case 10:
			type = U;
			break;
		case 11:
			type = V;
			break;
		case 12:
			type = W;
			break;
		case 13:
			type = YLEFT;
			break;
		case 14:
			type = YRIGHT;
			break;
		case 15:
			type = ZLEFT;
			break;
		case 16:
			type = ZRIGHT;
			break;
		case 17:
			type = STRAIGHT;
			break;
		};
		//rotation = Random.Range (0, 4);
		column = 3;
		this.gridWidth = gridWidth;
		this.board = board;

		ResolvePosition ();
	}

	private void ResolvePosition()
	{
		while (HasColumn (-1)) {
			column++;
		}

		while (HasColumn (gridWidth)) {
			column--;
		}

		height = 2*gridWidth;
		int thisHeight;
		for (int x = column; x < column + type.width; x++) {
			if (!HasColumn (x)) {
				continue;
			}

			thisHeight = board.getHighest (x) - GetLowest (x) - 1;
			if (thisHeight < height) {
				height = thisHeight;
			}
		}
	}

	public bool HasColumn(int c)
	{
		if(c < column || c >= column + type.width) {
			return false;
		}

		int x = c - column;
		for(int y = 0; y < type.width; y++) {
			if (getMap (x, y) != 0) {
				return true;
			}
		}

		return false;
	}

	public int GetLowest(int c)
	{
		if (!HasColumn (c)) {
			return 0;
		}

		int x = c - column;
		for (int y = type.width - 1; y >= 0; y--) {
			if (getMap (x, y) != 0) {
				return y;
			}
		}



		return 0;
	}

	public void MoveLeft()
	{
		column--;

		ResolvePosition ();
	}

	public void MoveRight()
	{
		column++;

		ResolvePosition ();
	}

	public void TurnLeft()
	{
		rotation--;
		if (rotation < 0) {
			rotation = 3;
		}

		ResolvePosition ();
	}

	public void TurnRight()
	{
		rotation++;
		if (rotation > 3) {
			rotation = 0;
		}

		ResolvePosition ();
	}

	private int index(int x, int y)
	{
		return type.map[x + y * type.width];
	}

	public int getMap(int x, int y)
	{
		int x2;
		int y2;

		switch (rotation)
		{
			case 1:
				x2 = y;
				y2 = type.width - x - 1;
				break;
			case 2:
				x2 = type.width - x - 1;
				y2 = type.width - y - 1;
				break;
			case 3:
				x2 = type.width - y - 1;
				y2 = x;
				break;
			default:
				x2 = x;
				y2 = y;
				break;
		}

		return index(x2, y2);
	}

	public List<IntPair> GetTileCoordinates()
	{
		valid = true;
		List<IntPair> list = new List<IntPair> ();
		for (int y = 0; y < type.width; y++) {
			for (int x = 0; x < type.width; x++) {
				if (getMap(x, y) != 0) {
					if (height + y < 0) {
						valid = false;
					} else {
						list.Add (new IntPair (x + column, y + height));
					}
				}
			}
		}
		return list;
	}

	public bool isValid()
	{
		return valid;
	}
}
