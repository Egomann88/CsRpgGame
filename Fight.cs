﻿using System;
using System.Linq;
using System.Threading;

namespace RpgGame
{
  internal class Fight
  {
    // Klassenvariablen
    protected const byte HEALCOOLDOWN = 3;    // Default cooldown of both sides for the healpotion
    protected const byte ULTIMATECOOLDOWN = 4; // Default cooldown for both sides on the ultimate
    protected const byte ULTHITBONUS = 20;  // 20 % hit bonus for Ultimate
    protected const int SHORTTIMEOUT = 800;
    protected const int TIMEOUT = 1200;
    protected short[] CHARACTERCOOLDOWN = new short[] { HEALCOOLDOWN, ULTIMATECOOLDOWN };    // Heal, Ult
    protected short[] ENEMYCOOLDOWN = new short[] { HEALCOOLDOWN, ULTIMATECOOLDOWN }; // Heal, Ult


    // Membervariablen

    // Konstruktor
    /// <summary>
    /// Need an Character and Enemy to fight.
    /// </summary>
    /// <param name="c">Character Object</param>
    /// <param name="e">Enemy Object</param>
    public Fight(Character c, Enemy e) {
      Character = c;
      Enemy = e;
      RoundCount = 0;
    }

    // Methoden (Funktionen)
    protected Character Character { get; set; }

    protected Enemy Enemy { get; set; }

    protected byte RoundCount { get; set; }

    /// <summary>
    /// Simulates the entire fight, with exp + gold if won<br />
    /// ! Outside must be checked whether the Character is still alive !
    /// </summary>
    /// <returns>Character with new stats</returns>
    virtual public Character FightIn() {
      bool fightOver = false;
      bool fled = false;
      bool isPlayerFirst = GetFirstMove();
      byte playerTurns = GetNumOfTurns(true);
      byte enemyTurns = GetNumOfTurns(false);

      Console.Clear();
      Console.WriteLine("Ein {0} seit auf der Hut.", Enemy.Name);
      Thread.Sleep(SHORTTIMEOUT);

      do {
        Console.Clear();    // clear all fighting texts

        if (isPlayerFirst) {
          for (byte i = 0; i < playerTurns; i++) {    // repeat as long as Player still has turns
            fled = PlayerTurn(); // if player fled, jump direct to end
            if (Enemy.Health[0] <= 0 || fled) {
              fightOver = true;
              break;
            }
          }
          if (fightOver) continue;    // go to end of while-loop -> enemy died
        }

        for (byte i = 0; i < enemyTurns; i++) { // repeat as long as Enemy still has turns
          EnemyTurn();
          if (Character.Health[0] <= 0) {
            fightOver = true;
            break;  // break out of for-loop
          }
        }
        if (fightOver) continue;    // go to end of while-loop -> character died

        if (!isPlayerFirst) {
          for (byte i = 0; i < playerTurns; i++) {    // repeat as long as Player still has turns
            fightOver = fled = PlayerTurn(); // if player fled, jump direct to end
            if (Enemy.Health[0] <= 0) {
              fightOver = true;
              break;
            }
          }
          if (fightOver) continue;    // go to end of while-loop -> enemy died
        }

        RoundCount++;

      } while (!fightOver);

      Console.Clear();
      if (fled) Console.WriteLine("{0} ist geflohen!", Character.Name);
      else if (Character.Health[0] <= 0) Console.WriteLine("{0} ist gestorben...", Character.Name);
      else {  // defeated enemy
        Console.WriteLine("{0} war siegreich!\n{1} Exp erhalten.\n{2} Gold erhalten.", Character.Name, Enemy.Exp, Enemy.Gold);
        // get enemy gold and exp
        Character.Exp[0] += Enemy.Exp;
        Character.ChangeAmoutOfGold(Enemy.Gold);

        // player lvl up
        Character.IncreaseLvl();
      }

      Console.WriteLine("\n\nDrücken Sie auf eine Taste, um fortzufahren...");
      Console.ReadKey(true);
      
      return Character;
    }

    /// <summary>
    /// Simulates Players turn
    /// </summary>
    /// <returns>true if player fled - false, if not</returns>
    virtual protected bool PlayerTurn() {
      short[] coolDown = GetCoolDown(true);   // cooldown of abilitys
      string ultimateName = GetUltimateName();
      string actionText = ""; // what player will do
      ushort damage = 0;  // players dmg
      ushort chance2Hit = (ushort)(75 + Character.Dexterity - Enemy.Dexterity); // 75 % base value + char dex - enemy dex (dodge chance)
      char input = '0';   // player input
      bool flee = false;

      do {
        Console.Clear();
        Console.WriteLine("{0}, was wollt ihr machen?\nLeben: {1} / {2}", Character.Name, Character.Health[0], Character.Health[1]);
        Console.Write("1) Angreifen\n2) Heilen (Abklingzeit: {0} Runden)\n3) {1} (Abklingzeit: {2} Runden)\n4) Fliehen"
            , coolDown[0], ultimateName, coolDown[1]);
        input = Console.ReadKey(true).KeyChar; // do not show input in console
        Console.Clear();
        switch (input) {
          case '1':
            damage = Character.Strength;

            actionText = $"{Character.Name} greift an.\n";

            if (IsCritDodge(Character.CritChance)) {
              damage = Convert.ToUInt16(Math.Round(damage * Character.CritDmg));
              actionText += "Kritischer Treffer!\n";
              chance2Hit = 100; // Crit is always an hit
            }
            
            if(!IsCritDodge(chance2Hit)) {
              actionText += $"{Enemy.Name} ist ausgewichen!\n";
              damage = 0;
            } else actionText += $"{damage} Schaden!";


            Enemy.ChangeCurrentHealth(Convert.ToInt16(-damage));
            break;
          case '2':
            // if abilty is still on cooldown, go back to start
            if (IsCharacterOnCoolDown(coolDown[0])) continue;

            damage = Character.Intelligents;

            actionText = $"{Character.Name} heilt sich.\n{damage} Leben wiederhergestellt";

            Character.ChangeCurrentHealth(Convert.ToInt16(damage));

            coolDown[0] = HEALCOOLDOWN;    // set heal cooldown
            break;
          case '3':
            // if abilty is still on cooldown, go back to start
            if (IsCharacterOnCoolDown(coolDown[1])) continue;

            damage = GetCharacterUltimate();

            actionText = $"{Character.Name} nutzt seine Ultimatie Fähigkeit \"{GetUltimateName()}\".\n";

            if (IsCritDodge(Character.CritChance)) {
              damage = Convert.ToUInt16(Math.Round(damage * Character.CritDmg));
              actionText += "Kritischer Treffer!\n";
              chance2Hit = 100; // Crit is always an hit
            }

            if (!IsCritDodge(chance2Hit + ULTHITBONUS)) { // ultimate has extra hit chance
              actionText += $"{Enemy.Name} ist ausgewichen!\n";
              damage = 0;
            } else actionText += $"{damage} Schaden!";

            Enemy.ChangeCurrentHealth(Convert.ToInt16(-damage));
            coolDown[1] = ULTIMATECOOLDOWN;    // set ulti cooldown
            break;
          case '4':
            actionText = $"{Character.Name} versucht zu fliehen.\n";

            if (IsFled()) flee = true;
            else actionText += "Fehlgeschalgen!";
            break;
          default: continue;  // must give new input
        }

        Console.WriteLine(actionText);

        Thread.Sleep(TIMEOUT);

        break;  // break out of loop
      } while (true);

      coolDown = coolDown.Select(x => --x).ToArray();   // decrease cooldowns by one

      CHARACTERCOOLDOWN = coolDown;  // save cooldowns for next round

      return flee;
    }

    /// <summary>
    /// Simulates the round of the enemy
    /// </summary>
    protected void EnemyTurn() {
      Random r = new Random();
      byte numberPool = 3;    // Attack, Heal, Ultimate
      short[] coolDown = GetCoolDown(false);  // apply current cooldowns
      ushort chance2Hit = (ushort)(75 + Enemy.Dexterity - Character.Dexterity); // 75 % base value - char dex (dodge chance)
      string actionText = "";
      ushort damage = 0;
      byte rnd = 0;

      if (coolDown[0] > 0) numberPool--; // if move is on cooldown, dont try to use it
      if (coolDown[1] > 0) numberPool--; // if move is on cooldown, dont try to use it

      rnd = Convert.ToByte(r.Next(1, numberPool + 1));    // roll next move

      if (rnd == 2 && coolDown[0] > 0) rnd = 3;   // if heal is on cd & heal is used -> use ultimate

      coolDown = coolDown.Select(x => --x).ToArray();   // decrease cooldowns by one

      switch (rnd) {
        case 1:
          damage = Enemy.Strength;
          actionText = $"{Enemy.Name} greift an.\n";

          if (IsCritDodge(Enemy.CritChance)) {
            damage = Convert.ToUInt16(Math.Round(damage * Enemy.CritDmg));
            actionText += " Kritischer Treffer!\n";
            chance2Hit = 100; // Crit is always an hit
          }

          if (!IsCritDodge(chance2Hit)) {
            actionText += $"{Character.Name} ist ausgewichen!\n";
            damage = 0;
          } else actionText += $"{damage} Schaden!";

          Character.ChangeCurrentHealth(Convert.ToInt16(-damage));
          break;
        case 2:
          damage = Enemy.Intelligents;
          actionText += $"{Enemy.Name} heilt sich.\n{damage} Leben wiederhergestellt.";

          Enemy.ChangeCurrentHealth(Convert.ToInt16(damage));

          coolDown[0] = HEALCOOLDOWN;    // set ability cooldown
          break;
        case 3:
          if (Enemy.IsDmgUlt) {
            // increase dmg with all possible variables
            damage = Convert.ToUInt16(Math.Round(Enemy.Strength + Enemy.Dexterity + Enemy.Intelligents * 1.5));
            actionText += $"{Enemy.Name} nutzt seine Ultimative Fähigkeit.\n";

            if (IsCritDodge(Enemy.CritChance)) {
              damage = Convert.ToUInt16(Math.Round(damage * Enemy.CritDmg));
              actionText += "Kritischer Treffer!\n";
              chance2Hit = 100; // Crit is always an hit
            }

            if (!IsCritDodge(chance2Hit)) {
              actionText += $"{Character.Name} ist ausgewichen!\n";
              damage = 0;
            } else actionText += $"{damage} Schaden!";

            Character.ChangeCurrentHealth(Convert.ToInt16(-damage));
          } else {
            // Heals himself with 1.2 of his Intelligents + 20 % of his max Health
            damage = Convert.ToUInt16(Math.Round(Enemy.Intelligents * 1.2 + Enemy.Health[1] / 5));
            actionText += $"{Enemy.Name} heilt sich enorm.\n{damage} Leben wiederhergestellt.";
            Enemy.ChangeCurrentHealth(Convert.ToInt16(damage), true);   // overheal allowed
          }

          coolDown[1] = ULTIMATECOOLDOWN;    // set ability cooldown
          break;
      }

      Console.WriteLine(actionText);
      ENEMYCOOLDOWN = coolDown;  // save Enemycooldown for next round
      Thread.Sleep(TIMEOUT);
    }

    /// <summary>
    /// Gives Ultimate diffrent name per class
    /// </summary>
    /// <returns>name -> string</returns>
    protected string GetUltimateName() {
      string name = "";

      switch (Character.Class) {
        case 1: name = "Bodenspalter"; break;
        case 2: name = "Meteorschauer"; break;
        case 3: name = "Exitus"; break;
      }

      return name;
    }

    /// <summary>
    /// Checks how dex is higher, the one with the higher one, is first on turn
    /// </summary>
    /// <returns>true, if players first - false, if not</returns>
    protected bool GetFirstMove() {
      Random r = new Random();

      // both are equal -> rnd shall decide
      if (Character.Dexterity == Enemy.Dexterity) return r.Next(1, 3) == 1 ? true : false;
      else if (Character.Dexterity > Enemy.Dexterity) return true;
      else return false;
    }

    /// <summary>
    /// Declairs how often the player / enemy is able to attack
    /// </summary>
    /// <param name="isPlayer">true if is player - false if enemy</param>
    /// <returns>num of turns</returns>
    protected byte GetNumOfTurns(bool isPlayer) {
      byte pTurns = 1, eTurns = 1;
      ushort pDex = Character.Dexterity;
      ushort eDex = Enemy.Dexterity;

      // starts with turn 2, cuz turn 1 is already definded
      while (pDex - (eDex + 5) >= 5) {
        pTurns++;
        if (pTurns == 5) break; // max 5 Turns
        eDex += 5;
      }

      while (eDex - (pDex + 5) >= 5) {
        eTurns++;
        if (eTurns == 5) break; // max 5 Turns
        pDex += 5;
      }

      return isPlayer ? pTurns : eTurns;
    }

    /// <summary>
    /// Every Class has a diffrent Ult, damage wil also be diffrenty calculated.
    /// </summary>
    /// <returns>damage -> uhsort</returns>
    protected ushort GetCharacterUltimate() {
      ushort damage = 0;
      switch (Character.Class) {
        case 1: // warrior
          if (Character.Strength * 2 + (Character.Intelligents / 2) - Math.Round(RoundCount * 1.2) <= 0) damage = 1;
          else damage = Convert.ToUInt16(Character.Strength * 2 + (Character.Intelligents / 2) - Math.Round(RoundCount * 1.2));
          break;
        case 2: // mage
          // if number of shot metors is lesser than 1, just use 1
          int countMetores = Character.Intelligents * 0.2 < 1 ? 1 : Convert.ToInt32(Character.Intelligents * 0.2);
          damage = Convert.ToUInt16(Math.Round(Character.Intelligents * 0.6 * countMetores));
          break;
        case 3: // thief
          // if hp is overheal, zero dmg, instead of -dmg
          int hpDmg = Character.Health[1] - Character.Health[0] < 0 ? 0 : Character.Health[1] - Character.Health[0];
          damage = Convert.ToUInt16(hpDmg + Character.Dexterity + RoundCount);

          // heals character with a quater of dealt dmg (no decimal number + no round + overheal allowed)
          Character.ChangeCurrentHealth((short)(damage / 4), true); // send copy of dmg, cuz its needed for return
          break;
      }

      return damage;
    }

    /// <summary>
    /// Get Enemy or Character Ability cooldowns<br />
    /// If cooldown ist under 0, it will be overwritten with it.
    /// </summary>
    /// <param name="isCharacter">true if its the Characters cooldown. If not -> false</param>
    /// <returns>coolodown -> short[]</returns>
    protected short[] GetCoolDown(bool isCharacter) {
      short[] coolDown = isCharacter ? CHARACTERCOOLDOWN : ENEMYCOOLDOWN;

      if (coolDown[0] <= 0) coolDown[0] = 0;  // does not allow the counter to go under 0
      if (coolDown[1] <= 0) coolDown[1] = 0;

      return coolDown;
    }

    /// <summary>
    /// Checks if was able to flee
    /// </summary>
    /// <returns>true - succeeded / false - failed</returns>
    private bool IsFled() {
      Random r = new Random();
      int rnd = r.Next(1, 5);

      if (rnd > 1) return true;  // 75 %

      return false;
    }

    /// <summary>
    /// Player method only.<br />
    /// Checks if abilty is on cooldown
    /// </summary>
    /// <param name="coolDown">cooldown of current abilty</param>
    /// <returns>bool -> true - Cooldown / false - no Cooldown</returns>
    protected bool IsCharacterOnCoolDown(short coolDown) {
      if (coolDown > 0) return true;

      return false;
    }

    /// <summary>
    /// Checks if attack is an cirt<br />
    /// OR<br />
    /// Checks if attack was dodged
    /// </summary>
    /// <param name="chance">Crit / Dodge Chance of Player / Enemy</param>
    /// <returns>bool -> Cirt / Dodge or no Crit / Dodge</returns>
    protected bool IsCritDodge(float chance) {
      Random r = new Random();
      double i = r.NextDouble() * 100;

      if (i > chance) return false; // no crit / dodge (enemy dodged)

      return true;  // crit or (no) dodge
    }
  }
}
