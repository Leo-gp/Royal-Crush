using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Board_Network : MonoBehaviour
{
	public int width;
	public int height;
	public int borderSize;
	public GameObject tilePrefab;
	public GameObject[] gamePiecePrefabs;
	public float swapTime = 0.5f;
	public float collapseTime = 0.15f;
    public int fillYOffset = 10;
    public float fillMoveTime = 0.5f;
	public float manaPerPiece;
    public AudioSource pieceMoveSound;
    public AudioSource piecePopSound;

    [HideInInspector] public Tile_Network m_clickedTile;
	[HideInInspector] public Tile_Network m_targetTile;
	[HideInInspector] public bool m_playerInputEnabled = true;

    private Tile_Network[,] m_allTiles;
    private GamePiece_Network[,] m_allGamePieces;
    private Mana_Network[] m_allManas;
	private ParticleManager m_particleManager;

    void Start () 
	{
		m_allTiles = new Tile_Network[width,height];
		m_allGamePieces = new GamePiece_Network[width,height];
		m_allManas = FindObjectsOfType<Mana_Network> ();

		SetupTiles();

        FillBoard(fillYOffset,fillMoveTime);
		m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();
	}

	void MakeTile (GameObject prefab, int x, int y, int z = 0)
	{
        if (prefab !=null && IsWithinBounds(x,y))
		{
			GameObject tile = Instantiate (prefab, new Vector3 (x, y, z), Quaternion.identity) as GameObject;
			tile.name = "Tile (" + x + "," + y + ")";
			m_allTiles [x, y] = tile.GetComponent<Tile_Network> ();
			tile.transform.parent = transform;
			m_allTiles [x, y].Init (x, y, this);
		}
	}

    void MakeGamePiece (GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (prefab != null && IsWithinBounds(x,y))
        {
            prefab.GetComponent<GamePiece_Network>().Init(this);
            PlaceGamePiece(prefab.GetComponent<GamePiece_Network>(), x, y);

            if (falseYOffset != 0)
            {
                prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                prefab.GetComponent<GamePiece_Network>().Move(x, y, moveTime);
            }
            prefab.transform.parent = transform;
        }
    }

	void SetupTiles()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				if (m_allTiles[i,j] == null)
				{
					MakeTile (tilePrefab, i,j);
				}
			}
		}
	}
		
	GameObject GetRandomGamePiece()
	{
		int randomIdx = Random.Range(0, gamePiecePrefabs.Length);

		if (gamePiecePrefabs[randomIdx] == null)
		{
			Debug.LogWarning("BOARD:  " + randomIdx + "does not contain a valid GamePiece prefab!");
		}

		return gamePiecePrefabs[randomIdx];
	}

	public void PlaceGamePiece(GamePiece_Network gamePiece, int x, int y)
	{
		if (gamePiece == null)
		{
			Debug.LogWarning("BOARD: Invalid GamePiece!");
			return;
		}

		gamePiece.transform.position = new Vector3(x, y, 0);
		gamePiece.transform.rotation = Quaternion.identity;

		if (IsWithinBounds(x,y))
		{
			m_allGamePieces[x,y] = gamePiece;
		}

		gamePiece.SetCoord(x,y);
	}

	bool IsWithinBounds(int x, int y)
	{
		return (x >= 0 && x < width && y >= 0 && y < height);
	}

    GamePiece_Network FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
    {
        if (IsWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

            MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
            return randomPiece.GetComponent<GamePiece_Network>();
        }
        return null;
    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxIterations = 100;
        int iterations = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    FillRandomAt(i, j, falseYOffset, moveTime);
                    iterations = 0;

                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;

                        if (iterations >= maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece_Network> leftMatches = FindMatches(x, y, new Vector2(-1, 0), minLength);
        List<GamePiece_Network> downwardMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece_Network>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece_Network>();
        }

        return (leftMatches.Count > 0 || downwardMatches.Count > 0);

    }

	public void ClickTile(Tile_Network tile)
	{
		if (m_clickedTile == null)
		{
			m_clickedTile = tile;
		}
	}

	public void DragToTile(Tile_Network tile)
	{
		if (m_clickedTile !=null && IsNextTo(tile,m_clickedTile))
		{
			m_targetTile = tile;
		}
	}

	public void ReleaseTile()
	{
		if (m_clickedTile !=null && m_targetTile !=null)
		{
			SwitchTiles(m_clickedTile, m_targetTile);
		}

		m_clickedTile = null;
		m_targetTile = null;
	}
		
	void SwitchTiles(Tile_Network clickedTile, Tile_Network targetTile)
	{
		StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
	}

	IEnumerator SwitchTilesRoutine(Tile_Network clickedTile, Tile_Network targetTile)
	{
		m_playerInputEnabled = false;

        GamePiece_Network clickedPiece = m_allGamePieces[clickedTile.xIndex,clickedTile.yIndex];
        GamePiece_Network targetPiece = m_allGamePieces[targetTile.xIndex,targetTile.yIndex];

		if (targetPiece !=null && clickedPiece !=null)
		{
			clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
			targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

            pieceMoveSound.Play();

            yield return new WaitForSeconds(swapTime);

			List<GamePiece_Network> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
			List<GamePiece_Network> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

			if (targetPieceMatches.Count == 0 && clickedPieceMatches.Count == 0)
			{
				clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex,swapTime);
				targetPiece.Move(targetTile.xIndex, targetTile.yIndex,swapTime);
		        yield return new WaitForSeconds(swapTime);
		        m_playerInputEnabled = true;
			}
			else
			{
				yield return new WaitForSeconds(swapTime);
				ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
			}
		}
	}

	bool IsNextTo(Tile_Network start, Tile_Network end)
	{
		if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
		{
			return true;
		}

		if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
		{
			return true;
		}

		return false;
	}

	List<GamePiece_Network> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
	{
		List<GamePiece_Network> matches = new List<GamePiece_Network>();

        GamePiece_Network startPiece = null;

		if (IsWithinBounds(startX, startY))
		{
			startPiece = m_allGamePieces[startX, startY];
		}

		if (startPiece !=null)
		{
			matches.Add(startPiece);
		}
		else
		{
			return null;
		}

		int nextX;
		int nextY;

		int maxValue = (width > height) ? width: height;

		for (int i = 1; i < maxValue - 1; i++)
		{
			nextX = startX + (int) Mathf.Clamp(searchDirection.x,-1,1) * i;
			nextY = startY + (int) Mathf.Clamp(searchDirection.y,-1,1) * i;

			if (!IsWithinBounds(nextX, nextY))
			{
				break;
			}

            GamePiece_Network nextPiece = m_allGamePieces[nextX, nextY];

			if (nextPiece == null)
			{
				break;
			}
			else
			{
				if (nextPiece.type == startPiece.type && !matches.Contains(nextPiece))
				{
					matches.Add(nextPiece);
				}
				else
				{
					break;
				}
			}
		}

		if (matches.Count >= minLength)
		{
			return matches;
		}
			
		return null;
	}

	List<GamePiece_Network> FindVerticalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece_Network> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
		List<GamePiece_Network> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);

		if (upwardMatches == null)
		{
			upwardMatches = new List<GamePiece_Network>();
		}

		if (downwardMatches == null)
		{
			downwardMatches = new List<GamePiece_Network>();
		}

		var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}

	List<GamePiece_Network> FindHorizontalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece_Network> rightMatches = FindMatches(startX, startY, new Vector2(1,0), 2);
		List<GamePiece_Network> leftMatches = FindMatches(startX, startY, new Vector2(-1,0), 2);

		if (rightMatches == null)
		{
			rightMatches = new List<GamePiece_Network>();
		}

		if (leftMatches == null)
		{
			leftMatches = new List<GamePiece_Network>();
		}

		var combinedMatches = rightMatches.Union(leftMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}

	List<GamePiece_Network> FindMatchesAt (int x, int y, int minLength = 3)
	{
		List<GamePiece_Network> horizMatches = FindHorizontalMatches (x, y, minLength);
		List<GamePiece_Network> vertMatches = FindVerticalMatches (x, y, minLength);

		if (horizMatches == null) 
		{
			horizMatches = new List<GamePiece_Network> ();
		}

		if (vertMatches == null) 
		{
			vertMatches = new List<GamePiece_Network> ();
		}
		var combinedMatches = horizMatches.Union (vertMatches).ToList ();

		return combinedMatches;
	}

	List<GamePiece_Network> FindMatchesAt (List<GamePiece_Network> gamePieces, int minLength = 3)
	{
		List<GamePiece_Network> matches = new List<GamePiece_Network>();

		foreach (GamePiece_Network piece in gamePieces)
		{
			matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
		}
		return matches;

	}

	List<GamePiece_Network> FindAllMatches()
	{
		List<GamePiece_Network> combinedMatches = new List<GamePiece_Network>();

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				var matches = FindMatchesAt(i,j);
				combinedMatches = combinedMatches.Union(matches).ToList();
			}
		}
		return combinedMatches;
	}

	void ClearPieceAt(int x, int y)
	{
        GamePiece_Network pieceToClear = m_allGamePieces[x,y];

		if (pieceToClear !=null)
		{
			m_allGamePieces[x,y] = null;
			Destroy(pieceToClear.gameObject);
		}
	}

	void ClearBoard()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				ClearPieceAt(i,j);
			}
		}
	}

	void ClearPieceAt(List<GamePiece_Network> gamePieces)
	{
		foreach (GamePiece_Network piece in gamePieces)
		{
			if (piece !=null)
			{
				ClearPieceAt(piece.xIndex, piece.yIndex);
				if (m_particleManager !=null)
				{
					m_particleManager.ClearPieceFXAt(piece.xIndex,piece.yIndex);
				}
			}
		}
	}

	List<GamePiece_Network> CollapseColumn(int column, float collapseTime = 0.1f)
	{
		List<GamePiece_Network> movingPieces = new List<GamePiece_Network>();

		for (int i = 0; i < height - 1; i++)
		{
			if (m_allGamePieces[column,i] == null)
			{
				for (int j = i + 1; j < height; j++)
				{
					if (m_allGamePieces[column,j] !=null)
					{
						m_allGamePieces[column,j].Move(column, i, collapseTime * (j-i));

						m_allGamePieces[column,i] = m_allGamePieces[column,j];
						m_allGamePieces[column,i].SetCoord(column,i);

						if (!movingPieces.Contains(m_allGamePieces[column,i]))
						{
							movingPieces.Add(m_allGamePieces[column,i]);
						}

						m_allGamePieces[column,j] = null;

						break;
					}
				}
			}
		}
		return movingPieces;
	}

	List<GamePiece_Network> CollapseColumn(List<GamePiece_Network> gamePieces)
	{
		List<GamePiece_Network> movingPieces = new List<GamePiece_Network>();

		List<int> columnsToCollapse = GetColumns(gamePieces);

		foreach (int column in columnsToCollapse)
		{
			movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
		}

		return movingPieces;
	}

	List<int> GetColumns (List<GamePiece_Network> gamePieces)
	{
		List<int> columns = new List<int>();

		foreach (GamePiece_Network piece in gamePieces)
		{
			if (!columns.Contains(piece.xIndex))
			{
				columns.Add(piece.xIndex);
			}
		}
	
		return columns;
	}

	void ClearAndRefillBoard(List<GamePiece_Network> gamePieces)
	{
		StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
	}

	IEnumerator ClearAndRefillBoardRoutine(List<GamePiece_Network> gamePieces)
	{
		m_playerInputEnabled = false;

		List<GamePiece_Network> matches = gamePieces;

		do 
		{
			yield return StartCoroutine(ClearAndCollapseRoutine(matches));

			yield return null;

			yield return StartCoroutine(RefillRoutine());

			matches = FindAllMatches();

			yield return new WaitForSeconds(0.2f);

		}
		while (matches.Count != 0);

		m_playerInputEnabled = true;
	}

	IEnumerator ClearAndCollapseRoutine(List<GamePiece_Network> gamePieces)
	{
		List<GamePiece_Network> movingPieces = new List<GamePiece_Network>();
		List<GamePiece_Network> matches = new List<GamePiece_Network>();

		yield return new WaitForSeconds(0.2f);

		bool isFinished = false;

		while (!isFinished)
		{
			ClearPieceAt (gamePieces);
			GainManaAt (gamePieces);
            piecePopSound.Play();

            yield return new WaitForSeconds(collapseTime);

			movingPieces = CollapseColumn(gamePieces);
			while (!IsCollapsed(movingPieces))
			{
				yield return null;
			}
			yield return new WaitForSeconds(0.2f);

			matches = FindMatchesAt(movingPieces);

			if (matches.Count == 0)
			{
				isFinished = true;
				break;
			}
			else
			{
				yield return StartCoroutine(ClearAndCollapseRoutine(matches));
			}
		}
		yield return null;
	}

	IEnumerator RefillRoutine()
	{
        FillBoard(fillYOffset, fillMoveTime);

		yield return null;

	}

	bool IsCollapsed(List<GamePiece_Network> gamePieces)
	{
		foreach (GamePiece_Network piece in gamePieces)
		{
			if (piece !=null)
			{
				if (piece.transform.position.y - (float) piece.yIndex > 0.001f)
				{
					return false;
				}
			}
		}
		return true;
	}

	void GainManaAt(List<GamePiece_Network> gamePieces)
	{
		foreach (GamePiece_Network piece in gamePieces)
		{
			if (piece != null)
			{
                Mana_Network targetMana = null;

				foreach (Mana_Network mana in m_allManas) 
				{
					if (mana.manaType == piece.type) 
					{
						targetMana = mana;
						break;
					}
				}
						
				if (targetMana != null)
				{
					targetMana.GainMana (manaPerPiece);
				}
			}
		}
	}
}