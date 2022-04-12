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
    [SerializeField] private Vector2Int boardSize = new Vector2Int(10,20);

    public RectInt Bounds {
        get {
            Vector2Int position = new Vector2Int(-boardSize.x/2, -boardSize.y/2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake() {

        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponent<Piece>();


        tetrominoes.Initialize();
    }
    
    private void Start() {
        SpawnPiece();
    }

    public void SpawnPiece () {
        int random= Random.Range(0,tetrominoes.tetrominoesData.Length);

        TetrominoData data = tetrominoes.tetrominoesData[random];

        activePiece.Initialize(this, spawnPosition , data);
        SetPiece(activePiece);
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

    public bool IsValidPosition (Piece piece, Vector3Int position) {
        RectInt bounds = Bounds;

        for (int i = 0; i <piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + position;

            if(!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            if(tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }
}
