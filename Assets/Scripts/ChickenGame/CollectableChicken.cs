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

    private bool isPecking = false;
    private int totalPecks = 0;
    private int currentPeckIndex = 0;
    private float peckTime = 0f;
    private float peckSpeed = 3.2f; // Fast, energetic peck

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

        if (isPecking)
        {
            UpdatePeckingAnimation();
        }
        else if (!isMoving)
        {
            ChooseNextTargetCell();
        }
        else
        {
            MoveTowardsTarget();
            UpdateWalkingAnimation();
        }
    }

    private void StartPecking()
    {
        isPecking = true;
        totalPecks = Random.Range(2, 5); // 2 to 4 pecks
        currentPeckIndex = 0;
        peckTime = 0f;

        // Reset walking animation positions
        Transform body = transform.Find("Body");
        if (body != null)
        {
            body.localPosition = Vector3.zero;
            body.localRotation = Quaternion.identity;
        }
    }

    private void UpdatePeckingAnimation()
    {
        peckTime += Time.deltaTime * peckSpeed;
        if (peckTime >= 1.0f)
        {
            peckTime = 0f;
            currentPeckIndex++;
            if (currentPeckIndex >= totalPecks)
            {
                isPecking = false;
                // Reset HeadGroup rotation
                Transform hg = transform.Find("HeadGroup");
                if (hg != null)
                {
                    hg.localPosition = new Vector3(0f, 0.15f, 0.15f);
                    hg.localRotation = Quaternion.identity;
                }
                return;
            }
        }

        // peckTime goes from 0 to 1
        float angle = 0f;
        float bodyAngle = 0f;
        float bodyY = 0f;

        if (peckTime < 0.4f)
        {
            float t = peckTime / 0.4f;
            angle = Mathf.Lerp(0f, 55f, t * t); // Swiftly tilt head down
            bodyAngle = Mathf.Lerp(0f, 12f, t * t); // Lean body forward
            bodyY = Mathf.Lerp(0f, -0.04f, t * t);
        }
        else if (peckTime < 0.6f)
        {
            float t = (peckTime - 0.4f) / 0.2f;
            angle = 55f + Mathf.Sin(t * Mathf.PI) * 4f; // peck/nudge the ground
            bodyAngle = 12f;
            bodyY = -0.04f;
        }
        else
        {
            float t = (peckTime - 0.6f) / 0.4f;
            angle = Mathf.Lerp(55f, 0f, t); // Return back up
            bodyAngle = Mathf.Lerp(12f, 0f, t);
            bodyY = Mathf.Lerp(-0.04f, 0f, t);
        }

        Transform headGroup = transform.Find("HeadGroup");
        if (headGroup != null)
        {
            headGroup.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        Transform body = transform.Find("Body");
        if (body != null)
        {
            body.localRotation = Quaternion.Euler(bodyAngle, 0f, 0f);
            body.localPosition = new Vector3(0f, bodyY, 0f);
        }
    }

    private void UpdateWalkingAnimation()
    {
        float bob = Mathf.Abs(Mathf.Sin(Time.time * 8f)) * 0.12f;
        float tilt = Mathf.Sin(Time.time * 12f) * 8f;
        
        Transform body = transform.Find("Body");
        if (body != null)
        {
            body.localPosition = new Vector3(0, bob, 0);
            body.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        // Bob the head slightly out of phase with the body for extra cuteness and life-likeness
        Transform headGroup = transform.Find("HeadGroup");
        if (headGroup != null)
        {
            float headBob = Mathf.Sin(Time.time * 8f - 1f) * 0.05f;
            headGroup.localPosition = new Vector3(0f, 0.15f + headBob, 0.15f);
            headGroup.localRotation = Quaternion.Euler(Mathf.Sin(Time.time * 8f) * 5f, 0f, 0f);
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

            // 75% chance to stop and peck the ground
            if (Random.value < 0.75f)
            {
                StartPecking();
            }
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
