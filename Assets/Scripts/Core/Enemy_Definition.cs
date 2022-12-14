using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Definition : MonoBehaviour, IEffectable
{
	public GameObject deathEffect;
	public StatusEffectData _data;
	private Transform target;

	public float currentHealth;
	public float maxHealth = 50;

	[Header("Enemy Type Bounty")]

	public int diceRollRange = 5;
	public int normalBounty = 10;
	public int specialBounty = 25;
	public int bossBounty = 50;
	public int massBounty = 5;
	public int farmBounty = 100;
	public string enemyType;
	private int totalBounty;

	private bool isDead = false;
	private int wavepoint = 0;
    
	private float moveSpeed = 20f;
	private float currentEffectTime = 0f;
	private float nextTickTime = 0f;

	[SerializeField]
	private Queue<string> enemyCaptured;

	void Start()
	{
		target = Waypoint_Logic.points[0];
		currentHealth = maxHealth;
		enemyCaptured = new Queue<string>(10);
	}

	void Update()
	{
		if (_data != null) HandleEffect();

		Vector3 direction = target.position - transform.position;
		transform.Translate(moveSpeed * Time.deltaTime * direction.normalized, Space.World);

		if (Vector3.Distance(transform.position, target.position) <= 0.4f)
		{
			if (wavepoint >= Waypoint_Logic.points.Length - 1)
			{
				Destroy(gameObject);
                Player_Currency.lives--;
				return;
			}
			wavepoint++;
			target = Waypoint_Logic.points[wavepoint];
		}
	}

	// Debuffs start
	public void Debuff_End()
	{
		_data = null;
		currentEffectTime = 0;
		nextTickTime = 0;
		moveSpeed = 20f;
	}

	public void Debuff_Apply(StatusEffectData _data)
	{
		Debuff_End();
		this._data = _data;
		if (_data.MovementPenalty > 0)
		{
			moveSpeed /= _data.MovementPenalty;
		}
	}

	public void HandleEffect()
	{
		if (currentHealth <= 0 && !isDead)
		{
			Die();
		}
		currentEffectTime += Time.deltaTime;

		if (currentEffectTime >= _data.Lifetime)
		{
			Debuff_End();
		}

		if (_data == null)
		{
			return;
		}

		if (_data.DOTAmount != 0 && currentEffectTime > nextTickTime)
		{
			nextTickTime += _data.TickSpeed;
            currentHealth -= _data.DOTAmount;
			currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
		}
	}
	// Debuffs end

	public void TakeDamage(int amount)
	{
        currentHealth -= amount;
		Debug.Log("Damage: " + amount + " // Current Health: " + currentHealth);
		if (currentHealth <= 0 && !isDead)
		{
			Die();
		}
	}

	void Die()
	{
		isDead = true;

		GetBounty();
		GetMana(target);

		GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
		Destroy(effect, 5f);
		Destroy(gameObject);
	}

	void GetMana(Transform target)
	{
		Player_Currency.mana += enemyCaptured.Count / 4;
		Debug.Log("Mana after death: " + Player_Currency.mana);

		if (Random.Range(0, 100) < 20)
		{
			enemyCaptured.Enqueue(target.name);			
			if (enemyCaptured.Count > 10)
			{
				enemyCaptured.Dequeue();
				Debug.Log("Dequeueing");
			}
		}
	}


	void GetBounty()
    {
		// "Normal", "Mass", "Boss", "Farm", "Special"
		switch (enemyType)
        {
            case "Normal":
				totalBounty = Random.Range(normalBounty - diceRollRange, normalBounty + diceRollRange);
				break;

			case "Mass":
				totalBounty = Random.Range(massBounty - diceRollRange, massBounty + diceRollRange);
				break;

			case "Boss":
				totalBounty = Random.Range(bossBounty - diceRollRange, bossBounty + diceRollRange);
				break;

			case "Farm":
				totalBounty = Random.Range(farmBounty - diceRollRange, farmBounty + diceRollRange);
				break;

			case "Special":
				totalBounty = Random.Range(specialBounty - diceRollRange, specialBounty + diceRollRange);
				break;
        }
		if (totalBounty < 0) totalBounty += diceRollRange;
		Player_Currency.money += totalBounty;
		Debug.Log("Gold after death: " + Player_Currency.money + " // Gold received: " + totalBounty);
		
	}

}
