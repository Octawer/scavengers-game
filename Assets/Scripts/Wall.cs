using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Sprite dmgSprite;
    public int hp = 4;

    public AudioClip chopWallSound1;
    public AudioClip chopWallSound2;

    private SpriteRenderer spriteRenderer;

    // Use this for initialization
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int damage)
    {
        spriteRenderer.sprite = dmgSprite;
        SoundManager.instance.RandomizeSfx(chopWallSound1, chopWallSound2);
        hp -= damage;
        if (hp <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
