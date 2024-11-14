using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class Hero_Character_Class : MonoBehaviour
{
    [SerializeField]
    public int mana;  //Related Functions - useMana, enoughMana, regenMana
    public stat maxMana;
    public stat magic;  //Related Functions - increaseMagic, decreaseMagic
    public bool pickingSpell = false;
    public List<Basic_Spell_Class> spellList;
    public Basic_Spell_Class selectedSpell = null;

    void Start()
    {
        if (this.gameObject.GetComponent<SpellList>() != null)
        {
            spellList = this.gameObject.GetComponent<SpellList>().spellList;
        }
    }


    // Update is called once per frame
    void Update()
    {
        //FOR TESTING PURPOSES ------ REDUCES THE CURRENT MANA COUNT BY 10 WHEN A IS PRESSED AND INCREASES THE CURRENT MANA COUNT BY 10 WHEN S IS PRESSED
        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            useMana(10);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            regenMana(10);
        }
        */
        
        if (pickingSpell && Input.GetMouseButtonDown(0))
        {
            pickingSpell = false;
            gameObject.GetComponent<Basic_Character_Class>().renderer.material.color = Color.red;

        }
        else if (pickingSpell)
        {
            gameObject.GetComponent<Basic_Character_Class>().renderer.material.color = Color.cyan;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gameObject.GetComponent<Basic_Character_Class>().beginTargetingSpell(spellList[0].range, spellList[0]);
                selectedSpell = spellList[0];
                pickingSpell = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && pickingSpell == true)
        {
            selectedSpell = null;
            pickingSpell = false;
        }
        
    }

    //Reduces the amount of current mana a character has
    //Input - Amount of mana to use
    //Output - Returns True if the mana was reduced, returns False if not

    bool useMana(int amount){
        if (enoughMana(amount))
        {
           mana = mana - amount;
           UnityEngine.Debug.Log("Used " + amount + " Mana. Current Mana is: " + mana);
           return true;
        }
        return false;
    }

    //Checks if there is enough mana for an ability to be used
    //Output - Returns True is there is, False if there is not

    bool enoughMana(int amount){
        if (mana - amount >= 0)
        {
            return true;
        }
        UnityEngine.Debug.Log("Not Enough Mana");
        return false;
    }

    //Regenerates Mana for the character, but not over the characters maximum amount
    //Input - Amount of mana to regenerate

    void regenMana(int amount){
        mana = mana + amount;
        if (mana > maxMana.moddedValue)
        {
            mana = maxMana.moddedValue;
            UnityEngine.Debug.Log("At Maximum Mana");
            return;
        }
        UnityEngine.Debug.Log("Regenerated " + amount + " Mana. Current Mana is: " + mana);
        return;
    }

    //Increases the character's magic value
    //Input - Amount of magic to increase by

    void increaseMagic(int amount){
        magic.moddedValue += amount;
    }

    //Decreases the character's magic value
    //Input - Amount of magic to decrease by

    void decreaseMagic(int amount){
        magic.moddedValue -= amount;
    }

    public void OnMouseDown()
    {
        UnityEngine.Debug.Log("here");
        if (spellList != null)
        {
            for (int i = 0; i < spellList.Count; i++)
            {
                UnityEngine.Debug.Log(spellList[i].name);
            }
        }

    }

    public void openSpellBook()
    {
        this.gameObject.GetComponent<Basic_Character_Class>().map.hidePath();
        pickingSpell = true;
        UnityEngine.Debug.Log("Spellbook for " + gameObject.GetComponent<Basic_Character_Class>().name + " is open. Press the corresponding number to begin targeting the spell");
        for(int i = 0; i < spellList.Count; i++)
        {
            UnityEngine.Debug.Log((i + 1) + ": " + spellList[i].name + ", " + spellList[i].description.Replace("{Damage Value}", magic.moddedValue.ToString()));
        }
    }

    public void castSpell(List<GameObject> targets)
    {

        Basic_Spell_Class spellInstance = Instantiate(selectedSpell);
        spellInstance.spellPrefab.GetComponent<Cast_Spell>().castSpell(targets, this.gameObject);
        

    }

}
