using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Maze : MonoBehaviour
{

    public GameObject wallPrefab;
    public int wallSize = 8;
    BitMapMaze m;
    void Start()
    {
        wallPrefab.tag = "wall";
        m = new BitMapMaze (wallSize);
        int width = (wallSize * 2) + 1;

        m.StartPoint(width + 1);
        for (int i = 0; i < width; i++){
            for (int j = 0; j < width; j++){
                if (m.grid[(i * width) + j] == 1) {
                    Object.Instantiate(wallPrefab, new Vector3 (j, 0, i), wallPrefab.transform.rotation);
                }
            }
        }
        m.Print();
    }

    void FixedUpdate(){
        m.PrimsStep();
        foreach (GameObject wall in GameObject.FindGameObjectsWithTag("wall")){
            if (wall != wallPrefab){
                Destroy(wall);
            }
        }
        int width = (wallSize * 2) + 1;
        for (int i = 0; i < width; i++){
            for (int j = 0; j < width; j++){
                if (m.grid[(i * width) + j] == 1) {
                    Object.Instantiate(wallPrefab, new Vector3 (j, 0, i), wallPrefab.transform.rotation);
                }
            }
        }
    }
}

/*

    [
        [1, 1, 1, 1, 1]
        [1, 1, 1, 1, 1]
        [1, 1, 1, 1, 1]
    ]

    let pixel = buffer[x][y];

    [
        1, 1, 1, 1, 1,
        1, 1, 1, 1, 1,
        1, 1, 1, 1, 1,
    ]

    let pixel = buffer[(y * width) + x]
*/

class BitMapMaze {
    public List<int> grid;
    private List<int> wallList;
    private List<int> visitedCells;
    int size;
    int width;
    public BitMapMaze (int size){
        this.size = size;
        width = (size * 2) + 1;
        var array = new int [width * width];
        for (int i = 0; i < width; i++){
            for (int j = 0; j < width; j++){
                if (i == 0 || i == width - 1 || j == 0 || j == width - 1){
                    array [(i * width) + j] = 1;
                }
                else if (j % 2 == 0) array [(i * width) + j] = 1;
                else if (i % 2 == 0) array [(i * width) + j] = 1;
            }
        }
        grid = new List<int>(array);
        wallList = new List<int>();
        visitedCells = new List<int>();
    }

    public void Print() {
        string result = "";
        for (int i = 0; i < width; i++){
            string row = "";
            for (int j = 0; j < width; j++){
                row += grid[(i * width) + j] + " ";
            }
            result += row + "\n";
        }
        Debug.Log(result);
    }

    // returns neighbouring walls of cells
    private (int, int, int, int) CellWalls (int cellIndex){
        int leftWall = cellIndex - 1;
        int rightWall  = cellIndex + 1;
        int topWall = cellIndex - width;
        int botWall = cellIndex + width;
        
        leftWall = grid [leftWall] == 0 ? - 1: leftWall;
        rightWall  = grid [rightWall] == 0 ? - 1: rightWall;
        topWall = grid [topWall] == 0 ? - 1: topWall;
        botWall = grid [botWall] == 0 ? - 1: botWall;
        Debug.Log(leftWall);
        Debug.Log(rightWall);
        Debug.Log(topWall);
        Debug.Log(botWall);
        
        return (leftWall, rightWall, topWall, botWall);

    }
    /*
    1 1 1 1 1 1 1
    1 0 1 0 1 0 1
    1 1 1 1 1 1 1
    */

    // returns cells that a wall divides
    private (int, int) WallCells (int wallIndex){
        int leftCell = wallIndex - 1;
        int rightCell  = wallIndex + 1;
        int topCell = wallIndex - width;
        int botCell = wallIndex + width;
        

        // if out of bounds in any dir make -1
        leftCell = leftCell < 0 || leftCell >= grid.Count ? -1 : leftCell;
        rightCell = rightCell < 0 || rightCell >= grid.Count ? -1 : rightCell;
        topCell = topCell < 0 || topCell >= grid.Count ? -1 : topCell;
        botCell = botCell < 0 || botCell >= grid.Count ? -1 : botCell;

        // checks if its wall
        leftCell = leftCell >= 0 && grid [leftCell] == 1 ? - 1: leftCell;
        rightCell  = rightCell >= 0 && grid [rightCell] == 1 ? - 1: rightCell;
        topCell = topCell >= 0 && grid [topCell] == 1 ? - 1: topCell;
        botCell = botCell >= 0 && grid [botCell] == 1 ? - 1: botCell;
        Debug.Log(leftCell);
        Debug.Log(rightCell);
        Debug.Log(topCell);
        Debug.Log(botCell);
        Debug.Log(rightCell != -1 && grid[rightCell] == 1);
        Debug.Log(leftCell != -1 && grid[leftCell] == 1);

        if ((rightCell != -1 && grid[rightCell] == 1) || (leftCell != -1 && grid[leftCell] == 1)){
            Debug.Log("safe");
            return (topCell, botCell);
        }
        else {return (leftCell, rightCell);}
    }

    public void PrimsStep(){
        if (wallList.Count > 0){
            int wallIndex = wallList[0];
            wallList.RemoveAt(0);
            (int cell1, int cell2) = WallCells(wallIndex);

            if (cell1 < 0 || cell2 < 0) return;

            bool cell1Visited = visitedCells.Contains(cell1) && !visitedCells.Contains(cell2);
            bool cell2Visited = visitedCells.Contains(cell2) && !visitedCells.Contains(cell1);

            if (cell1Visited || cell2Visited){
                grid[wallIndex] = 0;
                // pushes the unvisited cell
                int unvisitedCell = cell1Visited ? cell2 : cell1;
                visitedCells.Add(unvisitedCell);
                (int wall1, int wall2, int wall3, int wall4) = CellWalls(unvisitedCell);
                if (wall1 >= 0) wallList.Add(wall1); 
                if (wall2 >= 0) wallList.Add(wall2); 
                if (wall3 >= 0) wallList.Add(wall3); 
                if (wall4 >= 0) wallList.Add(wall4); 
            }
        }
    }

    public void StartPoint(int cellIndex){
        visitedCells.Add(cellIndex);
        (int wall1, int wall2, int wall3, int wall4) = CellWalls(cellIndex);
        if (wall2 >= 0) wallList.Add(wall2); 
        if (wall1 >= 0) wallList.Add(wall1); 
        if (wall3 >= 0) wallList.Add(wall3); 
        if (wall4 >= 0) wallList.Add(wall4); 
    }
}