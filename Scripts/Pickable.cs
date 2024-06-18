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
        PickableItem.CanFull => 64,
        _ => 64
      };

      return size;
    }
  }

  
}

public enum PickableItem {
  Coin = 0, Banknote = 1, Bottle = 2, Can = 3, Poop = 4, Paper = 5, Bone = 6, Carrot = 7, RotCarrot = 8, AllTrash = 9, AllRecyclable = 10, CanFull = 11, FoodSnack = 12
}