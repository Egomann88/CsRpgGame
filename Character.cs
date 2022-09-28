﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; // timeout
using System.Text.Json; // has to be installed in nuget Package
using System.IO;  // to create and read files

namespace RpgGame
{
  internal class Character :RegexMethods {
    // Klassenvariabeln


    // Membervariablen

    // Konstruktoren
    public Character() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">playername</param>
    /// <param name="cl">class of player</param>
    public Character(string name, byte cl) {
      Name = name;
      Class = cl;
      Exp = new uint[] { 0, 30 };
      Lvl = 1;

      switch (cl) {
        case 1: // warrior
          Strength = 4;
          Intelligents = 2;
          Dexterity = 2;
          CritChance = 2.4F;
          CritDmg = 1.25F;
          Health = new short[] { 30, 30 };
          Gold = 29;
          break;
        case 2: // mage
          Strength = 2;
          Intelligents = 4;
          Dexterity = 2;
          CritChance = 4.8F;
          CritDmg = 1.5F;
          Health = new short[] { 22, 22 };
          Gold = 21;
          break;
        case 3: // thief
          Strength = 2;
          Intelligents = 2;
          Dexterity = 4;
          CritChance = 3.2F;
          CritDmg = 1.75F;
          Health = new short[] { 26, 26 };
          Gold = 36;
          break;
        default: break;  // should not get here -> check for wrong input MUST happen before
      }
    }

    // Methoden 

    public string Name { get; set; }

    public byte Class { get; set; }

    public ushort Strength { get; set; }

    public ushort Intelligents { get; set; }

    public ushort Dexterity { get; set; }

    public float CritChance { get; set; }

    public float CritDmg { get; set; }

    public short[] Health { get; set; }

    public int Gold { get; set; }

    public uint[] Exp { get; set; }

    public byte Lvl { get; set; }

    /// <summary>
    /// Creates an new Character with Name and Class.<br />
    /// </summary>
    /// <returns>Character</returns>
    public static Character CreateCharacter() {
      string name = "";
      byte cl = 0;

      while (name == "") {
        Console.Clear();
        Console.WriteLine("Geben Sie den Namen ihres Charakters ein:");
        name = Console.ReadLine();

        if (IsInValidSign(name)) {
          Console.WriteLine("\nIm Namen ist ein unerlaubtes Zeichen enthalten!");
          Thread.Sleep(500);
          continue;
        } else if (name == "" || name == " ") {
          Console.WriteLine("\nDer Name darf nicht leer sein!");
          Thread.Sleep(500);
          continue;
        }

        // convert to char array of the string
        char[] letters = name.ToCharArray();
        // upper case the first char
        letters[0] = char.ToUpper(letters[0]);
        // put array back together
        name = new string(letters);
      }

      do {
        Console.Clear();
        Console.WriteLine("Was ist die Klasse ihres Charakters?\n1) Krieger\n2) Magier\n3) Schurke");
        cl = Convert.ToByte(Console.ReadKey(false).KeyChar - 48);
      } while (cl < 1 || cl > 4);

      return new Character(name, cl);
    }

    /// <summary>
    /// Saves Json File with current Character Stats<br />
    /// https://www.nuget.org/packages/System.Text.Json<br />
    /// </summary>
    /// <param name="c">current character</param>
    public static void SaveCharacter(Character c) {
      Character _characterData = new Character(c.Name, 0) {
        Name = c.Name,
        Class = c.Class,
        Strength = c.Strength,
        Intelligents = c.Intelligents,
        Dexterity = c.Dexterity,
        CritChance = c.CritChance,
        CritDmg = c.CritDmg,
        Health = c.Health,
        Gold = c.Gold,
        Exp = c.Exp,
        Lvl = c.Lvl,
      };

      string path = Directory.GetCurrentDirectory();  // current Path
      string json = JsonSerializer.Serialize(_characterData);

      try {
        File.WriteAllText(path + @"\character_saves\" + c.Name + @".json", json);
        Console.Clear();
        Console.WriteLine("Speichern erfolgreich.");
      } catch (InvalidCastException e) { }

      Thread.Sleep(600);
    }

    public static bool HasCharacters() {
      // https://www.geeksforgeeks.org/c-sharp-program-for-listing-the-files-in-a-directory/
      string path = Directory.GetCurrentDirectory() + @"\character_saves\";  // current Path
      DirectoryInfo characterSaves = new DirectoryInfo(path);
      FileInfo[] Files = characterSaves.GetFiles();

      if (Files == null) return false;
      return true;
    }

    public static Character GetCharacters() {
      // https://www.geeksforgeeks.org/c-sharp-program-for-listing-the-files-in-a-directory/
      string path = Directory.GetCurrentDirectory() + @"\character_saves\";  // current Path
      DirectoryInfo characterSaves = new DirectoryInfo(path);
      FileInfo[] Files = characterSaves.GetFiles();
      List<Character> charactersList = new List<Character>();
      byte choosenCharacterId = 0;

      // fill character list
      // https://www.tutorialsteacher.com/articles/convert-json-string-to-object-in-csharp
      foreach (FileInfo i in Files) {
        string jsonCharacterData = File.ReadAllText(path + i);
        charactersList.Add(JsonSerializer.Deserialize<Character>(jsonCharacterData));
      }

      // list all characters
      Character[] characters = charactersList.ToArray();  // convert list to array
      ListCharacters(characters);

      choosenCharacterId = ChooseCharacter(); // player input

      if (choosenCharacterId == 0) return CreateCharacter();  // create new character

      // decrease id by one to be sync with the array
      if (CanLoadCharacter(--choosenCharacterId, characters)) return LoadCharacter(--choosenCharacterId, characters);
      else throw new IndexOutOfRangeException("Die geladene Characterdatei ist korrput.");
    }
    private static void ListCharacters(Character[] characters) {
      Console.WriteLine("Welcher Charakter soll geladen werden:");
      Console.WriteLine("0) keiner (neuen Character erstellen)");

      for (byte i = 0; i < characters.Length; i++) {
        if (i == 255) break;
        Console.WriteLine("{0}) {1}, {2} (Level: {3})",
          i + 1, characters[i].Name, characters[i].GetClassName(), characters[i].Lvl
        );
      }
    }

    private static byte ChooseCharacter() {
      byte input = 0;

      do {
        Console.Write("Eingabe: ");
      } while (!byte.TryParse(Console.ReadLine(), out input));

      return input;
    }

    /// <summary>
    /// checks if charactersave if correct and can be loaded
    /// </summary>
    /// <param name="characterId">Id of loading Character</param>
    /// <param name="characters">Array of all Characters</param>
    /// <returns></returns>
    private static bool CanLoadCharacter(byte characterId, Character[] characters) {
      if (IsCharacterValid(characters[characterId])) return true;

      return false;
    }

    /// <summary>
    /// Checks if the loaded save wasnt modified.<br />
    /// If it was modified, it cannot be loaded
    /// </summary>
    /// <param name="c">characer object</param>
    /// <returns>true - if character is valid / flase - if not</returns>
    private static bool IsCharacterValid(Character c) {
      bool nameVaild = false, classValid = false;

      if (c.Name == "" || IsInValidSign(c.Name)) nameVaild = true;
      if (c.Class > 1 && c.Class < 4) classValid = true;
      c.Strength = c.Strength;
      c.Intelligents = c.Intelligents;
      c.Dexterity = c.Dexterity;
      c.CritChance = c.CritChance;
      c.CritDmg = c.CritDmg;
      c.Health = c.Health;
      c.Gold = c.Gold;
      c.Exp = c.Exp;
      c.Lvl = c.Lvl;

      if (nameVaild && classValid) return true;

      return false;
    }

    /// <summary>
    /// Return the character
    /// </summary>
    /// <param name="characterId">Id of loading Character</param>
    /// <param name="characters">Array of all Characters</param>
    /// <returns></returns>
    private static Character LoadCharacter(byte characterId, Character[] characters) {
      return characters[characterId];
    }

    /// <summary>
    /// Returns Classname
    /// </summary>
    /// <returns></returns>
    public string GetClassName() {
      string cl = "";
      if (Class == 1) cl = "Krieger";
      else if (Class == 2) cl = "Magier";
      else cl = "Schurke";

      return cl;
    }

    public void ShowCharacter() {
      string cl = GetClassName();

      do {
        Console.Clear();
        Console.WriteLine("Name:\t\t\t{0}", Name);
        Console.WriteLine("Klasse:\t\t\t{0}", cl);
        Console.WriteLine("Level:\t\t\t{0}", Lvl);
        Console.WriteLine("Exp:\t\t\t{0} / {1}", Exp[0], Exp[1]);
        Console.WriteLine("Leben:\t\t\t{0} / {1}", Health[0], Health[1]);
        Console.WriteLine("Gold:\t\t\t{0}", Gold);
        Console.WriteLine("Stärke:\t\t\t{0}", Strength);
        Console.WriteLine("Inteligents:\t\t{0}", Intelligents);
        Console.WriteLine("Geschwindigkeit:\t{0}", Dexterity);
        Console.WriteLine("Krit. Chance:\t\t{0} %", CritChance);
        Console.WriteLine("Krit. Schaden:\t\t{0} %", (CritDmg - 1.0F) * 100);
        Console.WriteLine("\nDrücken Sie <Enter> um zurückzukehren...");
      } while (Console.ReadKey(false).Key != ConsoleKey.Enter);
    }

    /// <summary>
    /// de- / increases max HP
    /// </summary>
    /// <param name="health">value with which max HP is increased or not</param>
    public void ChangeMaximumHealth(short health) {
      Health[1] += health;
    }

    /// <summary>
    /// de- / increases HP
    /// </summary>
    /// <param name="health">value with which HP is increased or not</param>
    /// <param name="overheal">allows to heal over max HP</param>
    public void ChangeCurrentHealth(short health, bool overheal = false) {
      Health[0] += health;
      if (Health[0] > Health[1] && !overheal) Health[0] = Health[1];
    }

    /// <summary>
    /// Sets Characters current HP to max
    /// </summary>
    public void FullHeal() {
      ChangeCurrentHealth(Health[1], false);
    }

    /// <summary>
    /// de- / increases amout of Gold
    /// </summary>
    /// <param name="gold">value with which Gold is increased or not</param>
    public void ChangeAmoutOfGold(int gold) {
      Gold += gold;
    }

    /// <summary>
    /// Increases Level and Exp, which is needed for next lvl<br />
    /// Sets current Exp back to 0
    /// </summary>
    public void IncreaseLvl() {
      // if lvl 100 is reached, no more leveling
      if (Lvl >= 100) Exp[1] = 0;
      else if (Exp[0] >= Exp[1]) {
        Console.WriteLine("{0} ist ein Level aufgestiegen.\n{0} ist nun Level {1}.", Name, ++Lvl);
        Console.ReadKey(true);
        Exp[0] = 0;
        Exp[1] += (byte)(20 + Lvl);

        if (Lvl % 10 == 0) Exp[1] += 50;    // increases exp need every 10 lvls a bit more

        IncreaseStats();
      }
    }

    /// <summary>
    /// Increases all stats by one (exept for class stat - increased by 2)<br />
    /// Heal the Character to max HP
    /// </summary>
    private void IncreaseStats() {
      Strength++;
      Intelligents++;
      Dexterity++;
      ChangeMaximumHealth(2);
      FullHeal();
      if (Lvl % 10 == 0) {
        CritChance += 0.2F;
        CritDmg += 0.5F;
      }

      switch (Class) {
        case 1: Strength++; break;
        case 2: Intelligents++; break;
        case 3: Dexterity++; break;
      }
    }
  }
}
