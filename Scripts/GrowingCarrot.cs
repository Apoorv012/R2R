using Godot;

public partial class GrowingCarrot : Control {
  [Export] Sprite2D Carrot;

  public void SetGrow(float grow) {
    Carrot.Position = new(21, 37 * grow + (1 - grow) * 94);
  }
}
