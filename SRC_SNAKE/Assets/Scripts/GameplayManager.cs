using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
	private enum EResultMove
	{
		EatSomething,
		Dead,
		Nothing,
	}

	private enum EDirection
	{
		Top,
		Right,
		Left,
		Bottom,
	}

	[SerializeField]
	private int m_xSize;
	[SerializeField]
	private int m_ySize;
	[SerializeField]
	private Vector2 m_uiTileSize;
	[SerializeField]
	private float m_durationTurn;
	[SerializeField]
	private TilePersistantData[] m_tileDatas;
	[SerializeField]
	private RectTransform m_gridContent;
	[SerializeField]
	private GameObject m_prefabUiSnakePart;
	[SerializeField]
	private GameObject m_prefabUiFood;
	[SerializeField]
	private RectTransform m_gameplayContainer;

	private Tile[,] m_grid;

	private List<SnakePart> m_snakePartInstances;
	private HashSet<int> m_tileAvailable;
	private IBaseEntity m_foodInstance;

	private EDirection m_lastDirectionSnake;
	private EDirection m_inputDirectionSnake;

	private void Awake()
	{
		m_snakePartInstances = new List<SnakePart>();
		Init();
	}

	private void Start()
	{
		StartCoroutine(GameplayLoopEnum());
	}

	private void Update()
	{
		ComputePlayerInput();
	}


	private void Init()
	{
		ConstructGrid();
		SpawnSnakePart(true, m_xSize / 2, m_ySize / 2);
		InitFood();
	}

	private void ConstructGrid()
	{
		m_grid = new Tile[m_ySize, m_xSize];
		m_tileAvailable = new HashSet<int>();
		var dataTileTypeOne = m_tileDatas[0];
		var dataTileTypeTwo = m_tileDatas[1];
		int count = 0;
		for (int i = 0; i < m_ySize; i++)
		{
			for (int j = 0; j < m_xSize; j++)
			{
				var data = Random.Range(0, 20) == 0 ? dataTileTypeTwo : dataTileTypeOne;
				var position = new Vector2(j * m_uiTileSize.x, i * m_uiTileSize.y);
				m_grid[i, j] = new Tile(data, CreateUiTile(data, position, (++count) % 2 == 0));
				m_tileAvailable.Add(i * m_xSize + j);
			}
		}
	}
	private RectTransform CreateUiTile(TilePersistantData data, Vector2 positionAnchored, bool debugColor)
	{
		var instance = new GameObject("Tile", typeof(RectTransform), typeof(Image));
		var rt = instance.transform as RectTransform;
		rt.SetParent(m_gridContent);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.zero;
		rt.pivot = Vector2.zero;
		rt.sizeDelta = m_uiTileSize;
		rt.anchoredPosition = positionAnchored;
		var img = instance.GetComponent<Image>();
		img.sprite = data.Terrain;
		if (debugColor)
			img.color = Color.gray;
		return rt;
	}

	private void SpawnSnakePart(bool isHead, int xPosition, int yPosition)
	{
		var rt = SpawnPrefab(m_prefabUiSnakePart, xPosition, yPosition);
		var script = new SnakePart(isHead, rt);
		var position = GetPosition(xPosition, yPosition);
		script.Move(position, xPosition, yPosition);
		m_snakePartInstances.Add(script);
		SetEntityOnTile(xPosition, yPosition, script);
	}

	private RectTransform SpawnPrefab(GameObject prefab, int xPosition, int yPosition)
	{
		var instance = GameObject.Instantiate(prefab, m_gameplayContainer);
		var rt = instance.transform as RectTransform;
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.zero;
		rt.pivot = Vector2.zero;

		rt.anchoredPosition = GetPosition(xPosition, yPosition);

		return rt;
	}

	private Vector2 GetPosition(int x, int y)
	{
		return new Vector2(x * m_uiTileSize.x, y * m_uiTileSize.y);
	}

	private void SetEntityOnTile(int xPos, int yPos, IBaseEntity entity)
	{
		var hasPreviousEntity = m_grid[yPos, xPos].RefEntity != null;
		var isNewEntityNull = entity == null;
		m_grid[yPos, xPos].RefEntity = entity;

		var position = yPos * m_xSize + xPos;
		if (!hasPreviousEntity && !isNewEntityNull)
			m_tileAvailable.Remove(position);

		if (hasPreviousEntity && isNewEntityNull)
			m_tileAvailable.Add(position);
	}

	private EResultMove MoveSnake()
	{
		var head = m_snakePartInstances[0];
		var xNewPos = head.XPosition;
		var yNewPos = head.YPosition;
		var xOldPos = xNewPos;
		var yOldPos = yNewPos;

		m_lastDirectionSnake = m_inputDirectionSnake;

		switch (m_lastDirectionSnake)
		{
			case EDirection.Top:
				yNewPos = (yNewPos + 1) % m_ySize;
				break;
			case EDirection.Right:
				xNewPos = (xNewPos + 1) % m_ySize;
				break;
			case EDirection.Left:
				xNewPos = (xNewPos - 1 + m_ySize) % m_ySize;
				break;
			case EDirection.Bottom:
				yNewPos = (yNewPos - 1 + m_ySize) % m_ySize;
				break;
			default:
				break;
		}

		var position = GetPosition(xNewPos, yNewPos);
		var data = m_grid[yNewPos, xNewPos];
		var tileData = data.Data;
		var previousEntity = data.RefEntity;
		head.Move(position, xNewPos, yNewPos);
		SetEntityOnTile(xNewPos, yNewPos, head);

		for (int i = 1, iLength = m_snakePartInstances.Count; i < iLength; ++i)
		{
			var part = m_snakePartInstances[i];

			xNewPos = xOldPos;
			yNewPos = yOldPos;
			xOldPos = part.XPosition;
			yOldPos = part.YPosition;

			position = GetPosition(xNewPos, yNewPos);
			part.Move(position, xNewPos, yNewPos);
			SetEntityOnTile(xNewPos, yNewPos, part);
		}

		if (tileData.Type == TilePersistantData.ETileType.Wall)
			return EResultMove.Dead;

		if (previousEntity != null)
		{
			var result = previousEntity.Eat();

			switch (result)
			{
				case EEatResult.AddSnakePart:
					GrowSnake(xOldPos, yOldPos);
					return EResultMove.EatSomething;
				case EEatResult.Death:
					return EResultMove.Dead;
				default:
					SetEntityOnTile(xOldPos, yOldPos, null);
					break;
			}
		}
		else
		{
			SetEntityOnTile(xOldPos, yOldPos, null);
		}

		return EResultMove.Nothing;
	}

	private void InitFood()
	{
		var randomIndex = UnityEngine.Random.Range(0, m_tileAvailable.Count);
		var randomPos = m_tileAvailable.ElementAt(randomIndex);

		var yPos = randomPos / m_xSize;
		var xPos = randomPos % m_xSize;
		var position = GetPosition(xPos, yPos);
		var rt = SpawnPrefab(m_prefabUiFood, xPos, yPos);
		m_foodInstance = new BaseFood(rt);
		m_foodInstance.Move(position, xPos, yPos);
		SetEntityOnTile(xPos, yPos, m_foodInstance);
	}

	private void SpawnFood()
	{
		var randomIndex = UnityEngine.Random.Range(0, m_tileAvailable.Count);
		var randomPos = m_tileAvailable.ElementAt(randomIndex);

		var yPos = randomPos / m_xSize;
		var xPos = randomPos % m_xSize;
		var position = GetPosition(xPos, yPos);

		m_foodInstance.Move(position, xPos, yPos);
		SetEntityOnTile(xPos, yPos, m_foodInstance);
	}

	private void GrowSnake(int xPos, int yPos)
	{
		SpawnSnakePart(false, xPos, yPos);
	}

	private void LaunchFailureGame()
	{

	}

	private void LaunchSuccessGame()
	{

	}

	private void ClearGame()
	{

	}

	private void ResetGame()
	{

	}

	private void ComputePlayerInput()
	{
		if (m_lastDirectionSnake != EDirection.Top && Input.GetKeyDown(KeyCode.DownArrow))
			m_inputDirectionSnake = EDirection.Bottom;
		if (m_lastDirectionSnake != EDirection.Bottom && Input.GetKeyDown(KeyCode.UpArrow))
			m_inputDirectionSnake = EDirection.Top;
		if (m_lastDirectionSnake != EDirection.Left && Input.GetKeyDown(KeyCode.RightArrow))
			m_inputDirectionSnake = EDirection.Right;
		if (m_lastDirectionSnake != EDirection.Right && Input.GetKeyDown(KeyCode.LeftArrow))
			m_inputDirectionSnake = EDirection.Left;
	}

	private IEnumerator GameplayLoopEnum()
	{
		yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.A));
		
		var waitTime = new WaitForSeconds(m_durationTurn);
		while (true)
		{
			var result = MoveSnake();


			if (result == EResultMove.EatSomething)
			{
				SpawnFood();
			}
			else if (result == EResultMove.Dead)
			{
				break;
			}
			else
			{

			}

			yield return waitTime;
		}
	}

}
