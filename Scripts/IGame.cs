
using Godot;
using R2R;

public partial interface IGame {
  public bool Won { get; }
  public Node2D iPlayer { get; }
  public void ShowNPCBalloon(string text, Node2D npc);
  public void GiveToPlayer(int happiness);
  public void NPCGone(NPC npc);
  public void EnemyGone();
  public void AddDogPoop(float xPos);
  public AudioStream[] iFNo { get; }
  public AudioStream[] iMNo { get; }
  public AudioStream[] iFYes { get; }
  public AudioStream[] iMYes { get; }
  public AudioStream[] iFDisgust { get; }
  public AudioStream[] iMDisgust { get; }
  public AudioStream[] iFApproval { get; }
  public AudioStream[] iMApproval { get; }
  public bool iSleepingOnBench { get; }
  public bool IsTopBoulevard();
  public bool IsNorthRoad();

  public void JailTime(string msg);
  public void SpawnBottlesForDrunkGuy();
  public bool IsPlayerReacheable();

  public void RobPlayer(EnemyType type);
}
