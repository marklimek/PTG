using UnityEngine;
using System.Collections;

public abstract class MazeAlgorithm
{
	protected Cells[,] mazeCells;
	protected int mazeRows, mazeColumns;

	protected MazeAlgorithm(Cells[,] mazeCells) : base()
	{
		this.mazeCells = mazeCells;
		mazeRows = mazeCells.GetLength(0);
		mazeColumns = mazeCells.GetLength(1);
	}

	public abstract void CreateMaze();
}