using System.Collections.Generic;

namespace R2R;

public class SaveData {
  public int currentRoad { get; set; }
  public float roadXPos { get; set; }
  public bool firstStart { get; set; }
  public Game.Status status { get; set; }
  public int dayNum { get; set; }
  public double dayTime { get; set; }
  public double waitUntil { get; set; }
  public double waitUntilNextDay { get; set; }
  public bool sleeping { get; set; }
  public double sleepStartTime { get; set; }
  public bool hospital { get; set; }
  public bool doingGym { get; set; }
  public double userGameSpeed { get; set; }
  public int gameDifficulty { get; set; }
  public bool moving { get; set; }
  public double moveDelta { get; set; }
  public double doActionDelta { get; set; }
  public double blinkDelta { get; set; }
  public bool enableLights { get; set; }
  public bool running { get; set; }
  public bool pickup { get; set; }
  public bool denial { get; set; }
  public bool goingUp { get; set; }
  public bool restingOnBench { get; set; }
  public bool sleepingOnBench { get; set; }
  public bool jail { get; set; }
  public double globalMessageTimeout { get; set; }
  public bool keepShiftPressedToRun { get; set; }
  public double food { get; set; }
  public double drink { get; set; }
  public double rest { get; set; }
  public double bodySmell { get; set; }
  public double fitness { get; set; }
  public int clothes { get; set; }
  public double dirtyClothes { get; set; }
  public double totalSmell { get; set; }
  public double beard { get; set; }
  public double education { get; set; }
  public int money { get; set; }
  public int investedMoney { get; set; }
  public bool hasATM { get; set; }
  public bool hasBag { get; set; }
  public int hasRazor { get; set; }
  public int hasSoap { get; set; }
  public bool hasBroom { get; set; }
  public double broomLevel { get; set; }
  public bool isSweeping { get; set; }
  public int numCoins { get; set; }
  public int numBanknotes { get; set; }
  public int numBottles { get; set; }
  public int numCans { get; set; }
  public int numCansFull { get; set; }
  public int numFoodSnacks { get; set; }
  public int numBones { get; set; }
  public int numPaper { get; set; }
  public int numPoop { get; set; }
  public int numCarrots { get; set; }
  public int numRotCarrots { get; set; }
  public int numTickets { get; set; }
  public string textForBaloon { get; set; }
  public double deltaBaloon { get; set; }
  public bool findAround { get; set; }
  public int foundLocationRoad { get; set; }
  public int foundLocationIndex { get; set; }
  public int currentHotelRoad { get; set; }
  public int currentHotelIndex { get; set; }
  public double prevDayTime { get; set; }
  public float deathDelta { get; set; }
  public bool fading { get; set; }
  public int roadToGo { get; set; }
  public float roadXToGo { get; set; }
  public bool isDogPacified { get; set; }
  public double dogBark { get; set; }
  public double throwBone { get; set; }
  public bool marketSpawn { get; set; }
  public bool barSpawn { get; set; }
  public bool wasInsideCrossroad { get; set; }
  public bool playedMusic { get; set; }
  public int prevHour { get; set; }
  public int windowWidth { get; set; }
  public double lastSpawnMain { get; set; }
  public double lastSpawnSlum { get; set; }
  public double lastSpawnTop { get; set; }
  public double lastSpawnNorth { get; set; }
  public double lastSpawnSide { get; set; }
  public double lastSpawnSuburb { get; set; }
  public double spawnDelay { get; set; }
  public bool[] npcOrders { get; set; }
  public double enemyDelay { get; set; }
  public SaveEnemy enemy { get; set; }
  public bool removeEnemy { get; set; }
  public int drunkGuyBench { get; set; }

  public List<SaveItem> slumItems { get; set; }
  public List<SaveItem> mainItems { get; set; }
  public List<SaveItem> topItems { get; set; }
  public List<SaveItem> northItems { get; set; }
  public List<SaveItem> sideItems { get; set; }
  public List<SaveItem> suburbItems { get; set; }


  public List<SaveLocation> gardens { get; set; }
  public List<SaveLocation> apartments { get; set; }
  public List<SaveLocation> hotels { get; set; }

  public List<SaveNPC> npcs { get; set; }
}

[System.Serializable]
public class SaveItem {
  public int itemType {get; set; }
  public float posX {get; set; }
  public float posY {get; set; }
}

[System.Serializable]
public class SaveLocation {
  public SaveLocation() { }
  public SaveLocation(Location l) {
    locationType = (int)l.type;
    price = l.price;
    amount = l.amount;
    rentedDays = l.RentedDays;
    if (l.carrots != null && l.carrots.Count > 0) {
      carrots = new();
      foreach (var c in l.carrots) {
        carrots.Add(new() { x = c.Position.X, y = c.Position.Y });
      }
    }
    else carrots = null;
  }

  public int locationType {get; set; }
  public int price {get; set; }
  public int amount {get; set; }
  public int rentedDays { get; set; }
  public List<SaveGrowingCarrot> carrots {get; set; }
}

[System.Serializable]
public class SaveGrowingCarrot {
  public float x { get; set; }
  public float y { get; set; }
}
[System.Serializable]
public class SaveEnemy {
  public float X { get; set; }
  public float Y { get; set; }
  public bool goingLeft { get; set; }
  public float Speed { get; set; }
  public int type { get; set; }
  public bool completedAction { get; set; }
  public bool fleeing { get; set; }
  public double catchTime { get; set; }
  public bool wasInsideCrossroad { get; set; }
}
[System.Serializable]
public class SaveNPC {
  public float X { get; set; }
  public float Y { get; set; }
  public float speed { get; set; }
  public int npcPrefab { get; set; }
  public int minPos { get; set; }
  public int maxPos { get; set; }
  public int order { get; set; }
  public bool goingLeft { get; set; }
  public bool hasDog { get; set; }
  public int dogPooped { get; set; }
  public float dogX { get; set; }
  public float dogY { get; set; }
  public int happiness { get; set; }
  public int emotion { get; set; }
  public bool checkedPlayer { get; set; }
  public bool giveToPlayer { get; set; }
  public double giveToPlayerDelay { get; set; }
  public bool giveToPlayerStopMovement { get; set; }
  public bool female { get; set; }
  public int road { get; set; }
  public bool stopForDogPoop { get; set; }
  public string Hair { get; set; }
  public string Dress { get; set; }
  public string Skirt { get; set; }
  public string Tie { get; set; }
  public string Bag { get; set; }
  public string Eyes { get; set; }
  public string Hat { get; set; }
  public bool hasGlasses { get; set; }
  public string glasses { get; set; }
  public int hair { get; set; }
}