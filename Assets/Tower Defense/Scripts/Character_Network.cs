using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Character_Network : NetworkBehaviour 
{
    public string charName;
    public float maxHealth;
	public float health;
	public float strength;
	public float moveSpeed;
	public float attackSpeed;
	public Vector2 attackRange;
	public int manaCost;
	public GamePiece_Network.Type type;
	public BoxCollider2D bodyCollider;
	public BoxCollider2D rangeCollider;
	public Slider healthBar;
    public AudioSource attackSound;

    [HideInInspector] public bool dead;
	[HideInInspector] public List<GameObject> targets;
	[HideInInspector] public GameObject target;
	[HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer spriteRend;
    [HideInInspector] public Color spriteDefaultColor;

    private bool facingRight;
	private bool moving;
	private bool attackCoroutineIsActive;
	private Rigidbody2D rb2d;

	void Awake ()
	{
		targets = new List<GameObject> ();
		rb2d = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
        spriteRend = GetComponent<SpriteRenderer> ();
    }

	void Start () 
	{
		StartCoroutine(SetupCharacter());
	}

	IEnumerator SetupCharacter ()
	{
		health = maxHealth;
		rangeCollider.size = new Vector2 (attackRange.x * 3, attackRange.y * 3);
		facingRight = true;
		moving = true;
		while (this.tag == "Untagged") // Waits for PlayerNetwork to set tag
			yield return null;
		if (this.tag == "Player 2")
			Flip ();
        spriteDefaultColor = spriteRend.color;
    }

	void FixedUpdate ()
	{
		if (isServer == false)
		{
			return;
		}

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

			Character_Network enemyChar = target.GetComponent<Character_Network>();
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
			rb2d.velocity = new Vector2(moveSpeed, rb2d.velocity.y);
		}
		else
		{
			rb2d.velocity = new Vector2(-moveSpeed, rb2d.velocity.y);
		}

		animator.SetFloat("Speed", Mathf.Abs (rb2d.velocity.x));
	}

	void Stop ()
	{
		moving = false;

		rb2d.velocity = new Vector2 (0, 0);

		animator.SetFloat("Speed", Mathf.Abs (rb2d.velocity.x));
	}

	IEnumerator Attack (Character_Network enemyChar)
	{
		attackCoroutineIsActive = true;

		if (enemyChar != null)
		{
			while (this.dead == false && enemyChar.dead == false) 
			{
				animator.SetBool ("Attacking", true);
				animator.SetFloat ("Attack_Speed", attackSpeed);
				
				yield return new WaitForSeconds (1f / attackSpeed / 2);

                RpcPlayAttackSound();
                Damage(enemyChar);

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

                RpcPlayAttackSound();
                Damage(enemyBase);

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

    [ClientRpc]
    void RpcPlayAttackSound ()
    {
        attackSound.Play();
    }

    void Damage (Character_Network enemyChar)
	{
		if (enemyChar != null && enemyChar != dead)
		{
			enemyChar.health -= this.strength;
			UpdateHealthBar (enemyChar);

			if (enemyChar.health <= 0)
			{
				enemyChar.dead = true;
				StartCoroutine(Kill (enemyChar));
			}

            RpcDamageColorChange(enemyChar.gameObject);
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
				RpcKillBase (enemyBase.gameObject);
			}

            RpcDamageColorChange(enemyBase.gameObject);
        }
	}

    [ClientRpc]
    void RpcDamageColorChange (GameObject target)
    {
        StartCoroutine(DamageColorChange(target));
    }

    IEnumerator DamageColorChange (GameObject target)
    {
        if (target == null)
            Debug.LogError("Target not found!");

        Character_Network c = target.GetComponent<Character_Network>();
        Base b = target.GetComponent<Base>();

        if (c != null)
        {
            c.spriteRend.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            c.spriteRend.color = c.spriteDefaultColor;
        }
        else if (b != null)
        {
            b.spriteRend.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            b.spriteRend.color = b.spriteDefaultColor;
        }
        else
        {
            Debug.LogError("Target not found!");
        }
    }

    IEnumerator Kill (Character_Network enemyChar)
	{
		if (enemyChar != null)
		{
			enemyChar.animator.SetBool ("Dead", true);

			enemyChar.rangeCollider.enabled = false;
			enemyChar.bodyCollider.enabled = false;
			enemyChar.healthBar.gameObject.SetActive (false);

		}
		yield return new WaitForSeconds(2f);
        if (enemyChar != null)
        {
            Destroy (enemyChar.gameObject);
		    NetworkServer.Destroy(enemyChar.gameObject);
        }
	}

	[ClientRpc]
	void RpcKillBase (GameObject enemyBase)
	{
		if (enemyBase == null)
		{
			Debug.LogError("Base not found!");
			return;
		}

		Base b = enemyBase.GetComponent<Base>();

		if (b != null)
		{
			b.gameObject.SetActive(false);
			b.destroyed = true;
			GameController_Network.instance.GameOver();
		}
		else
			Debug.LogError("Base not found!");
	}

	void UpdateHealthBar (Character_Network enemyChar)
	{
		if (enemyChar != null)
		{ 
			float v = enemyChar.health / enemyChar.maxHealth;
			enemyChar.healthBar.value = v;
			RpcUpdateHealthBar(enemyChar.gameObject, v);
		}
	}

	void UpdateHealthBar (Base enemyBase)
	{
		if (enemyBase != null)
		{ 
			float v = enemyBase.health / enemyBase.maxHealth;
			enemyBase.healthBar.value = v;
			RpcUpdateHealthBar(enemyBase.gameObject, v);
		}
	}

	[ClientRpc]
	void RpcUpdateHealthBar (GameObject target, float healthBarValue)
	{
		if (target == null)
		{
			Debug.LogError("Target not found!");
			return;
		}

		Character_Network c = target.GetComponent<Character_Network>();
		Base b = target.GetComponent<Base>();

		if (c != null)
		{
			c.healthBar.value = healthBarValue;
		}
		else if (b != null)
		{
			b.healthBar.value = healthBarValue;
		}
		else
		{
			Debug.LogError("Target not found!");
		}
	}
}