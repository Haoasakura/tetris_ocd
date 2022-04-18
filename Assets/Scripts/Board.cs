using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour {
    private Tilemap tilemap;
    private Piece activePiece;

    [SerializeField] private Tetrominoes tetrominoes;

    [SerializeField] private Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    private int nextPiece;

    [SerializeField] private string nextTetromino; //temp
    [SerializeField] private int frozenRows = 0;

    [SerializeField] private Vector3[] positionToTest = new Vector3[4];
    public LayerMask layerMask;

    private Transform[,] grid = new Transform[10, 20];
    private Vector2Int gridOffset;

    public RectInt Bounds {
        get {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            RectInt r = new RectInt(position, boardSize);
            r.yMin += frozenRows;
            return r;
        }
    }


    private void Awake() {

        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();


        tetrominoes.Initialize();
        gridOffset = new Vector2Int(grid.GetLength(0) / 2, grid.GetLength(1) / 2);
        for (int row = 0; row < grid.GetLength(0); row++) {
            for (int col = 0; col < grid.GetLength(1); col++) {
                grid[row, col] = null;
            }
        }

    }

    private void Start() {
        nextPiece = Random.Range(0, tetrominoes.tetrominoesData.Length - 1);
        SpawnPiece();
    }

    private void LateUpdate() {
        // UpdateGrid();
    }

    public void SpawnPiece() {
        int random = nextPiece;
        nextPiece = Random.Range(0, tetrominoes.tetrominoesData.Length - 1);

        TetrominoData data = tetrominoes.tetrominoesData[random];
        TetrominoData nextData = tetrominoes.tetrominoesData[nextPiece];
        nextTetromino = nextData.tetromino.ToString();
        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPositionC(activePiece, spawnPosition)) {
            SetPiece(activePiece);
        } else {
            GameOver();
        }

    }

    public void UpdateGrid() {
        // Transform subBlock=null;
        // for (int i = 0; i < activePiece.cells.Length; i++) {
        //     Vector3Int tileOldPosition = activePiece.cells[i] + activePiece.oldPosition;

        //     Vector2Int gridOldPos = (Vector2Int)tileOldPosition + gridOffset;
        //     subBlock=grid[gridOldPos.x, gridOldPos.y];
        //     grid[gridOldPos.x, gridOldPos.y] = null;
        // }
        // for (int i = 0; i < activePiece.cells.Length; i++) {
        //     Vector3Int tileNewPosition = activePiece.cells[i] +  activePiece.position;

        //     Vector2Int gridNewPos = (Vector2Int)tileNewPosition + gridOffset;
        //     grid[gridNewPos.x, gridNewPos.y] = subBlock;
        // }

        for (int row = 0; row < grid.GetLength(0); row++) {
            for (int col = 0; col < grid.GetLength(1); col++) {
                Vector2 pos = new Vector2(row, col) - gridOffset + new Vector2(.5f, .5f);

                Collider2D coll = Physics2D.OverlapBox(pos, new Vector2(.95f, .95f), 0f);
                if (coll != null)
                    grid[row, col] = coll.transform;
                else
                    grid[row, col] = null;
            }
        }
    }

    public void SetPiece(Piece piece) {

        piece.pieceRef.transform.localPosition = piece.position;

        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tileNewPosition = piece.cells[i] + piece.position;

            Vector2Int gridNewPos = (Vector2Int)tileNewPosition + gridOffset;
            grid[gridNewPos.x, gridNewPos.y] = piece.pieceRef.transform.GetChild(i);
            grid[gridNewPos.x, gridNewPos.y].localPosition = piece.cells[i] + new Vector3(0.5f, 0.5f);
        }

        // UpdateGrid();
    }

    public void ClearPiece(Piece piece) {
        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            Vector2Int gridNewPos = (Vector2Int)tilePosition + gridOffset;
            grid[gridNewPos.x, gridNewPos.y] = null;
        }
        // UpdateGrid();
    }

    public void ClearLines() {

        int row = 0;

        while (row < grid.GetLength(1)) {
            if (IsLineFull(row)) {
                LineClear(row);
            } else {
                row++;
            }
        }
    }

    private void LineClear(int row) {
        print("Line Clear " + row);

        for (int col = 0; col < grid.GetLength(0); col++) {
            if (grid[col, row] != null) {
                DestroyImmediate(grid[col, row].gameObject);
                grid[col, row] = null;
            }
        }

        while (row < (grid.GetLength(1) - 1)) {
            for (int col = 0; col < grid.GetLength(0); col++) {
                if (grid[col, (row + 1)] != null) {
                    grid[col, row] = grid[col, (row + 1)];
                    grid[col, (row + 1)] = null;
                    grid[col, row].localPosition += Vector3Int.down;
                }
            }
            row++;
        }

    }

    private void GameOver() {
        tilemap.ClearAllTiles();
        frozenRows = 0;
        activePiece.stepDelay = activePiece.initialStepDelay;
        activePiece.Reset();
    }

    public bool IsValidPositionB(Piece piece) {

        for (int row = Bounds.yMin; row < Bounds.yMax; row++) {
            for (int col = Bounds.xMin; col < Bounds.xMax; col++) {
                Vector3 position = new Vector3Int(col, row) + new Vector3(.5f, -.5f);

                Collider2D coll = Physics2D.OverlapBox(position, new Vector2(.95f, .9f), 0f);
                if (coll != null)
                    print(position + " HIT");
                else
                    print(position + " MISS");

            }
        }

        // for (int i = 0; i < piece.cells.Length; i++) {
        //     Vector3Int tilePosition = piece.cells[i] + position;

        //     if (!Bounds.Contains((Vector2Int)tilePosition)) {
        //         return false;
        //     }

        //     if (tilemap.HasTile(tilePosition)) {
        //         return false;
        //     }

        // }

        return true;
    }

    public bool IsValidPositionC(Piece piece, Vector3Int position) {

        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3 tilePosition = piece.cells[i] + position;
            tilePosition += new Vector3(.5f, .5f);
            positionToTest[i] = tilePosition;
        }

        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3 tilePosition = piece.cells[i] + position;

            Vector3Int tilePositionInt = piece.cells[i] + position;
            if (!Bounds.Contains((Vector2Int)tilePositionInt)) {
                return false;
            }
            Vector3Int gridNewPos = tilePositionInt + (Vector3Int)gridOffset;
            if (grid[gridNewPos.x, gridNewPos.y] != null)
                return false;
                
            // tilePosition += new Vector3(.5f, .5f);

            // List<Collider2D> colls = new List<Collider2D>();
            // ContactFilter2D contactFilter2D = new ContactFilter2D();
            // contactFilter2D.SetLayerMask(layerMask);
            // contactFilter2D.useTriggers = true;
            // Collider2D coll = Physics2D.OverlapBox(tilePosition, new Vector2(.95f, .95f), 0f, layerMask);
            // if (coll != null) {
            //     return false;
            // }
        }
        return true;
    }

    private bool IsLineFull(int row) {

        for (int col = 0; col < grid.GetLength(0); col++) {
            if (grid[col, row] == null) {
                return false;
            }
        }
        return true;
    }

    public void Freeze(int rowsToFreeze) {
        frozenRows += rowsToFreeze;


        for (int i = Bounds.yMin - frozenRows; i < Bounds.yMin; i++) {
            for (int col = Bounds.xMin; col < Bounds.xMax; col++) {
                Vector3Int position = new Vector3Int(col, i);

                tilemap.SetTile(position, tetrominoes.tetrominoesData[tetrominoes.tetrominoesData.Length - 1].tile);
            }
        }

    }

    private void OnDrawGizmosSelected() {
        // for (int i = 0; i < 4; i++) {
        //     Gizmos.DrawCube(positionToTest[i], Vector3.one);
        // }
        for (int row = 0; row < grid.GetLength(0); row++) {
            for (int col = 0; col < grid.GetLength(1); col++) {
                if (grid[row, col] != null) {
                    Gizmos.color = Color.green;
                } else {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawCube(new Vector3(row - 4.5f, col - 9.5f), new Vector2(.9f, .9f));

            }
        }

    }

    // private void OnDrawGizmos() {

    //     for (int row = Bounds.yMin + 1; row <= Bounds.yMax; row++) {
    //         for (int col = Bounds.xMin; col < Bounds.xMax; col++) {
    //             Vector3 position = new Vector3Int(col, row) + new Vector3(.5f, -.5f);

    //             // Collider2D coll = Physics2D.OverlapBox(position, new Vector2(.95f,.95f), 0f);
    //             // if (coll != null) {
    //             //     Gizmos.color=Color.green;
    //             //     Gizmos.DrawCube(position,new Vector2(.99f,.99f));

    //             // }
    //             // // else {
    //             // //                     Gizmos.color=Color.red;

    //             // //     Gizmos.DrawCube(position,new Vector2(.9f,.9f));

    //             // // }

    //         }

    //     }
    // }
}
