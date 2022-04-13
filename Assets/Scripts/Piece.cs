using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int position;
    public Vector3Int[] cells { get; private set; }

    private int rotationIdx;

    public void Initialize(Board _board, Vector3Int _position, TetrominoData _data) {
        board = _board;
        position = _position;
        data = _data;
        rotationIdx = 0;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update() {
        board.ClearPiece(this);

        if (Input.GetKeyDown(KeyCode.Q)) {
            Rotate(-1);
        } else if (Input.GetKeyDown(KeyCode.E)) {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            Move(Vector2Int.left);
        } else if (Input.GetKeyDown(KeyCode.D)) {
            Move(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            Move(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();
        }

        board.SetPiece(this);
    }

    private bool Move(Vector2Int translation) {

        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid = board.IsValidPosition(this, newPosition);

        if (isValid) {
            position = newPosition;
        }

        return isValid;
    }

    private void Rotate(int direction) {
        rotationIdx = Wrap(rotationIdx + direction, 0, 4);

        for (int i = 0; i < cells.Length; i++) {
            Vector3 cell = cells[i];
            int x, y;
            switch (data.tetromino) {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= .5f;
                    cell.y -= .5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }
            cells[i] = new Vector3Int(x,y);
        }
    }

    private void HardDrop() {
        while (Move(Vector2Int.down)) {
            continue;
        }
    }

    private int Wrap(int input, int min, int max) {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}
