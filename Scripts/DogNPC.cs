using Godot;

namespace R2R;

public partial class DogNPC : Sprite2D {
  NPC owner;
  bool goingLeft = false;
  Vector2 direction;
  RandomNumberGenerator rnd = new();
  PoopingStatus hasPooped = PoopingStatus.NotPooped;

  public void Init(NPC npc, int windowWidth) {
    owner = npc;
    owner.GetParent().AddChild(this);
    goingLeft = npc.goingLeft;
    direction = goingLeft ? Vector2.Left : Vector2.Right;
    GlobalPosition = new(goingLeft ? windowWidth - 6 : -30, npc.GlobalPosition.Y + 130);
    Scale = (goingLeft ? Vector2.Left : Vector2.Right) + Vector2.Down;
    ZIndex = 60 + npc.order;
  }

  Vector2 walkOffset = Vector2.Zero;
  Vector2 targetOffset = Vector2.Zero;
  Vector2 walkDir = Vector2.Zero;
  float speed = 1;

  double step = 0;
  double poopDelay = 0;
  bool wasInsideCrossroad = false;
  public void ProcessDog(double delta, float playerX, Vector2[] crossroads, float streetX) {
    if (owner.NPCSpeed == 0) return;

    if (hasPooped != PoopingStatus.Pooping) {
      GlobalPosition += (float)(owner.NPCSpeed * delta) * direction - walkOffset;
      if (walkOffset.DistanceSquaredTo(targetOffset) < 1) {
        targetOffset = new(rnd.RandfRange(-15f, 15f), rnd.RandfRange(-8f, 8f));
        speed = targetOffset.DistanceTo(walkOffset) * .5f;
        walkDir = (targetOffset - walkOffset).Normalized();
      }
      walkOffset += speed * (float)delta * walkDir;
      GlobalPosition += walkOffset;

      float dogX = GlobalPosition.X;
      bool inside = false;
      for (int i = 0; i < crossroads.Length; i++) {
        if (dogX > crossroads[i].X + streetX && dogX < crossroads[i].Y + streetX) {
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



      step += delta * 5.5;
      if (step >= 4.0) step -= 4.0;
      int frame = ((int)step % 4) switch {
        0 => 0,
        1 => 1,
        2 => 0,
        3 => 2,
        _ => 0,
      };
      if (Frame != frame) {
        Frame = frame;
      }
    }
    else {
      poopDelay -= delta;
      if (poopDelay < 0) {
        poopDelay = 0;
        hasPooped = PoopingStatus.HasPooped;
        owner.StopForDogPoop(false); // The generation of poop will be done in the Game called inside this function
      }
    }

    if (hasPooped == PoopingStatus.NotPooped && !owner.gameWon) {
      float pdist = Mathf.Abs(playerX - GlobalPosition.X);
      if (pdist > 250f && pdist < 450f) {
        poopDelay -= delta;
        if (poopDelay < 0) {
          poopDelay = .5;
          if (rnd.RandiRange(0, 3) == 0) {
            owner.StopForDogPoop(true);
            hasPooped = PoopingStatus.Pooping;
            Frame = 3;
            poopDelay = 1.5;
          }
        }
      }
    }
  }


  enum PoopingStatus {
    NotPooped, Pooping, HasPooped
  }
}
