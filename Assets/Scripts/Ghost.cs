using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour {
    [SerializeField] Tile tile;
    [SerializeField] Board mainBoard;
    [SerializeField] Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position;

    private void Awake() {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    private void LateUpdate() {
        Clear();
        Copy();
        Drop();
        SetPiece();
    }

    private void Clear() {
        for (int i = 0; i < cells.Length; i++) {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    private void Copy() {
        for (int i = 0; i < cells.Length; i++) {
            cells[i] = trackingPiece.cells[i];
        }
    }

    private void Drop() {
        Vector3Int _position = trackingPiece.position;

        int current = _position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.ClearPiece(trackingPiece);

        for (int row = current; row >= bottom; row--) {
            _position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, _position)) {
                position = _position;
            } else {
                break;
            }
        }

        mainBoard.SetPiece(trackingPiece);
    }

    private void SetPiece() {
        for (int i = 0; i < cells.Length; i++) {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
        }
    }
}
