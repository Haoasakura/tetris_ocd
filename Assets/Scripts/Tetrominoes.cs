using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TetrominoesData")]
public class Tetrominoes : ScriptableObject
{
    public TetrominoData[] tetrominoesData;

    public void Initialize() {
        for (int i = 0; i < tetrominoesData.Length; i++) {
            tetrominoesData[i].Initialize();

        }
    }
}
