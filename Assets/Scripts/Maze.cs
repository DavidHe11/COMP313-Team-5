﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour
{

    public IntVector2 size;

    public MazeCell cellPrefab;

    public float generationStepDelay;

    public MazePassage passagePrefab;
    public MazeWall wallPrefab;

    private MazeCell[,] cells;

    void Start()
    {

    }

    public IntVector2 RandomCoordinates
    {
        get
        {
            return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
        }
    }

    public bool ContainsCoordinates(IntVector2 coordinate)
    {
        return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
    }

    public MazeCell GetCell(IntVector2 coordinates)
    {
        return cells[coordinates.x, coordinates.z];
    }

    public IEnumerator Generate()
    {
        // term between each cell generation
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        PlayerSettings settings = PlayerPersistence.LoadData();

        // initialization when no stored values
        if (settings.MazeWidth == 0)
        {
            settings.MazeWidth = 10;
            PlayerPrefs.SetInt("mazeWidth", settings.MazeWidth);
            Debug.Log("MazeWidth Initialized: " + settings.MazeWidth);
        }
        if (settings.MazeHeight == 0)
        {
            settings.MazeHeight = 10;
            PlayerPrefs.SetInt("mazeHeight", settings.MazeHeight);
            Debug.Log("MazeHeight Initialized: " + settings.MazeHeight);
        }
        size.x = settings.MazeWidth;
        size.z = settings.MazeHeight;

        Debug.Log("MazeWidth Initialized: " + size.x);
        Debug.Log("MazeHeight Initialized: " + size.z);

        // initialize cell list
        cells = new MazeCell[size.x, size.z];
        List<MazeCell> activeCells = new List<MazeCell>();
        DoFirstGenerationStep(activeCells);
        while (activeCells.Count > 0)
        {
            yield return delay;
            DoNextGenerationStep(activeCells);
        }
    }

    private void DoFirstGenerationStep(List<MazeCell> activeCells)
    {
        activeCells.Add(CreateCell(RandomCoordinates));
    }

    private void DoNextGenerationStep(List<MazeCell> activeCells)
    {
        int currentIndex = activeCells.Count - 1;
        MazeCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            // If the cell is surrounded by 4 walls
            activeCells.RemoveAt(currentIndex);
            return;
        }
        MazeDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates))
        {
            MazeCell neighbor = GetCell(coordinates);
            if (neighbor == null)
            {
                neighbor = CreateCell(coordinates);
                CreatePassage(currentCell, neighbor, direction);
                activeCells.Add(neighbor);
            }
            else
            {
                CreateWall(currentCell, neighbor, direction);
                // activeCells.RemoveAt(currentIndex);
                // No longer need to remove the cell, (check has been put above)
            }
        }
        else
        {
            CreateWall(currentCell, null, direction);
            // activeCells.RemoveAt(currentIndex);
            // No longer need to remove the cell, (check has been put above)
        }
    }

    private MazeCell CreateCell(IntVector2 coordinates)
    {
        MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
        cells[coordinates.x, coordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
        return newCell;
    }

    private void CreatePassage(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
    }

    private void CreateWall(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazeWall wall = Instantiate(wallPrefab) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null)
        {
            wall = Instantiate(wallPrefab) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
    }
}