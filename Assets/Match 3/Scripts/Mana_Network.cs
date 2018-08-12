using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mana_Network : MonoBehaviour 
{
	public GamePiece_Network.Type manaType;

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

	public static Mana_Network GetMana (GamePiece_Network.Type manaType, Mana_Network[] allManasReference)
	{
		foreach (Mana_Network mana in allManasReference) 
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
		foreach (Character_Network character in GameController_Network.instance.characters) 
		{
			if (character.type == manaType)
			{
				if (SpawnManager_Network.instance.CanSpawn(character))
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