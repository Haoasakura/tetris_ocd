using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubPiece : MonoBehaviour
{
    [SerializeField] Sprite placedSprite;
    [SerializeField] Color movingColor;

    private SpriteRenderer spriteRenderer;
    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = movingColor;

        placedSprite = spriteRenderer.sprite;
    }



    public void SetPlacedSprite() {
        spriteRenderer.sprite = placedSprite;
        spriteRenderer.color = Color.white;
    }
}
