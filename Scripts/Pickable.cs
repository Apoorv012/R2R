using Godot;

public partial class Pickable : Node2D {
  [Export] public PickableItem ItemType;
  public float GlobalX {
    get {
      return GlobalPosition.X;
    }
  }
  public float ItemSize {
    get {
      float size = ItemType switch {
        PickableItem.Coin => 53,
        PickableItem.Banknote => 104,
        PickableItem.Bottle => 104,
        PickableItem.Can => 64,
        PickableItem.Poop => 60,
        PickableItem.Paper => 128,
        PickableItem.Bone => 64,
        PickableItem.Carrot => 64,
        PickableItem.RotCarrot => 64,
        _ => 64
      };

      return size;
    }
  }

  
}

public enum PickableItem {
  Coin, Banknote, Bottle, Can, Poop, Paper, Bone, Carrot, RotCarrot, AllTrash, AllRecyclable
}