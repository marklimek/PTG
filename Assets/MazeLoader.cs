using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeLoader : MonoBehaviour
{
	public int mazeRows, mazeColumns;
	public GameObject wall;
	public float size = 2f;

	public Cells[,] mazeCells;
	public System.Action OnMazeGenerated;

	void Start()
	{
		InitializeMaze();

		MazeAlgorithm ma = new HuntAndKillMazeAlgorithm(mazeCells);
		ma.CreateMaze();

		OnMazeGenerated?.Invoke();
	}

	void Update()
	{
	}

	private void InitializeMaze()
	{
		mazeCells = new Cells[mazeRows, mazeColumns];

		// Load realistic stone paving texture from Assets/Texture Bricks 'n Blocks/Textures
		Texture2D floorTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Texture Bricks 'n Blocks/Textures/Block_pavers_diffuseOriginal.png");

		// Create a realistic unlit desaturated material for both floor and walls
		Material floorGrayMat = new Material(Shader.Find("Custom/UnlitDesaturate"));
		floorGrayMat.name = "FloorGray";
		floorGrayMat.SetColor("_Color", new Color(0.85f, 0.85f, 0.88f)); // Clean light gray tint
		floorGrayMat.SetFloat("_Desaturation", 1.0f); // 100% greyscale
		if (floorTex != null)
		{
			floorGrayMat.mainTexture = floorTex;
		}

		for (int r = 0; r < mazeRows; r++)
		{
			for (int c = 0; c < mazeColumns; c++)
			{
				mazeCells[r, c] = new Cells();

				// Floor
				GameObject floorObj = Instantiate(wall, new Vector3(r * size, -(size / 2f), c * size), Quaternion.identity) as GameObject;
				floorObj.name = "Floor " + r + "," + c;
				floorObj.transform.Rotate(Vector3.right, 90f);
				mazeCells[r, c].floor = floorObj;

				Renderer floorRenderer = floorObj.GetComponent<Renderer>();
				if (floorRenderer != null)
				{
					floorRenderer.material = floorGrayMat;
				}

				// West Wall
				if (c == 0)
				{
					GameObject westWallObj = Instantiate(wall, new Vector3(r * size, 0, (c * size) - (size / 2f)), Quaternion.identity) as GameObject;
					westWallObj.name = "West Wall " + r + "," + c;
					Renderer rdr = westWallObj.GetComponent<Renderer>();
					if (rdr != null) rdr.material = floorGrayMat;
					mazeCells[r, c].westWall = westWallObj;
				}

				// East Wall
				GameObject eastWallObj = Instantiate(wall, new Vector3(r * size, 0, (c * size) + (size / 2f)), Quaternion.identity) as GameObject;
				eastWallObj.name = "East Wall " + r + "," + c;
				Renderer rdrEast = eastWallObj.GetComponent<Renderer>();
				if (rdrEast != null) rdrEast.material = floorGrayMat;
				mazeCells[r, c].eastWall = eastWallObj;

				// North Wall
				if (r == 0)
				{
					GameObject northWallObj = Instantiate(wall, new Vector3((r * size) - (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
					northWallObj.name = "North Wall " + r + "," + c;
					northWallObj.transform.Rotate(Vector3.up * 90f);
					Renderer rdrNorth = northWallObj.GetComponent<Renderer>();
					if (rdrNorth != null) rdrNorth.material = floorGrayMat;
					mazeCells[r, c].northWall = northWallObj;
				}

				// South Wall
				GameObject southWallObj = Instantiate(wall, new Vector3((r * size) + (size / 2f), 0, c * size), Quaternion.identity) as GameObject;
				southWallObj.name = "South Wall " + r + "," + c;
				southWallObj.transform.Rotate(Vector3.up * 90f);
				Renderer rdrSouth = southWallObj.GetComponent<Renderer>();
				if (rdrSouth != null) rdrSouth.material = floorGrayMat;
				mazeCells[r, c].southWall = southWallObj;
			}
		}
	}
}