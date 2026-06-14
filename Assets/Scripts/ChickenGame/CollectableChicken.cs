using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableChicken : MonoBehaviour
{
    private ChickenColor color;
    private int pointsValue;
    private Vector2Int currentCell;
    private Vector2Int targetCell;
    private Vector3 targetWorldPos;
    
    private float moveSpeed = 1.5f;
    private bool isMoving = false;
    private MazeLoader mazeLoader;

    private List<Vector2Int> pathHistory = new List<Vector2Int>();

    private Transform playerTransform;

    public void Initialize(ChickenColor chickenColor, int points, Vector2Int startCell)
    {
        color = chickenColor;
        pointsValue = points;
        currentCell = startCell;
        targetCell = startCell;
        
        mazeLoader = FindFirstObjectByType<MazeLoader>();
        
        if (color == ChickenColor.Blue)
        {
            moveSpeed = 2.5f;
        }
        else if (color == ChickenColor.White)
        {
            moveSpeed = 1.2f;
        }

        targetWorldPos = transform.position;
    }

    private void Start()
    {
        if (mazeLoader == null) mazeLoader = FindFirstObjectByType<MazeLoader>();
        
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (mazeLoader == null) return;

        if (playerTransform != null)
        {
            Vector3 flatChicken = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPlayer = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
            if (Vector3.Distance(flatChicken, flatPlayer) < 1.8f)
            {
                Collect();
                return;
            }
        }

        if (!isMoving)
        {
            ChooseNextTargetCell();
        }
        else
        {
            MoveTowardsTarget();
        }

        if (isMoving)
        {
            float bob = Mathf.Abs(Mathf.Sin(Time.time * 8f)) * 0.12f;
            float tilt = Mathf.Sin(Time.time * 12f) * 8f;
            
            Transform body = transform.Find("Body");
            if (body != null)
            {
                body.localPosition = new Vector3(0, bob, 0);
                body.localRotation = Quaternion.Euler(0f, 0f, tilt);
            }
        }
    }

    private void ChooseNextTargetCell()
    {
        List<Vector2Int> openNeighbors = GetOpenNeighbors(currentCell);
        if (openNeighbors == null || openNeighbors.Count == 0)
        {
            // Stuck!
            return;
        }

        Vector2Int chosenCell = openNeighbors[0];

        if (openNeighbors.Count > 1)
        {
            if (pathHistory.Count > 0)
            {
                Vector2Int lastCell = pathHistory[pathHistory.Count - 1];
                List<Vector2Int> nonBacktrackingNeighbors = new List<Vector2Int>();
                foreach (Vector2Int n in openNeighbors)
                {
                    if (n != lastCell) nonBacktrackingNeighbors.Add(n);
                }

                if (nonBacktrackingNeighbors.Count > 0)
                {
                    chosenCell = nonBacktrackingNeighbors[Random.Range(0, nonBacktrackingNeighbors.Count)];
                }
                else
                {
                    chosenCell = openNeighbors[Random.Range(0, openNeighbors.Count)];
                }
            }
            else
            {
                chosenCell = openNeighbors[Random.Range(0, openNeighbors.Count)];
            }
        }

        pathHistory.Add(currentCell);
        if (pathHistory.Count > 4) pathHistory.RemoveAt(0);

        targetCell = chosenCell;
        float size = mazeLoader.size;
        targetWorldPos = new Vector3(targetCell.x * size, transform.position.y, targetCell.y * size);

        Vector3 dir = (targetWorldPos - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        isMoving = true;
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.01f)
        {
            transform.position = targetWorldPos;
            currentCell = targetCell;
            isMoving = false;
        }
    }

    private List<Vector2Int> GetOpenNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (mazeLoader == null) return neighbors;

        int r = cell.x;
        int c = cell.y;
        int maxRows = mazeLoader.mazeRows;
        int maxCols = mazeLoader.mazeColumns;

        if (r > 0)
        {
            if (mazeLoader.mazeCells[r - 1, c].southWall == null)
            {
                neighbors.Add(new Vector2Int(r - 1, c));
            }
        }

        if (r < maxRows - 1)
        {
            if (mazeLoader.mazeCells[r, c].southWall == null)
            {
                neighbors.Add(new Vector2Int(r + 1, c));
            }
        }

        if (c < maxCols - 1)
        {
            if (mazeLoader.mazeCells[r, c].eastWall == null)
            {
                neighbors.Add(new Vector2Int(r, c + 1));
            }
        }

        if (c > 0)
        {
            if (mazeLoader.mazeCells[r, c - 1].eastWall == null)
            {
                neighbors.Add(new Vector2Int(r, c - 1));
            }
        }

        return neighbors;
    }

    private bool isCollected = false;

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        if (ChickenGameManager.Instance != null)
        {
            Color visualColor = Color.white;
            Transform body = transform.Find("Body");
            if (body != null)
            {
                Renderer r = body.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    visualColor = r.sharedMaterial.color;
                }
            }

            int doubledMultiplier = ChickenGameManager.Instance.HasDoublePointsBuff() ? 2 : 1;
            ChickenGameManager.Instance.OnChickenCollected(color, pointsValue);

            ChickenCollectionEffect.Create(transform.position, visualColor, pointsValue * doubledMultiplier);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name == "Player")
        {
            Collect();
        }
    }
}
