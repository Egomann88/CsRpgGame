﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RpgGame
{
  internal class Marketplace
  {
    // Klassenvariabeln
    private const byte WEAKHEALERPRICE = 25;
    private const byte NORMALHEALERPRICE = 45;
    private const byte STRONGHEALERPRICE = 80;
    private const byte LVLFORHIGHUSES = 8;
    private const int SHORTTIMEOUT = 700;
    private const int TIMEOUT = 1200;
    private const int LONGTIMEOUT = 2000;
    private const ushort STRPRICE = 220;
    private const ushort INTPRICE = 200;
    private const ushort DEXPRICE = 240;
    private const ushort HELPRICE = 210;
    private const ushort CCHPRICE = 460;
    private const ushort CDMPRICE = 440;

    // Membervariabeln

    // Konstruktoren
    public Marketplace(Character c) {
      Character = c;
    }


    // Methoden (funktionen)

    private Character Character { get; set; }

    public Character OnMarket() {
      bool onMarket = true;
      char input = '0';

      while (onMarket) {
        Console.Clear();
        Console.WriteLine("Ihr befindet Euch auf dem Marktplatz.\nWohin wollt Ihr gehen?");
        Console.WriteLine("1) Heiler\n2) Glückspiel\n3) Arena\n4) Verstärkungsmagier\n9) Marktplatz verlassen");
        input = Console.ReadKey().KeyChar;

        switch (input) {
          case '1': HealerOverView(); break;
          case '2': GamblingOverView(); break;
          case '3': ArenaOverView(); break;
          case '4': StatPushOverView(); break;
          case '9': onMarket = false; break;
          default: continue;
        }
      }

      return Character;
    }

    private void HealerOverView() {
      char input = '0';
      float healValue = 0;

      while (true) {
        Console.Clear();
        Console.WriteLine("Es gibt drei Heiler auf dem Markt:");
        Console.WriteLine("1) den Anfänger, er kann 25 % eures Lebens wiederherstellen (Preis: {0})\n" +
            "2) den Erfaherenen, er kann 45 % eures Lebens wiederherstellen (Preis: {1})\n" +
            "3) die Meisterin, sie kann eurere Leben komplett wiederherstellen (Preis: {2})\n" +
            "4) Zurück zum Marktplatz",
            WEAKHEALERPRICE, NORMALHEALERPRICE, STRONGHEALERPRICE);
        input = Console.ReadKey().KeyChar;

        switch (input) {
          case '1':
            if (Character.Gold >= WEAKHEALERPRICE) {
              healValue = Convert.ToSingle(Math.Round(Character.Health[1] * 0.25));

              Character.ChangeCurrentHealth((short)healValue);
              Console.WriteLine("{0} HP wurden wiederhergestellt.", healValue);
              Character.Gold -= WEAKHEALERPRICE;
            } else NotEnoughMoney();

            break;
          case '2':
            if (Character.Gold >= NORMALHEALERPRICE) {
              healValue = Convert.ToSingle(Math.Round(Character.Health[1] * 0.45));
              Character.ChangeCurrentHealth((short)healValue);
              Console.WriteLine("{0} HP wurden wiederhergestellt.", healValue);
              Character.Gold -= NORMALHEALERPRICE;
            } else NotEnoughMoney();

            break;
          case '3':
            if (Character.Lvl < LVLFORHIGHUSES) {
              Console.WriteLine("Die Heilerin lässt euch nicht hinein. Euer Level ist zu tief.");
              Thread.Sleep(SHORTTIMEOUT);
              continue;
            } else if (Character.Gold >= STRONGHEALERPRICE) {
              Character.FullHeal();
              Console.WriteLine("Komplettes Leben wurde wiederhergestellt.");
              Character.Gold -= STRONGHEALERPRICE;
            } else NotEnoughMoney();

            break;
          case '4': return;
          default: continue;
        }
        Thread.Sleep(SHORTTIMEOUT);
        return;
      }
    }

    private void GamblingOverView() {
      char input = '0';

      while (true) {
        Console.Clear();
        Console.WriteLine("Ihr könnt \"Rot oder Schwarz\" oder \"Höher oder Tiefer\" spielen.");
        Console.WriteLine("1) Rot oder Schwarz\n2) Höher oder Tiefer\n3) Zurück zum Marktplatz");
        input = Console.ReadKey().KeyChar;

        switch (input) {
          case '1': RedBlack(); break;
          case '2': HighLess(); break;
          case '3': return;
          default: continue;
        }
      }
    }

    private void RedBlack() {
      Random r = new Random();
      string[] moods = { "grimmige", "gelangweilte", "fröhliche" };
      string dealerMood = moods[r.Next(1, moods.Length)];
      string dealer = "Der {0} Spielmeister ";
      bool gameIsRed = false;
      bool characterIsRed = false;
      uint stake = 0;
      char input = '0';

      do {
        Console.Clear();
        Console.WriteLine(dealer + "fragt nach eurem Einsatz.", dealerMood);
        Console.Write("Euer Einsatz: ");
      } while (!uint.TryParse(Console.ReadLine(), out stake) && stake <= Character.Gold);

      Console.WriteLine(dealer + "fragt, für das Ihr wettet.");
      while (true) {
        Console.WriteLine("1) Rot\n 2) Schwarz");
        Console.Write("Eure Wahl: ");
        input = Console.ReadKey().KeyChar;

        if (input == '1') characterIsRed = true;
        else if (input == '2') characterIsRed = false;
        else continue;

        break;
      }

      Console.Write("Das Ergebnis ist");
      for (byte i = 0; i < 4; i++) {
        Console.Write(".");
        Thread.Sleep(SHORTTIMEOUT - 400);
      }

      if (r.Next(1, 3) == 1) {
        gameIsRed = true;
        Console.WriteLine("\nRot!");
      } else {
        gameIsRed = false;
        Console.WriteLine("\nSchwarz!");
      }

      if (characterIsRed == gameIsRed) {
        Console.WriteLine("Ihr habt gewonnen!");
        Character.Gold += Convert.ToInt32(stake);
      } else {
        Console.WriteLine("Ihr habt verloren...");
        Character.Gold -= Convert.ToInt32(stake);
      }

      Thread.Sleep(TIMEOUT);
    }

    private void HighLess() {
      Random r = new Random();
      char input = '0';
      bool playerIsHigher = false, gameIsHigher = false;
      int lastGameNumber = 0, gameNumber = 0, stake = 0;
      const int stdPlyValue = 20; // standard Play Value

      if (Character.Gold < stdPlyValue) {
        NotEnoughMoney();
        return;
      }

      Console.WriteLine("Eine Zahl zwischen 1 und 10 wird gewürfelt." +
          "Ihr müsst sagen, ob die nächste Zahl höher oder tiefer, als die jetzige sein wird.\n" +
          "Der erste Einsatz ist 20 Gold, dieser wird jede Runde verdoppelt. Das Spiel geht max. 5 Runden.");

      while (true) {
        int i = 1;
        stake = stdPlyValue;
        lastGameNumber = r.Next(1, 11);

        Console.WriteLine("Die {0}. Zahl ist eine {1}", i, lastGameNumber);

        while (true) {
          Console.WriteLine("1) Höher\n2) Tiefer\n3) Gewinn nehmen & gehen");
          input = Console.ReadKey().KeyChar;

          if (input == '1') { playerIsHigher = true; break; } else if (input == '2') { playerIsHigher = false; break; } else if (input == '3') { PayPlayer(true, stake); return; }    // end func with payout
                                                                                                                        else continue;
        }

        stake += 20;
        if (stake >= Character.Gold) {
          Console.WriteLine("Wenn Ihr verliert, könnt ihr nicht bezahlen.\nDie Runde wird beendet.");
          PayPlayer(true, stake - 20);
          Thread.Sleep(SHORTTIMEOUT);
          return;
        }

        gameNumber = r.Next(1, 11);

        Console.WriteLine("Die {0}. Zahl ist eine {1}", i + 1, gameNumber);
        if (gameNumber > lastGameNumber) gameIsHigher = true;
        else gameIsHigher = false;

        if (playerIsHigher == gameIsHigher) {
          Console.WriteLine("Ihr hattet recht!");
          PayPlayer(true, stake);
        } else {
          Console.WriteLine("Ihr lagt falsch...");
          PayPlayer(true, -stake);
        }
        break;
      }
    }

    private void PayPlayer(bool pWon, int stake) {
      string wonMsg = pWon ? "erhaltet" : "bezahlt";
      Console.WriteLine("Ihr {0} {1} Gold.", wonMsg, stake);
      Character.Gold += stake;
      Thread.Sleep(TIMEOUT);
    }

    private void ArenaOverView() {
      char input = '0';

      while (true) {
        Console.Clear();
        Console.WriteLine("In der Arena werdet ihr ausschliesslich starke Gegner treffen und wie der Zufall es will, " +
            "habt ihr die Möglichkeit gegen besonders starke Gegner zu kämpfen, mit höheren Belohnungen natürlich\n" +
            "In der Arena gelten nicht dieselben Regeln wie in der Wildnis. Hier könnt ihr nicht sterben.");
        Console.WriteLine("1) Normaler Arenakampf\n2) Kampf gegen starken Gegner\n3) Zurück zum Marktplatz.");
        input = Console.ReadKey().KeyChar;

        switch (input) {
          case '1': EvalEnemy(false); break;
          case '2': EvalEnemy(true); break;
          case '3': return;
          default: continue;
        }
      }
    }

    private void EvalEnemy(bool isHard) {
      Random r = new Random();
      byte enemyId = Convert.ToByte(r.Next(1, 101));

      if (enemyId <= 21) enemyId = 5; // 21 %
      else if (enemyId <= 42) enemyId = 6; // 21 %
      else if (enemyId <= 62) enemyId = 7; // 20 %
      else if (enemyId <= 80) enemyId = 8; // 18 %
      else if (enemyId <= 96) enemyId = 9; // 14 %
      else enemyId = 10; // 6 %

      Enemy e = new Enemy(Character.Lvl, enemyId, isHard);

      // how to start new fight?
      // new class, with inherits from fight?
    }

    // increase stats
    private void StatPushOverView() {
      char input = '0';

      if (Character.Lvl < LVLFORHIGHUSES) {
        Console.Clear();
        Console.WriteLine("Die Verstärkungsmagier lässt euch nicht hinein. Euer Level ist zu tief.");
        Thread.Sleep(SHORTTIMEOUT);
        return;
      }

      while (true) {
        Console.Clear();
        Console.WriteLine("Der Verstärkungsmagier kann euch, auf eine neue Ebene der Macht bringen," +
            "für einen kleinen Betrag natürlich.");
        Console.WriteLine("1) +1 Stärke (Preis: {0} Gold)\n2) +1 Inteligents (Preis: {1} Gold)\n3) +1 Geschicklichkeit (Preis: {2} Gold)\n" +
          "4) +5 Max Leben (Preis: {3} Gold)\n5) Krit. Chance + 2 % (Preis: {4} Gold)\n6) Krit. Schaden + 5 % (Preis: {5} Gold)\n" +
          "9) Zurück zum Marktplatzs",
          STRPRICE, INTPRICE, DEXPRICE, HELPRICE, CCHPRICE, CDMPRICE);
                  
        switch (input) {
          case '1':
            if (Character.Gold < STRPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.Strength++;
            break;
          case '2':
            if (Character.Gold < INTPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.Intelligents++;
            break;
          case '3':
            if (Character.Gold < DEXPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.Dexterity++;
            break;
          case '4':
            if (Character.Gold < HELPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.Health[1] += 5;
            break;
          case '5':
            if (Character.Gold < CCHPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.CritChance += 0.02F;
            break;
          case '6':
            if (Character.Gold < CDMPRICE) {
              NotEnoughMoney();
              continue;
            }
            Character.CritDmg += 0.05F;
            break;
          case '9': return;
          default: continue;
        }

      }
    }

    private void NotEnoughMoney() {
      Console.WriteLine("Ihr habt nicht genügend Geld.");
      Thread.Sleep(SHORTTIMEOUT);
    }
  }
}
