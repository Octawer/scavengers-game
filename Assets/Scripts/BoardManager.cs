using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Completed
{
    public class BoardManager : MonoBehaviour
    {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count
        {
            public int Minimum { get; set; }
            public int Maximum { get; set; }
        }

        public class GameBoard
        {
            public List<Vector3> OuterWallZoneBoardPositions { get; set; }
            public List<Vector3> SafeZoneBoardPositions { get; set; }
            public List<Vector3> SpawnZoneBoardPositions { get; set; }

            public int Cols { get; set; }
            public int Rows { get; set; }
            public int OuterWallOffset { get; set; }
            public int SafeZoneOffset { get; set; }

            
            public GameBoard(int rows, int cols, int outerWallOffset = 1, int safeZoneOffset = 1)
            {
                Rows = rows;
                Cols = cols;
                OuterWallOffset = outerWallOffset;
                SafeZoneOffset = safeZoneOffset;

                OuterWallZoneBoardPositions = new List<Vector3>();
                SafeZoneBoardPositions = new List<Vector3>();
                SpawnZoneBoardPositions = new List<Vector3>();

                PopulateBoardPositions();
            }

            /// <summary>
            /// RandomPosition returns a random position from our list gridPositions.
            /// </summary>
            /// <returns></returns>
            public Vector3 GetRandomBoardPosition()
            {
                int randomIndex = Random.Range(0, SpawnZoneBoardPositions.Count);
                Vector3 randomPosition = SpawnZoneBoardPositions[randomIndex];

                //Remove the entry at randomIndex from the list so that it can't be re-used.
                SpawnZoneBoardPositions.RemoveAt(randomIndex);

                return randomPosition;
            }

            private void PopulateBoardPositions()
            {
                for (int row = 0; row < Rows; row++)
                {
                    for (int col = 0; col < Cols; col++)
                    {
                        if (IsInSpawnZone(row, col))
                        {
                            SpawnZoneBoardPositions.Add(new Vector3(col, row));
                        }
                        else if (IsInSafeZone(row, col))
                        {
                            SafeZoneBoardPositions.Add(new Vector3(col, row));
                        }
                        else if (IsInOuterWall(row, col))
                        {
                            OuterWallZoneBoardPositions.Add(new Vector3(col, row));
                        }

                    }
                }
            }

            private bool IsInOuterWall(int row, int col) => row >= 0 && row < OuterWallOffset ||
                    row >= Rows - OuterWallOffset && row < Rows ||
                    col >= 0 && col < OuterWallOffset ||
                    col >= Cols - OuterWallOffset && col < Cols;

            private bool IsInSpawnZone(int row, int col) => row >= 0 + OuterWallOffset + SafeZoneOffset &&
                    row < Rows - OuterWallOffset - SafeZoneOffset &&
                    col >= 0 + OuterWallOffset + SafeZoneOffset &&
                    col < Cols - OuterWallOffset - SafeZoneOffset;

            private bool IsInSafeZone(int row, int col) => !IsInOuterWall(row, col) && !IsInSpawnZone(row, col);
        }


        
        public GameBoard TheGameBoard;

        public int GAMEBOARD_COLS = 8;
        public int GAMEBOARD_ROWS = 8;
        public int OUTER_WALL_OFFSET = 1;
        public int SAFE_ZONE_OFFSET = 1;

        public Count wallCount = new Count { Minimum = 5, Maximum = 9 };                      //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count { Minimum = 1, Maximum = 5 };                      //Lower and upper limit for our random number of food items per level.

        public GameObject exit;                                         //Prefab to spawn for exit.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.


        /// <summary>
        /// Sets up the outer walls and floor (background) of the game board.
        /// </summary>
        void BoardSetup()
        {
            TheGameBoard = new GameBoard(GAMEBOARD_ROWS, GAMEBOARD_COLS, OUTER_WALL_OFFSET, SAFE_ZONE_OFFSET);
            //Instantiate Board and set boardHolder to its transform.
            Transform boardHolder = new GameObject("Board").transform;

            TheGameBoard.OuterWallZoneBoardPositions.ForEach(position => CreateGameObjectAtPosition(outerWallTiles[Random.Range(0, outerWallTiles.Length)], position, boardHolder));
            TheGameBoard.SafeZoneBoardPositions.ForEach(position => CreateGameObjectAtPosition(floorTiles[Random.Range(0, floorTiles.Length)], position, boardHolder));
            TheGameBoard.SpawnZoneBoardPositions.ForEach(position => CreateGameObjectAtPosition(floorTiles[Random.Range(0, floorTiles.Length)], position, boardHolder));
        }

        private void CreateGameObjectAtPosition(GameObject gameObject, Vector3 position, Transform boardHolder)
        {
            GameObject instance = Instantiate(gameObject, new Vector3(position.x, position.y), Quaternion.identity);

            //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
            instance.transform.SetParent(boardHolder);
        }


        
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            int objectCount = Random.Range(minimum, maximum + 1);
            
            for (int i = 0; i < objectCount; i++)
            {
                Vector3 randomPosition = TheGameBoard.GetRandomBoardPosition();
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }


        /// <summary>
        /// SetupScene initializes our level and calls the previous functions to lay out the game board
        /// </summary>
        /// <param name="level"></param>
        public void SetupScene(int level)
        {
            BoardSetup();
            
            LayoutObjectAtRandom(wallTiles, wallCount.Minimum, wallCount.Maximum);
            LayoutObjectAtRandom(foodTiles, foodCount.Maximum, foodCount.Maximum);

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = (int)Mathf.Log(level, 2f);
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

            //Instantiate the exit tile in the upper right hand corner of our game board
            Instantiate(exit, new Vector3(GAMEBOARD_COLS - OUTER_WALL_OFFSET - SAFE_ZONE_OFFSET, GAMEBOARD_ROWS - OUTER_WALL_OFFSET - SAFE_ZONE_OFFSET, 0f), Quaternion.identity);
        }
    }
}