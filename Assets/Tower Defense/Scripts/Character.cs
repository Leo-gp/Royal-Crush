using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour 
{
	public float maxHealth;
	public float health;
	public float strength;
	public float moveSpeed;
	public float attackSpeed;
	public Vector2 attackRange;
	public int manaCost;
	public GamePiece.Type type;
	public BoxCollider2D bodyCollider;
	public BoxCollider2D rangeCollider;

	public Slider healthBar;

	private bool facingRight;
	private bool moving;
	private bool attackCoroutineIsActive;
	[HideInInspector] public bool dead;
	[HideInInspector] public List<GameObject> targets;
	[HideInInspector] public GameObject target;

	private Rigidbody2D rb2d;
	[HideInInspector] public Animator animator;

	public static Character instance;

	void Awake ()
	{
		instance = this;
		targets = new List<GameObject> ();
		rb2d = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
	}

	void Start () 
	{
		health = maxHealth;

		rangeCollider.size = new Vector2 (attackRange.x * 3, attackRange.y * 3);

		facingRight = true;

		if (this.tag == "Player 2")
		{
			Flip ();
		}

		Move ();
	}

	void Update ()
	{
		if (moving)
			Move ();
		else
			Stop ();

		if (dead)
		{
			Stop ();

			target = null;
			targets.Clear ();
		}

		if (target != null && !attackCoroutineIsActive) 
		{
			if (moving)
				Stop ();

			Character enemyChar = target.GetComponent<Character>();
			Base enemyBase = target.GetComponent<Base>();

			if (enemyChar != null)
				StartCoroutine (Attack (enemyChar));
			else if (enemyBase != null)
				StartCoroutine (Attack (enemyBase));
		}
		else if (targets.Count > 0)
			target = targets [0];
	}

	void Flip ()
	{
		facingRight = !facingRight;
		Vector3 myScale = transform.localScale;
		myScale.x *= -1;
		transform.localScale = myScale;
	}

	public void Move ()
	{
		moving = true;

		if (facingRight)
		{
			rb2d.velocity = new Vector2(moveSpeed, rb2d.velocity.y) * Time.deltaTime;
		}
		else
		{
			rb2d.velocity = new Vector2(-moveSpeed, rb2d.velocity.y) * Time.deltaTime;
		}

		animator.SetFloat("Speed", Mathf.Abs (rb2d.velocity.x));
	}

	void Stop ()
	{
		moving = false;

		rb2d.velocity = new Vector2 (0, 0);

		animator.SetFloat("Speed", Mathf.Abs (rb2d.velocity.x));
	}

	IEnumerator Attack (Character enemyChar)
	{
		attackCoroutineIsActive = true;

		if (enemyChar != null)
		{
			while (this.dead == false && enemyChar.dead == false) 
			{
				animator.SetBool ("Attacking", true);
				animator.SetFloat ("Attack_Speed", attackSpeed);
				
				yield return new WaitForSeconds (1f / attackSpeed / 2);
				
				Damage (enemyChar);
				
				yield return new WaitForSeconds (1f / attackSpeed / 2);
			}
		}

		if(this.dead == false)
		{
			targets.Remove(target);
			target = null;
			attackCoroutineIsActive = false;

			animator.SetBool ("Attacking", false);
			Move ();
		}
	}

	IEnumerator Attack (Base enemyBase)
	{
		attackCoroutineIsActive = true;

		if (enemyBase != null)
		{
			while (this.dead == false && enemyBase != null) 
			{
				animator.SetBool ("Attacking", true);
				animator.SetFloat ("Attack_Speed", attackSpeed);

				yield return new WaitForSeconds (1f / attackSpeed / 2);

				Damage (enemyBase);

				yield return new WaitForSeconds (1f / attackSpeed / 2);
			}
		}

		if(this.dead == false)
		{
			targets.Remove(target);
			target = null;
			attackCoroutineIsActive = false;

			animator.SetBool ("Attacking", false);
			Move ();
		}
	}

	void Damage (Character enemyChar)
	{
		if (enemyChar != null && enemyChar != dead)
		{
			enemyChar.health -= this.strength;
			UpdateHealthBar (enemyChar);

			if (enemyChar.health <= 0)
			{
				enemyChar.dead = true;
				Kill (enemyChar);
			}
		}
	}

	void Damage (Base enemyBase)
	{
		if (enemyBase != null)
		{
			enemyBase.health -= this.strength;
			UpdateHealthBar (enemyBase);

			if (enemyBase.health <= 0)
			{
				Kill (enemyBase);
			}
		}
	}

	void Kill (Character enemyChar)
	{
		if (enemyChar != null)
		{
			enemyChar.animator.SetBool ("Dead", true);

			enemyChar.rangeCollider.enabled = false;
			enemyChar.bodyCollider.enabled = false;
			enemyChar.healthBar.gameObject.SetActive (false);

			Destroy (enemyChar.gameObject, 2);
		}
	}

	void Kill (Base enemyBase)
	{
		if (enemyBase != null)
		{
			enemyBase.bodyCollider.enabled = false;
			enemyBase.healthBar.gameObject.SetActive (false);
			enemyBase.destroyed = true;

			GameController.instance.GameOver();

			Destroy (enemyBase.gameObject);
		}
	}

	void UpdateHealthBar (Character enemyChar)
	{
		if (enemyChar != null)
		{ 
			enemyChar.healthBar.value = enemyChar.health / enemyChar.maxHealth;
		}
	}

	void UpdateHealthBar (Base enemyBase)
	{
		if (enemyBase != null)
		{ 
			enemyBase.healthBar.value = enemyBase.health / enemyBase.maxHealth;
		}
	}
}