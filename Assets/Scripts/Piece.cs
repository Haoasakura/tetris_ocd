using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    [HideInInspector] public Vector3Int position;
    //public Vector3Int[] cells { get; private set; }

    private int rotationIdx;

    public float initialStepDelay = 1f;

    public float stepDelay = 1f;
    [SerializeField] private float moveDelay = .1f;
    [SerializeField] private float rotateDelay = .5f;
    [SerializeField] private float lockDelay = .5f;

    private float stepTime;
    private float moveTime;
    private float rotateTime;
    private float lockTime;

    [SerializeField] private float incrementStep = .1f;
    [SerializeField] private float incrementInterval = 5f;
    private float incrementTime = 0f;

    public Transform tilesParent;
    [HideInInspector] public GameObject pieceRef;
    private int namecount = 0;

    public Vector3 gridAlignOffset = new Vector3(.5f, .5f, 0f);

    [SerializeField, Range(0, 360)] private int rotateDegree = 90;

    [HideInInspector] public List<Vector2Int> occupiedCells = new List<Vector2Int>();

    [Header("UI")]

    [SerializeField] private TMP_Text rotationText;

    //public List<Vector3> pivots = new List<Vector3>();

    public void Initialize(Board _board, Vector3Int _position, TetrominoData _data) {

        board = _board;
        position = _position;
        data = _data;
        rotationIdx = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        rotateTime = Time.time + rotateDelay;
        lockTime = 0;
        occupiedCells.Clear();

        pieceRef = Instantiate(data.tetrominoGO, position, Quaternion.identity, tilesParent);
        pieceRef.transform.position = position;
        pieceRef.name += namecount;
        for (int i = 0; i < pieceRef.transform.childCount; i++) {
            pieceRef.transform.GetChild(i).name = namecount + " - " + pieceRef.transform.GetChild(i).name;
            Vector3Int tilePosition = Vector3Int.RoundToInt(pieceRef.transform.GetChild(i).localPosition + position - gridAlignOffset);
            Vector2Int gridPos = (Vector2Int)tilePosition + board.gridOffset;
            occupiedCells.Add(gridPos);
        }
        namecount++;
        int oldRot = rotateDegree;
        rotateDegree = Random.Range(0, 180)*2;
        Rotate(Random.Range(-1f, +1f) > 0 ? +1 : -1);
        rotateDegree = oldRot;
    }

    private void Update() {

        board.ClearPiece();

        lockTime += Time.deltaTime;
        incrementTime += Time.deltaTime;

        if (Input.GetKey(KeyCode.Q) && Time.time > rotateTime) {
            Rotate(-1);
        }
        else if (Input.GetKey(KeyCode.E) && Time.time > rotateTime) {
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

        if (incrementTime >= incrementInterval) {
            stepDelay = Mathf.Max(0.1f, stepDelay - incrementStep);
            rotateDelay = Mathf.Max(0.03f, rotateDelay - (incrementStep / 5f));
            incrementTime = 0;
        }

        board.SetPiece();

        UpdateUI();
    }


    private void HandleMoveInputs() {

        if (Input.GetKey(KeyCode.S)) {
            if (Move(Vector2Int.down)) {
                stepTime = Time.time + stepDelay;
            }
        }

        if (Input.GetKey(KeyCode.A)) {
            Move(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.D)) {
            Move(Vector2Int.right);
        }
    }

    private bool Move(Vector2Int translation) {

        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool isValid = board.IsValidPosition(newPosition, translation);
        if (isValid) {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0;

            for (int i = 0; i < occupiedCells.Count; i++) {
                occupiedCells[i] += translation;
            }

        }

        return isValid;
    }

    private void Rotate(int direction) {
        int originalRotation = rotationIdx;
        rotationIdx = Wrap(rotationIdx + direction, 0, 4);
        if (!CanRotate(direction))
            return;

        ApplyRotationMatrix(direction);

        //if (!TestWallKicks(rotationIdx, direction))        
        if (!board.IsValidPosition(position, Vector2Int.zero)) {
            rotationIdx = originalRotation;
            ApplyRotationMatrix(-direction);
        }
        rotateTime = Time.time + rotateDelay;
    }

    private void Step() {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    //private bool TestWallKicks(int rotationIndex, int rotationDirection) {
    //    int wallKickIdx = GetWallKickIndex(rotationIndex, rotationDirection);

    //    for (int i = 0; i < data.wallKicks.GetLength(1); i++) {
    //        Vector2Int translation = data.wallKicks[wallKickIdx, i];

    //        if (Move(translation))
    //            return true;
    //    }

    //    return false;
    //}

    private bool CanRotate(int direction) {
        GameObject go = Instantiate(pieceRef);
        Vector3 pivot = go.transform.position + gridAlignOffset;

        for (int i = 0; i < pieceRef.transform.childCount; i++) {
            Transform c = go.transform.GetChild(i);
            switch (data.tetromino) {
                case Tetromino.I:
                case Tetromino.O:
                c.RotateAround((pivot + gridAlignOffset), Vector3.forward, direction * -rotateDegree);
                break;
                default:
                c.RotateAround(pivot, Vector3.forward, direction * -rotateDegree);
                break;
            }
        }
        List<Vector2Int> cells = UpdateOccupiedCells(go);
        Vector3Int posInt = new Vector3Int((int)go.transform.position.x, (int)go.transform.position.y, (int)go.transform.position.z);
        if (!IsValidRotation(go, posInt, Vector2Int.zero, cells)) {

            DestroyImmediate(go);
            return false;
        }

        DestroyImmediate(go);
        return true;
    }

    //private int GetWallKickIndex(int rotationIndex, int rotationDirection) {
    //    int wallKickIdx = rotationIndex * 2;

    //    if (rotationDirection < 0) {
    //        wallKickIdx--;
    //    }

    //    return Wrap(wallKickIdx, 0, data.wallKicks.GetLength(0));
    //}

    private void ApplyRotationMatrix(int direction) {
        Vector3 pivot = pieceRef.transform.position + gridAlignOffset*4;
        //pivots.Clear();
        for (int i = 0; i < pieceRef.transform.childCount; i++) {
            Transform c = pieceRef.transform.GetChild(i);
            switch (data.tetromino) {
                case Tetromino.I:
                case Tetromino.O:
                c.RotateAround(pivot + gridAlignOffset * 4, Vector3.forward, direction * -rotateDegree);
                //pivots.Add(pivot + gridAlignOffset * 4);
                break;
                default:
                c.RotateAround(pivot, Vector3.forward, direction * -rotateDegree);
                //pivots.Add(pivot);
                break;
            }
        }
    }

    public void HardDrop() {
        while (Move(Vector2Int.down)) {
            continue;
        }
        Lock();
    }

    private void Lock() {

        pieceRef.layer = LayerMask.NameToLayer("Placed");
        for (int i = 0; i < pieceRef.transform.childCount; i++) {
            pieceRef.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Placed");
            pieceRef.transform.GetChild(i).GetComponent<SubPiece>().SetPlacedSprite();
        }

        board.SetPiece();
        board.ClearLines();
        board.SpawnPiece();
    }

    public void Reset() {
        for (int i = tilesParent.childCount - 2; i >= 0; i--) {
            Destroy(tilesParent.GetChild(i).gameObject);
        }
    }

    public List<Vector2Int> UpdateOccupiedCells() {
        List<Vector2Int> oldOccupiedCells = new List<Vector2Int>();
        foreach (var c in occupiedCells) {
            oldOccupiedCells.Add(new Vector2Int(c.x, c.y));
        }
        occupiedCells.Clear();
        for (int i = 0; i < pieceRef.transform.childCount; i++) {
            Vector3Int tileNewPosition = Vector3Int.RoundToInt(pieceRef.transform.GetChild(i).position - gridAlignOffset);
            Vector2Int gridNewPos = (Vector2Int)tileNewPosition + board.gridOffset;

            board.SetGridPiece(gridNewPos, pieceRef.transform.GetChild(i), (pieceRef.transform.GetChild(i).position), pieceRef.transform.GetChild(i).GetComponent<BoxCollider2D>().size);
        }
        return oldOccupiedCells;
    }
    public List<Vector2Int> UpdateOccupiedCells(GameObject go) {
        List<Vector2Int> oldOccupiedCells = new List<Vector2Int>();

        for (int i = 0; i < go.transform.childCount; i++) {
            Vector3Int tileNewPosition = Vector3Int.RoundToInt(go.transform.GetChild(i).position - gridAlignOffset);
            Vector2Int gridNewPos = (Vector2Int)tileNewPosition + board.gridOffset;

            SetGridPiece(gridNewPos, (go.transform.GetChild(i).position), pieceRef.transform.GetChild(i).GetComponent<BoxCollider2D>().size, ref oldOccupiedCells);
        }
        return oldOccupiedCells;
    }
    public void SetGridPiece(Vector2Int centerPosition, Vector3 position, Vector2 size, ref List<Vector2Int> occCells) {
        int minX = centerPosition.x - 1;
        int maxX = centerPosition.x + 2;
        int minY = centerPosition.y - 1;
        int maxY = centerPosition.y + 2;
        for (int row = minX; row < maxX; row++) {
            for (int col = minY; col < maxY; col++) {
                Vector2 pos = new Vector2(row, col) - board.gridOffset + new Vector2(.5f, .5f);

                Bounds b1 = new Bounds(pos, new Vector2(.95f, .95f));
                Bounds b2 = new Bounds(position, size);

                if (b1.Intersects(b2)) {
                    if (!occCells.Contains(new Vector2Int(row, col)))
                        occCells.Add(new Vector2Int(row, col));
                }
            }
        }
    }


    private int Wrap(int input, int min, int max) {
        if (input < min) {
            return max - (min - input) % (max - min);
        }
        else {
            return min + (input - min) % (max - min);
        }
    }

    public bool IsValidRotation(GameObject piece, Vector3Int position, Vector2Int translation, List<Vector2Int> cells) {

        for (int i = 0; i < piece.transform.childCount; i++) {

            Vector3Int tilePosition = Vector3Int.RoundToInt(piece.transform.GetChild(i).localPosition + position - gridAlignOffset);

            if (!board.Bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

        }

        foreach (Vector2Int pos in cells) {

            Vector3Int tilePosition = new Vector3Int(pos.x + translation.x - board.gridOffset.x, pos.y + translation.y - board.gridOffset.y);

            if (!board.Bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            Vector2Int gridNewPos = pos + translation;

            if (board.grid[gridNewPos.x, gridNewPos.y] != null && board.grid[gridNewPos.x, gridNewPos.y].gameObject.layer != LayerMask.NameToLayer("Moving"))
                return false;

        }
        return true;
    }

    public void ButtonRotate(int direction) {
        if (Time.time > rotateTime) {
            Rotate((int)Mathf.Sign(direction));
        }
    }

    public void ButtonMoveInput (int value) {
        if (Time.time > moveTime) {

            if (value==0) {
                if (Move(Vector2Int.down)) {
                    stepTime = Time.time + stepDelay;
                }
            }

            if (value == -1) {
                Move(Vector2Int.left);
            }
            else if (value == 1) {
                Move(Vector2Int.right);
            }
        }
    }

    private void UpdateUI() {
        rotationText.text = Mathf.RoundToInt(pieceRef.transform.GetChild(0).eulerAngles.z) + "°";
    }

    //private void OnDrawGizmosSelected() {
    //    //Gizmos.color = Color.blue;
    //    //foreach (var c in occupiedCells) {
    //    //    Gizmos.DrawCube(new Vector3(c.x - board.gridOffset.x + .5f, c.y - board.gridOffset.y + .5f), new Vector2(.9f, .9f));
    //    //}

    //    Gizmos.color = Color.blue;
    //    foreach (var c in pivots) {
    //        Gizmos.DrawCube(new Vector3(c.x, c.y), new Vector2(.9f, .9f));
    //    }
    //}
}
