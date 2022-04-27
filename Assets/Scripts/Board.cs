using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    private Tilemap tilemap;
    private Piece activePiece;

    [SerializeField] private Tetrominoes tetrominoes;

    [SerializeField] private Vector3Int spawnPosition;

    [SerializeField] private Vector2Int size = new Vector2Int(10, 20);
    public Vector2Int boardSize = new Vector2Int(10, 20);

    private int nextPiece;

    [SerializeField] private string nextTetromino; //temp
    [SerializeField] private int frozenRows = 0;

    public LayerMask layerMask;

    public Transform[,] grid = new Transform[10, 20];
    [SerializeField] public Vector2Int gridOffset;

    public RectInt Bounds {
        get {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            RectInt r = new RectInt(position, boardSize);
            r.yMin += frozenRows;
            return r;
        }
    }


    private void Awake() {

        boardSize = size;
        grid = new Transform[size.x, size.y];

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

    public void SpawnPiece() {
        int random = nextPiece;
        nextPiece = Random.Range(0, tetrominoes.tetrominoesData.Length - 1);

        TetrominoData data = tetrominoes.tetrominoesData[random];
        TetrominoData nextData = tetrominoes.tetrominoesData[nextPiece];
        nextTetromino = nextData.tetromino.ToString();
        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition, Vector2Int.zero)) {
            SetPiece(activePiece);
        }
        else {
            GameOver();
        }
    }

    public void SetPiece(Piece piece) {
        activePiece.occupiedCells.Clear();
        piece.pieceRef.transform.position = piece.position;
        for (int i = 0; i < piece.pieceRef.transform.childCount; i++) {
            Vector3Int tileNewPosition = Vector3Int.RoundToInt(piece.pieceRef.transform.GetChild(i).position - piece.gridAlignOffset);
            Vector2Int gridNewPos = (Vector2Int)tileNewPosition + gridOffset;

            SetGridPiece(gridNewPos, piece.pieceRef.transform.GetChild(i), (piece.pieceRef.transform.GetChild(i).position), piece.pieceRef.transform.GetChild(i).GetComponent<BoxCollider2D>().size);
        }
    }

    public void SetGridPiece(Vector2Int centerPosition, Transform subPiece, Vector3 position, Vector2 size) {
        int minX = Mathf.Max(0, centerPosition.x - 1);
        int maxX = Mathf.Min(grid.GetLength(0), centerPosition.x + 2);
        int minY = Mathf.Max(0, centerPosition.y - 1);
        int maxY = Mathf.Min(grid.GetLength(1), centerPosition.y + 2);
        for (int row = minX; row < maxX; row++) {
            for (int col = minY; col < maxY; col++) {
                Vector2 pos = new Vector2(row, col) - gridOffset + new Vector2(.5f, .5f);

                Bounds b1 = new Bounds(pos, new Vector2(.95f, .95f));
                Bounds b2 = new Bounds(position, size);

                if (b1.Intersects(b2)) {
                    grid[row, col] = subPiece;
                    if (!activePiece.occupiedCells.Contains(new Vector2Int(row, col)))
                        activePiece.occupiedCells.Add(new Vector2Int(row, col));
                }
            }
        }
    }

    public void ClearPiece(Piece piece) {
        for (int i = 0; i < piece.pieceRef.transform.childCount; i++) {
            Vector3Int tilePosition = Vector3Int.RoundToInt(piece.pieceRef.transform.GetChild(i).position - piece.gridAlignOffset);
            Vector2Int gridNewPos = (Vector2Int)tilePosition + gridOffset;

            ClearGridPiece(gridNewPos, piece.pieceRef.transform.GetChild(i));
        }
    }
    public void ClearGridPiece(Vector2Int centerPosition, Transform subPiece) {
        int minX = Mathf.Max(0, centerPosition.x - 1);
        int maxX = Mathf.Min(grid.GetLength(0), centerPosition.x + 2);
        int minY = Mathf.Max(0, centerPosition.y - 1);
        int maxY = Mathf.Min(grid.GetLength(1), centerPosition.y + 2);
        for (int row = minX; row < maxX; row++) {
            for (int col = minY; col < maxY; col++) {
                if (grid[row, col] != null && grid[row, col].gameObject.name == subPiece.gameObject.name)
                    grid[row, col] = null;
            }
        }
    }

    public void ClearLines() {

        int row = 0;

        while (row < grid.GetLength(1)) {
            if (IsLineFull(row)) {
                LineClear(row);
            }
            else {
                row++;
            }
        }
    }

    private void LineClear(int row) {

        for (int col = 0; col < grid.GetLength(0); col++) {
            if (grid[col, row] != null) {
                DestroyImmediate(grid[col, row].gameObject);
                grid[col, row] = null;
            }
        }

        List<Transform> alreadyMoved = new List<Transform>();
        while (row < (grid.GetLength(1) - 1)) {
            for (int col = 0; col < grid.GetLength(0); col++) {
                if (grid[col, (row + 1)] != null /*&& !alreadyMoved.Contains(grid[col,row])*/) {
                    grid[col, row] = grid[col, (row + 1)];
                    grid[col, (row + 1)] = null;
                    grid[col, row].localPosition += Vector3Int.down;
                    alreadyMoved.Add(grid[col, row]);
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

    public bool IsValidPosition(Piece piece, Vector3Int position, Vector2Int translation) {

        for (int i = 0; i < piece.pieceRef.transform.childCount; i++) {

            Vector3Int tilePosition = Vector3Int.RoundToInt(piece.pieceRef.transform.GetChild(i).localPosition + position - piece.gridAlignOffset);

            if (!Bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }
        }

        foreach (Vector2Int pos in piece.occupiedCells) {

            Vector3Int tilePosition = new Vector3Int(pos.x + translation.x - gridOffset.x, pos.y + translation.y - gridOffset.y);

            if (!Bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            Vector2Int gridNewPos = pos + translation;

            if (grid[gridNewPos.x, gridNewPos.y] != null && grid[gridNewPos.x, gridNewPos.y].gameObject.layer != LayerMask.NameToLayer("Moving"))
                return false;

        }
        return true;
    }

    private bool IsLineFull(int row) {

        for (int col = 0; col < grid.GetLength(0); col++) {
            if (grid[col, row] == null || (int)grid[col, row].localEulerAngles.z % 90 != 0) {
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

        for (int row = 0; row < grid.GetLength(0); row++) {
            for (int col = 0; col < grid.GetLength(1); col++) {
                if (grid[row, col] != null) {
                    Gizmos.color = Color.green;
                }
                else {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawCube(new Vector3(row - gridOffset.x + .5f, col - gridOffset.y + .5f), new Vector2(.9f, .9f));
            }
        }
        // Gizmos.color = Color.blue;

        // foreach (Vector3 v in checks) {
        //     Gizmos.DrawCube(new Vector3(v.x - gridOffset.x + .5f, v.y - gridOffset.y + .5f), new Vector2(.9f, .9f));

        // }
        // for (int row = minX; row < maxX; row++) {
        //     for (int col = minY; col < maxY; col++) {
        //         Gizmos.color = Color.green;

        //         Gizmos.DrawCube(new Vector3(row - 4.5f, col - 9.5f), new Vector2(.9f, .9f));

        //     }
        // }
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
