using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int position;
    public Vector3Int[] cells { get; private set; }

    private int rotationIdx;

    [SerializeField] private float stepDelay = 1f;
    [SerializeField] private float moveDelay = .1f;
    [SerializeField] private float lockDelay = .5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    [SerializeField] private float incrementStep = .1f;
    [SerializeField] private float incrementInterval = 5f;
    private float incrementTime = 5f;

    public void Initialize(Board _board, Vector3Int _position, TetrominoData _data) {
        board = _board;
        position = _position;
        data = _data;
        rotationIdx = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0;
        incrementTime=0;

        if (cells == null) {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++) {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update() {
        board.ClearPiece(this);

        lockTime += Time.deltaTime;
        incrementTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q)) {
            Rotate(-1);
        } else if (Input.GetKeyDown(KeyCode.E)) {
            Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();
        }

        if (Time.time > moveTime) {
            HandleMoveInputs();
        }


        if (Time.time >= stepTime) {
            Step();
        }

        if(incrementTime >= incrementInterval) {
            stepDelay = Mathf.Max(0.1f, stepDelay-incrementStep);
            incrementTime=0;
        }

        board.SetPiece(this);
    }

    private void HandleMoveInputs() {

        if (Input.GetKey(KeyCode.S)) {
            if (Move(Vector2Int.down)) {
                stepTime = Time.time + stepDelay;
            }
        }

        if (Input.GetKey(KeyCode.A)) {
            Move(Vector2Int.left);
        } else if (Input.GetKey(KeyCode.D)) {
            Move(Vector2Int.right);
        }
    }

    private bool Move(Vector2Int translation) {

        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid = board.IsValidPosition(this, newPosition);

        if (isValid) {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0;
        }

        return isValid;
    }

    private void Rotate(int direction) {
        int originalRotation = rotationIdx;
        rotationIdx = Wrap(rotationIdx + direction, 0, 4);

        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIdx, direction)) {
            rotationIdx = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void Step() {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection) {
        int wallKickIdx = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++) {
            Vector2Int translation = data.wallKicks[wallKickIdx, i];

            if (Move(translation))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection) {
        int wallKickIdx = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIdx--;
        }

        return Wrap(wallKickIdx, 0, data.wallKicks.GetLength(0));
    }

    private void ApplyRotationMatrix(int direction) {
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
            cells[i] = new Vector3Int(x, y);
        }
    }

    private void Lock() {
        board.SetPiece(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private void HardDrop() {
        while (Move(Vector2Int.down)) {
            continue;
        }
        Lock();
    }



    private int Wrap(int input, int min, int max) {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }
}
