using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Nameplate : MonoBehaviour
{
    [SerializeField] 

    public TextMeshProUGUI charName;
    public Slider health;
    public Slider mana;
    public Image pic;


    public void displayName(string name) {
        charName.text = name;
    }

    public void displayImage(Sprite sp) {
        UnityEngine.Debug.Log("Setting Sprite");
        pic.sprite = sp;
    }

    public void displayHealth(int newhealth, stat newmaxhealth) {
        health.maxValue = (float)newmaxhealth.moddedValue;
        health.value = (float)newhealth;
    }

    public void displayMana(int newmana, stat newmaxmana) {
        mana.maxValue = newmaxmana.moddedValue;
        mana.value = newmana;
    }


}
