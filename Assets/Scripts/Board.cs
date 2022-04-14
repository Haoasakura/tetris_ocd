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

        if (IsValidPosition(activePiece, spawnPosition)) {
            SetPiece(activePiece);
        } else {
            GameOver();
        }

    }

    public void SetPiece(Piece piece) {
        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void ClearPiece(Piece piece) {
        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public void ClearLines() {

        int row = Bounds.yMin;

        while (row < Bounds.yMax) {
            if (IsLineFull(row)) {
                LineClear(row);
            } else {
                row++;
            }
        }
    }

    private void LineClear(int row) {

        for (int col = Bounds.xMin; col < Bounds.xMax; col++) {

            Vector3Int position = new Vector3Int(col, row);
            tilemap.SetTile(position, null);
        }

        while (row < Bounds.yMax) {
            for (int col = Bounds.xMin; col < Bounds.xMax; col++) {

                Vector3Int position = new Vector3Int(col, row + 1);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row);
                tilemap.SetTile(position, above);
            }
            row++;
        }
    }

    private void GameOver() {
        tilemap.ClearAllTiles();
        frozenRows = 0;
        activePiece.stepDelay = activePiece.initialStepDelay;
    }

    public bool IsValidPosition(Piece piece, Vector3Int position) {

        for (int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!Bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    private bool IsLineFull(int row) {

        for (int col = Bounds.xMin; col < Bounds.xMax; col++) {
            Vector3Int position = new Vector3Int(col, row);

            if (!tilemap.HasTile(position)) {
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


}
