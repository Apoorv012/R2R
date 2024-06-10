using Godot;

namespace R2R;

public partial class Road : Node2D {
  [Export] public new RoadName Name;
  [Export] public float MinX;
  [Export] public float MaxX;
  [Export] public float Y;

  [Export] public Node2D Doors;
  [Export] public Node2D BackItems;
  [Export] public Node2D PeopleLayer;
  [Export] public Node2D StreetLights;
  [Export] public Node2D FrontItems;
  [Export] public Node2D CrossroadsDef;

  public Vector2[] Crossroads;

  public override void _Ready() {
  int num = CrossroadsDef?.GetChildCount() ?? 0;
    Crossroads = new Vector2[num];
    for (int i = 0; i < num; i++) {
      Sprite2D crossroad = CrossroadsDef.GetChild(i) as Sprite2D;
      float x = crossroad.GlobalPosition.X;
      float s = crossroad.Texture.GetWidth() * crossroad.Scale.X * .5f;
      Crossroads[i].X = x - s;
      Crossroads[i].Y = x + s;
    }
  }

}
