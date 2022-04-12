using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board {get; private set;}
    public TetrominoData data {get; private set;}
    public Vector3Int position;

    public Vector3Int[] cells {get; private set; }

    public void Initialize(Board _board, Vector3Int _position, TetrominoData _data) {
        board = _board;
        position = _position;
        data = _data;

        if(cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for(int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update() {
        board.ClearPiece(this);

        if(Input.GetKeyDown(KeyCode.A)) {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            Move(Vector2Int.right);
        }
        
        if(Input.GetKeyDown(KeyCode.S)) {
            Move(Vector2Int.down);
        }

        if(Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();
        }

        board.SetPiece(this);
    }

    private bool Move(Vector2Int translation) {

        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid =board.IsValidPosition(this, newPosition);

        if(isValid) {
            position=newPosition;
        }

        return isValid;
    }

    private void HardDrop() {
        while(Move(Vector2Int.down)) {
            continue;
        }
    }
}
