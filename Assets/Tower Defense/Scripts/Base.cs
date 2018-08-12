using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour 
{
	public float maxHealth;
	public Slider healthBar;
	public BoxCollider2D bodyCollider;

	[HideInInspector] public float health;
	[HideInInspector] public bool destroyed;
    [HideInInspector] public SpriteRenderer spriteRend;
    [HideInInspector] public Color spriteDefaultColor;

    void Awake ()
    {
        spriteRend = GetComponent<SpriteRenderer>();
    }

    void Start ()
	{
		health = maxHealth;
		healthBar.value = health / 100f;
		destroyed = false;
        spriteDefaultColor = spriteRend.color;
	}
}