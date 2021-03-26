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
    public bool playerDefending;
    public bool enemyDefending;
    public Text dbText;
    public Text UltText;
    public int ultPts;
    public Slider UltSlider;
    public int EnemyUltPts;
    public Canvas StoreCanvas;

    public TurnPhases state;

    // Start is called before the first frame update
    void Start()
    {
        StoreCanvas.enabled = false;
        UltSlider.maxValue = 10;
        UltSlider.minValue = 0;
        UltSlider.value = 0;
        state = TurnPhases.START;
        StartCoroutine(SetupBattle());

    }
    private void Update()
    {
        UltText.text = "Ult Pts: " + ultPts + "/10";
        UltSlider.value = Mathf.Clamp(ultPts, 0, 10) ;

    }

    IEnumerator SetupBattle()
    {
        playerUnit = player.GetComponent<Unit>();
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
    public void OnStoreBtn()
    {
        if (state != TurnPhases.PLAYERTURN)
        {
            return;
        }
        else
        {
            StoreCanvas.enabled = true;


        }


    }
    public void onHealBtn()
    {
        if (ultPts>=4) 
        {
            StartCoroutine(HealthPotion());

        }else 
        { 
            return; 
        }
        

    }
    IEnumerator HealthPotion() 
    {
        ultPts -= 4;
        StoreCanvas.enabled = false;
        dbText.text = "Consuming Healing Potion... +5HP!";
        yield return new WaitForSeconds(2f);
        playerUnit.currentHp += 5;
        playerHud.setHud(playerUnit);
        state = TurnPhases.ENEMYTURN;
        StartCoroutine(EnemyTurn());


    }
    public void ExitBtn() 
    {
        Application.Quit();
    }
    public void onBackBtn() 
    {
        StoreCanvas.enabled = false;
    }
    public void onAttackBtn() 
    {
        if (state!=TurnPhases.PLAYERTURN) 
        {
            return;        
        }
        StartCoroutine(PlayerAttack());
    }
    public void onDefend() 
    {
        if (state != TurnPhases.PLAYERTURN)
        {
            return;
        } 
        StartCoroutine(PlayerDefend());


    }
    public void onUltimate()
    {
        if (state != TurnPhases.PLAYERTURN)
        {
            return;
        }
        if (ultPts == 10)
        {
            StartCoroutine(PlayerUlt());
        }
        else { return; }
       


    }
    IEnumerator PlayerUlt() 
    {
        bool isDead = enemyUnit.TakeDamage(playerUnit.UltDamage);
        dbText.text = "Ultimate unleashed! Enemy cannot defend!";
        ultPts = 0;
        enemyHud.setHP(enemyUnit.currentHp);
        yield return new WaitForSeconds(1f);
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
    IEnumerator PlayerDefend() 
    {
        playerDefending = true;
        dbText.text = "Entered defense stance.Next turn only damage is halved.";

        yield return new WaitForSeconds(2f);
        state = TurnPhases.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerAttack()
    {
        if (playerDefending==true) { playerDefending = false; }
        if (Random.Range(1, 6) >= 3)
        {
            if (enemyDefending == true)
            {
                bool isDead = enemyUnit.TakeDefendedDamage(playerUnit.damage);
                dbText.text = "Enemy defends. Attack partially successful.";
                EnemyUltPts++;
                ultPts++;
                Mathf.Clamp(ultPts, 0, 10);
                enemyHud.setHP(enemyUnit.currentHp);
                yield return new WaitForSeconds(1f);
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
                bool isDead = enemyUnit.TakeDamage(playerUnit.damage);
                enemyHud.setHP(enemyUnit.currentHp);
                ultPts += 2;
                dbText.text = "Attack is succesful!";
                Mathf.Clamp(ultPts, 0, 10);


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
    { if (enemyDefending == true) 
        {
            enemyDefending = false;
        
        }
        int RandomAct = Random.Range(1, 6);
        if (RandomAct <= 3)
        {
            dbText.text = enemyUnit.name + " attacks!";
            yield return new WaitForSeconds(2f);
            if (Random.Range(1, 6) > 2)
            {

                if (playerDefending == true)
                {
                    bool isDead = playerUnit.TakeDefendedDamage(enemyUnit.damage);
                    playerDefending = false;
                    EnemyUltPts++;
                    ultPts++;
                    Mathf.Clamp(EnemyUltPts, 0, 10);
                    dbText.text = "Player defends. Attack partially successful.";
                    playerHud.setHP(playerUnit.currentHp);
                    yield return new WaitForSeconds(1f);
                    if (isDead)
                    {
                        state = TurnPhases.LOST;
                        EndBattle();

                    }
                    else
                    {
                        state = TurnPhases.PLAYERTURN;
                        PlayerTurn();
                    }
                }
                else
                {
                    bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
                    dbText.text = "Attack successful!";
                    EnemyUltPts += 2;
                    Mathf.Clamp(EnemyUltPts, 0, 10);

                    playerHud.setHP(playerUnit.currentHp);
                    yield return new WaitForSeconds(1f);
                    if (isDead)
                    {
                        state = TurnPhases.LOST;
                        EndBattle();

                    }
                    else
                    {
                        state = TurnPhases.PLAYERTURN;
                        PlayerTurn();
                    }
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
        
        else if ((RandomAct == 6 && EnemyUltPts == 10)|| (RandomAct == 5 && EnemyUltPts == 10))
        {
            EnemyUltPts = 0;
            bool isDead = playerUnit.TakeDamage(enemyUnit.UltDamage);
            dbText.text = "Enemy unleashes Ultimate! You cannot defend.";
            EnemyUltPts += 2;
            Mathf.Clamp(EnemyUltPts, 0, 10);

            playerHud.setHP(playerUnit.currentHp);
            yield return new WaitForSeconds(2f);
            if (isDead)
            {
                state = TurnPhases.LOST;
                EndBattle();

            }
            else
            {
                dbText.text = "Enemy smoked some weed and is tripping crazy. No action made.";
                state = TurnPhases.PLAYERTURN;
                PlayerTurn();
            }
        }
        else if (RandomAct == 4 || RandomAct == 5)
        {
            enemyDefending = true;
            dbText.text = "Entered defense stance.Next turn only damage dealt is halved.";

            yield return new WaitForSeconds(2f);
            state = TurnPhases.PLAYERTURN;
            PlayerTurn();


        }
        else { }
        

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
   
