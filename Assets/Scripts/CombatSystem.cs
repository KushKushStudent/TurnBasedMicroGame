using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum TurnPhases {START,PLAYERTURN,ENEMYTURN,WON,LOST }
public class CombatSystem : MonoBehaviour
{
    public HudController playerHud;
    public HudController enemyHud;
    public GameObject player;
    public GameObject enemy;
    Unit playerUnit;
    Unit enemyUnit;

    public Text dbText;

    public TurnPhases state;
   
    // Start is called before the first frame update
    void Start()
    {
        state = TurnPhases.START;
       StartCoroutine( SetupBattle());
        
    }

    IEnumerator SetupBattle() 
    {
      playerUnit=  player.GetComponent<Unit>();
        enemyUnit = enemy.GetComponent<Unit>();
        dbText.text = " A wounded " + playerUnit.name + " approaches!";
        playerHud.setHud(playerUnit);
        enemyHud.setHud(enemyUnit);
        yield return new WaitForSeconds(2f);
        state = TurnPhases.PLAYERTURN;
        PlayerTurn();
    }
    void PlayerTurn() 
    {
        dbText.text = "Choose an Action: ";
    }
   public void onAttackBtn() 
    {
        if (state!=TurnPhases.PLAYERTURN) 
        {
            return;        
        }
        StartCoroutine(PlayerAttack());
    }
    IEnumerator PlayerAttack()
    {
        if (Random.Range(1, 6) >= 3)
        {
            bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
        enemyHud.setHP(enemyUnit.currentHp);
            dbText.text = "Attack is succesful!";



            enemyUnit.TakeDamage(playerUnit.damage);
            yield return new WaitForSeconds(2f);
            if (isDead)
            {
                state = TurnPhases.WON;
                EndBattle();
            }
            else 
            {
                state = TurnPhases.ENEMYTURN;
                StartCoroutine(EnemyTurn());
            }
        }
        else
        {
            dbText.text = "Attack misses";
            yield return new WaitForSeconds(2f);
                state = TurnPhases.ENEMYTURN; 
            StartCoroutine(EnemyTurn());
        }

       
       
    }
    IEnumerator EnemyTurn()
    { int RandomAct = Random.Range(1, 6);
        if (RandomAct <= 3)
        {
            dbText.text = enemyUnit.name + " attacks!";
            yield return new WaitForSeconds(2f);
            if (Random.Range(1, 6) > 2)
            {
                bool isDead = playerUnit.TakeDamage(enemyUnit.damage);

                dbText.text = "Attack successful!";
                playerHud.setHP(playerUnit.currentHp);
                yield return new WaitForSeconds(1f);
                if (isDead)
                {
                    state = TurnPhases.LOST; EndBattle();

                }
                else 
                {
                    state = TurnPhases.PLAYERTURN;
                    PlayerTurn();
                }
            }
            else
            {

                dbText.text = "Attack has missed!";
                yield return new WaitForSeconds(2f);
                state = TurnPhases.PLAYERTURN;
                PlayerTurn();
               
                
            }

        }
        else { //add new attacks
              
               }
        

    }
     public void EndBattle()
    {
        if (state == TurnPhases.WON)
        {
            dbText.text = enemyUnit.name + " has fallen in battle! You reign victorious!";
        } else if (state==TurnPhases.LOST) 
        {
            dbText.text = "You have Fallen in battle.";
        }
    
    
    }
}
    // Update is called once per frame
   
