using Godot;

namespace R2R;

public partial class NPC : Sprite2D {
  // Spawn on the left or right of the screen, a little bit more far away than the camera
  // Face the direction
  // Register itself in the game, so we can know how many there are (we try to keep max one right now, in future max 2)
  // Start walking in the specified direction, at a conveninet speed
  // Once the NPC goes off screen (little bit more than off screen) in the opposite direction kill itself and unregister from the game
  [Export] Sprite2D Hair;
  [Export] Sprite2D Dress;
  [Export] Sprite2D Skirt;
  [Export] Sprite2D Tie;
  [Export] Sprite2D Bag;
  [Export] Sprite2D Eyes;
  [Export] Sprite2D Hat;
  [Export] Sprite2D Mouth;

  [Export] Sprite2D GlassesIn;
  [Export] Sprite2D GlassesOut;

  [Export] public float NPCSpeed = 20;
  [Export] Texture2D Hair1;
  [Export] Texture2D Hair2;
  [Export] Texture2D Hair3;
  [Export] Texture2D Hair4;

  [Export] Gradient HairGradient;
  [Export] Gradient DressGradient;
  [Export] Gradient TopGradient;
  [Export] Gradient TieGradient;
  [Export] Gradient SkirtGradient;
  [Export] Gradient BagGradient;
  [Export] Gradient EyesGradient;
  [Export] Gradient HatGradient;
  [Export] Gradient GlassesInGradient;

  [Export] PackedScene DogPrefab;

  [Export] AudioStreamPlayer2D SoundPlayer;


  RandomNumberGenerator rnd = new();
  public int order;
  public bool goingLeft = false;
  Vector2 direction;
  IGame game = null;
  DogNPC dog = null;
  int happiness = 1; // <2 sad ; 2..3 normal; >3 happy.    Alter it when the player does the expected actions
  bool checkedPlayer = false;
  bool giveToPlayer = false;
  double giveToPlayerDelay = 1;
  bool giveToPlayerStopMovement = false;
  bool female = false;
  public bool gameWon = false;
  RoadName road;
  int minPos, maxPos;

  string[] disgustSentences = { "Disgusting", "Ew!", "Bad smell!", "Go take a shower!", "Wash yourself!", "Don't play with poop!" };
  string[] approvalSentences = { "Thanks!", "Thank you!", "Appreciated!", "Well done!", "A small gift for you!" };


  public void Spawn(IGame game, Road road, int order, bool isFemale) {
    this.game = game;
    this.order = order;
    ZIndex = 60 + order * 2;
    if (rnd.Randf() < .5f) goingLeft = true;
    female = isFemale;
    gameWon = game.Won;
    this.road = road.Name;

    if (GetParent() != road.PeopleLayer) road.PeopleLayer.AddChild(this);
    // 960player 2064 npc  on right
    // 960player -144 npc  on left

    minPos = 960 - game.WindowWidth / 2 - 155;
    maxPos = 960 + game.WindowWidth / 2 + 155;

    GlobalPosition = new(goingLeft ? maxPos : minPos, 450 + order * 8);
    Scale = (goingLeft ? Vector2.Left : Vector2.Right) + Vector2.Down;
    direction = goingLeft ? Vector2.Left : Vector2.Right;
    NPCSpeed = rnd.RandfRange(120f, 200f);

    if (Hair != null) Hair.Modulate = EnsureNoAlpha(HairGradient.Sample(rnd.Randf()));
    if (Dress != null) Dress.Modulate = EnsureNoAlpha(DressGradient.Sample(rnd.Randf()));
    if (Skirt != null) Skirt.Modulate = EnsureNoAlpha(SkirtGradient.Sample(rnd.Randf()));
    if (Tie != null) Tie.Modulate = EnsureNoAlpha(TieGradient.Sample(rnd.Randf()));
    if (Bag != null) Bag.Modulate = BagGradient.Sample(rnd.Randf());
    if (Eyes != null) Eyes.Modulate = EnsureNoAlpha(EyesGradient.Sample(rnd.Randf()));
    if (Hat != null) Hat.Modulate = FlattenAlpha(HatGradient.Sample(rnd.Randf()));

    var glasses = rnd.Randf() > .6;
    if (GlassesIn != null) {
      GlassesIn.Visible = glasses;
      GlassesIn.Modulate = GlassesInGradient.Sample(rnd.Randf());
    }
    if (GlassesOut != null) {
      GlassesOut.Visible = glasses;
    }

    if (Hair1 != null && Hair2 != null && Hair3 != null && Hair4 != null) {
      Hair.Texture = rnd.RandiRange(0, 3) switch {
        0 => Hair1,
        1 => Hair2,
        2 => Hair3,
        _ => Hair4,
      };
    }

    if (DogPrefab != null && rnd.RandiRange(0, 5) == 0) {
      dog = DogPrefab.Instantiate() as DogNPC;
      dog.Init(this);
    }

    happiness = rnd.RandiRange(1, 3);
  }

  private static Color FlattenAlpha(Color color) {
    if (color.A < .5f) color.A = 0;
    else color.A = 1;
    return color;
  }

  private static Color EnsureNoAlpha(Color color) {
    color.A = 1;
    return color;
  }

  double step = 0;
  bool wasInsideCrossroad = false;
  public void ProcessNpc(double delta, float playerX, double playerSmell, Vector2[] crossroads, float streetX) {
    if (NPCSpeed == 0) return;
    if (stopForDogPoop) {
      dog?.ProcessDog(delta, playerX, crossroads, streetX);
      return;
    }

    if (!giveToPlayerStopMovement) {
      GlobalPosition += (float)(NPCSpeed * delta) * direction;
    }

    bool won = game.Won;
    float npcX = GlobalPosition.X;
    bool inside = false;
    for (int i = 0; i < crossroads.Length; i++) {
      if (npcX > crossroads[i].X + streetX && npcX < crossroads[i].Y + streetX) {
        inside = true;
        break;
      }
    }
    if (inside && !wasInsideCrossroad) {
      Position += Vector2.Down * 30;
      wasInsideCrossroad = true;
    }
    else if (!inside && wasInsideCrossroad) {
      Position -= Vector2.Down * 30;
      wasInsideCrossroad = false;
    }

    if (!giveToPlayerStopMovement) {
      step += delta * 3.5;
      if (step >= 4.0) step -= 4.0;
      int frame = (int)step % 4;
      if (Frame != frame) {
        Frame = frame;
        if (Hair != null) Hair.Frame = frame;
        if (Dress != null) Dress.Frame = frame;
        if (Skirt != null) Skirt.Frame = frame;
        if (Tie != null) Tie.Frame = frame;
        if (Bag != null) Bag.Frame = frame;
        if (Eyes != null) Eyes.Frame = frame;
        if (Hat != null) Hat.Frame = frame;
        if (GlassesIn != null) GlassesIn.Frame = frame;
        if (GlassesOut != null) GlassesOut.Frame = frame;
        if (Mouth != null) Mouth.Frame = frame + (int)emotion * 4;
      }
    }
    else {
      Frame = 4;
      if (Hair != null) Hair.Frame = 0;
      if (Dress != null) {
        if (Dress.Hframes == 5) Dress.Frame = 4;
        else Dress.Frame = 0;
      }
      if (Skirt != null) {
        if (Skirt.Hframes == 5) Skirt.Frame = 4;
        else Skirt.Frame = 0;
      }
      if (Tie != null) Tie.Frame = 0;
      if (Bag != null) Bag.Frame = 0;
      if (Eyes != null) Eyes.Frame = 0;
      if (Hat != null) Hat.Frame = 0;
      if (GlassesIn != null) GlassesIn.Frame = 0;
      if (GlassesOut != null) GlassesOut.Frame = 0;
      if (Mouth != null) Mouth.Frame = (int)emotion * 4;
    }


    if (giveToPlayer && game.iPlayer.Visible && !won) {
      if ((goingLeft && playerX - GlobalPosition.X > -200) ||
          (!goingLeft && playerX - GlobalPosition.X < 200)) {
        if (!giveToPlayerStopMovement) {
          PlaySound(Sound.Enjoy);
          giveToPlayerStopMovement = true;
          game.ShowNPCBalloon(approvalSentences[rnd.RandiRange(0, approvalSentences.Length - 1)], this);
        }
        // We are close to the player, stop and give some coins depending on happiness level
        giveToPlayerDelay -= delta;
        if (giveToPlayerDelay < 0) {
          game.GiveToPlayer(happiness);
          giveToPlayer = false;
          giveToPlayerStopMovement = false;
        }
        checkedPlayer = true;
      }
    }

    if (won) {
      gameWon = true;
      if (!checkedPlayer && ((goingLeft && playerX - GlobalPosition.X > -400) || (!goingLeft && playerX - GlobalPosition.X < 400))) {
        // We are close to the player, in case the player smells too much we will reduce happiness
        happiness = 5;
        SetEmotion();
        PlaySound(Sound.Approval);
        game.ShowNPCBalloon(approvalSentences[rnd.RandiRange(0, disgustSentences.Length - 1)], this);
        checkedPlayer = true;
      }
    }
    else if (!checkedPlayer && game.iPlayer.Visible) {
      if ((goingLeft && playerX - GlobalPosition.X > -300) ||
        (!goingLeft && playerX - GlobalPosition.X < 300)) {
        // We are close to the player, in case the player smells too much we will reduce happiness
        if (playerSmell > 60) {
          happiness -= 2;
          SetEmotion();
          if (emotion == Emotion.Sad) {
            PlaySound(Sound.Disgust);
            game.ShowNPCBalloon(disgustSentences[rnd.RandiRange(0, disgustSentences.Length - 1)], this);
          }
        }
        checkedPlayer = true;
      }
    }

    dog?.ProcessDog(delta, playerX, crossroads, streetX);

    if (goingLeft && GlobalPosition.X < minPos) {
      game.NPCGone(this);
      NPCSpeed = 0;
    }
    else if (!goingLeft && GlobalPosition.X > maxPos) {
      game.NPCGone(this);
      NPCSpeed = 0;
    }
  }

  public enum Emotion { Normal=0, Happy=1, Sad=2 };
  Emotion emotion = Emotion.Normal;
  public void SetEmotion() {
    if (happiness < 2) emotion = Emotion.Sad;
    else if (happiness < 4) emotion = Emotion.Normal;
    else emotion = Emotion.Happy;
  }
  public bool SetHappiness(int value, bool positiveReaction, float playerX) {
    happiness += value;
    SetEmotion();
    switch (happiness) {
      case 0: PlaySound(Sound.Disapproval); break;
      case 1: break;
      default: PlaySound(Sound.Approval); break;
    }

    if (positiveReaction) {
      // Are we facing the player?
      if ((goingLeft && GlobalPosition.X > playerX + 300) || (!goingLeft && GlobalPosition.X < playerX - 300)) {
        // Go to the player and give coin or banknote depending on the happiness level
        giveToPlayer = true;
        goingLeft = (playerX < GlobalPosition.X);
        direction = goingLeft ? Vector2.Left : Vector2.Right;
        Scale = (goingLeft ? Vector2.Left : Vector2.Right) + Vector2.Down;
        return true;
      }
    }
    return false;
  }

  enum Sound { Approval, Disapproval, Disgust, Enjoy };

  void PlaySound(Sound type) {
    AudioStream[] sounds;
    AudioStream sound;
    switch (type) {
      case Sound.Approval:
        sounds = female ? game.iFYes : game.iMYes;
        break;
      case Sound.Disapproval:
        sounds = female ? game.iFNo : game.iMNo;
        break;
      case Sound.Disgust:
        sounds = female ? game.iFDisgust : game.iMDisgust;
        break;
      case Sound.Enjoy:
        sounds = female ? game.iFApproval : game.iMApproval;
        break;
      default: return;
    }
    sound = sounds[rnd.RandiRange(0, sounds.Length - 1)];

    SoundPlayer.PitchScale = rnd.RandfRange(.95f, 1.05f);
    SoundPlayer.Stream = sound;
    SoundPlayer.Play();
  }

  public void Delete() {
    if (dog != null) {
      dog.Free();
      dog = null;
    }
    Free();
  }

  internal void RemoveDog() {
    if (dog == null) return;
    dog.Free();
    dog = null;
  }

  bool stopForDogPoop = false;
  internal void StopForDogPoop(bool doStop) {
    if (doStop) {
      stopForDogPoop = true;
      Frame = 4;
      if (Hair != null) Hair.Frame = 0;
      if (Dress != null) {
        if (Dress.Hframes == 5) Dress.Frame = 4;
        else Dress.Frame = 0;
      }
      if (Skirt != null) {
        if (Skirt.Hframes == 5) Skirt.Frame = 4;
        else Skirt.Frame = 0;
      }
      if (Tie != null) Tie.Frame = 0;
      if (Bag != null) Bag.Frame = 0;
      if (Eyes != null) Eyes.Frame = 0;
      if (Hat != null) Hat.Frame = 0;
      if (GlassesIn != null) GlassesIn.Frame = 0;
      if (GlassesOut != null) GlassesOut.Frame = 0;
      if (Mouth != null) Mouth.Frame = (int)emotion * 4;
    }
    else {
      stopForDogPoop = false;
      game.AddDogPoop(dog.GlobalPosition.X + (goingLeft ? 20 : -20));
    }
  }

  public SaveNPC Save() {
    return new() {
      X = Position.X,
      Y = Position.Y,
      minPos = minPos,
      maxPos = maxPos,
      order = order,
      goingLeft = goingLeft,
      hasDog = dog != null,
      dogPooped = (int)(dog?.hasPooped ?? 0),
      dogX = dog?.Position.X ?? 0,
      dogY = dog?.Position.Y ?? 0,
      happiness = happiness,
      emotion = (int)emotion,
      checkedPlayer = checkedPlayer,
      giveToPlayer = giveToPlayer,
      giveToPlayerDelay = giveToPlayerDelay,
      giveToPlayerStopMovement = giveToPlayerStopMovement,
      female = female,
      road = (int)road,
      stopForDogPoop = stopForDogPoop
    };
  }
}
