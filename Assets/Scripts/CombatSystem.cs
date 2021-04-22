using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum TurnPhases {START,PLAYERTURN,ENEMYTURN,WON,LOST }
public class CombatSystem : MonoBehaviour
{
    public HudController playerHud;
    public StoreController SC;
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
    public Text storeText;
    public bool PlayerRateboost;
    public float PlayerAttackPerc;
    public int RateBoostUses=2;
    public float standardRate=50;
    public TurnPhases state;
    public int DamageDealt;
    public bool DamageBoostActive;
    

    // Start is called before the first frame update
    void Start()
    {
        SC.RefreshStore();
        DamageBoostActive = false;
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
        if (ultPts>=SC.HealCost) 
        {
            StartCoroutine(HealthPotion());

        }else 
        {
            storeText.text = "Not enough UP!";
            return; 
        }
        

    }
    IEnumerator HealthPotion() 
    {
        ultPts -= SC.HealCost;
        SC.increaseHealCost();
        SC.RefreshStore();
        
        storeText.text = "Consuming Health Potion! +5HP";
       
      
     
        yield return new WaitForSeconds(2f);
        playerUnit.currentHp += 5;
        playerHud.setHud(playerUnit);

       


    }
    public void onRateBoostClick()
    {
        if (ultPts>=SC.HRCost)
        {
            ultPts -= SC.HRCost;
            SC.increaseHRCost();  SC.RefreshStore();
            RateBoostUses = 2;
            PlayerAttackPerc += 20;
            storeText.text = "Purchased Hit Rate Boost. The next 2 attacks have a +20% hit chance. ";

        }
        else
        {
            storeText.text = "Not enough UP!";
            return;
        }
    
    }  
    public void onDamageBoostClick()
    {
        if (ultPts>=SC.DBCost)
        {
            ultPts -= SC.DBCost;
            SC.increaseDBCost(); SC.RefreshStore();
            DamageBoostActive = true;
            PlayerAttackPerc += 20;
            storeText.text = "Damage boost now active! 1.5x damage! ";

        }
        else
        {
            storeText.text = "Not enough UP!";
            return;
        }
    
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
        StartCoroutine(PlayerAttack2());
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
        playerUnit.Defending= true;
        dbText.text = "Entered defense stance.Next turn only damage is halved.";

        yield return new WaitForSeconds(2f);
        state = TurnPhases.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerAttack()
    {
        if (playerUnit.Defending==true) { playerUnit.Defending = false; }
        if (Random.Range(1, 6) >= 3)
        {
            if (enemyUnit.Defending== true)
            {
                bool isDead = enemyUnit.TakeDefendedDamage(playerUnit.MaxRoundDamage);
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
                bool isDead = enemyUnit.TakeDamage(playerUnit.MaxRoundDamage);
                enemyHud.setHP(enemyUnit.currentHp);
                ultPts += 3;
                dbText.text = "Attack is succesful!";
                Mathf.Clamp(ultPts, 0, 10);


                enemyUnit.TakeDamage(playerUnit.MaxRoundDamage);
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
    IEnumerator PlayerAttack2() 
    {
        bool isDead = false; 
        yield return new WaitForSeconds(1);
        if (RateBoostUses == 0)
        {
            PlayerAttackPerc = 0; 
        }
       
            int rollVal= Random.Range(0, 100);
            if ((rollVal + PlayerAttackPerc) >= 40) //attack hits
            {
                     DamageDealt = playerUnit.MaxRoundDamage;
                         if (DamageBoostActive == true)
                         {
                                  DamageDealt = playerUnit.MaxRoundDamage * (int)1.5f;
                         }

                //  DamageDealt = (int)3*((playerUnit.unitLevel *2* (Mathf.Clamp(playerUnit.currentHp, playerUnit.MaxRoundDamage,100) / playerUnit.MaxRoundDamage))/(playerUnit.MaxRoundDamage-DamageBoostAmmount)); Damage Formula drafting, requires more time to test. Sticking with simple calc. issues with floats too broad to change now.
            if (enemyUnit.Defending == true)
            {
                DamageDealt = DamageDealt / 2;
                isDead = enemyUnit.TakeDamage((int)(DamageDealt));

                dbText.text = "Enemy defends. Attack partially successful.";
                EnemyUltPts+=2;
                ultPts+=3;
                Mathf.Clamp(ultPts, 0, 10);
                enemyHud.setHP(enemyUnit.currentHp);
                yield return new WaitForSeconds(3f);

            }
            else
            {
               
                
                isDead = enemyUnit.TakeDamage((int)(DamageDealt));
                enemyHud.setHP(enemyUnit.currentHp);
                ultPts += 3;
                dbText.text = "Attack is succesful!";
                Mathf.Clamp(ultPts, 0, 10);



                yield return new WaitForSeconds(3f);
            }
          
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


        //attack fails
        else
        {
            dbText.text = "Attack misses";
            yield return new WaitForSeconds(2f);
            state = TurnPhases.ENEMYTURN;
            StartCoroutine(EnemyTurn());

        }
        RateBoostUses--;
        Mathf.Clamp(RateBoostUses, 0, 2);
    }
    IEnumerator EnemyTurn()
    { if (enemyUnit.Defending == true)  
        {enemyUnit.Defending = false;
        
        }
        int RandomAct = Random.Range(1, 6);
        if (RandomAct <= 3)
        {
            dbText.text = enemyUnit.name + " attacks!";
            yield return new WaitForSeconds(2f);
            if (Random.Range(1, 6) > 2)
            {

                if (playerUnit.Defending== true)
                {
                    bool isDead = playerUnit.TakeDefendedDamage(enemyUnit.MaxRoundDamage);
                    playerUnit.Defending = false;
                    EnemyUltPts+=2;
                    ultPts+=2;
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
                    bool isDead = playerUnit.TakeDamage(enemyUnit.MaxRoundDamage);
                    dbText.text = "Attack successful!";
                    EnemyUltPts += 3;
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
            EnemyUltPts += 3;
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
            enemyUnit.Defending =true;
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
   
