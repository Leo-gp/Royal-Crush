using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mana : MonoBehaviour 
{
	public GamePiece.Type manaType;

	[HideInInspector] public int manaPoints;

	private Slider manaSlider;
	private Text manaText;

	void Start ()
	{
		manaPoints = 0;
		manaSlider = GetComponentInParent<Slider> ();
		manaText = GetComponentInChildren<Text> ();
	}
	public void GainMana (float amount)
	{
		if (manaSlider.value + amount / 100f < 1f)
		{
			manaSlider.value += amount / 100f;
		}
		else
		{
			float points = manaSlider.value +  (amount / 100f);
			while (points >= 1f)
			{
				manaPoints++;
				manaText.text = manaPoints.ToString();
				points -= 1f;
			}
			manaSlider.value = points;
		}

		UpdateButtonInteractivity ();
	}

	public void SpendMana (int amount)
	{
		manaPoints -= amount;
		manaText.text = manaPoints.ToString();

		UpdateButtonInteractivity ();
	}

	public static Mana GetMana (GamePiece.Type manaType, Mana[] allManasReference)
	{
		foreach (Mana mana in allManasReference) 
		{
			if (mana.manaType == manaType)
			{
				return mana;
			}
		}

		return null;
	}

	public void UpdateButtonInteractivity ()
	{
		foreach (Character character in GameController.instance.characters) 
		{
			if (character.type == manaType)
			{
				if (SpawnManager.instance.CanSpawn(character))
				{
					GameObject.Find(character.charName).GetComponentInChildren<Button>().interactable = true;
				}
				else
				{
					GameObject.Find(character.charName).GetComponentInChildren<Button>().interactable = false;
				}
			}
		}
	}
}