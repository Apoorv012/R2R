using Godot;
using System.Collections.Generic;

namespace R2R;

public partial class Location : Sprite2D {
  public float PositionX { get => GlobalPosition.X; } // Position ?Should it be just calculated?
  float wdt = -1;
  public float Width { get {
	  if (wdt == -1) {
		if (Texture != null) wdt = Texture.GetSize().X * .5f * Scale.X;
		else wdt = 80;
		if (wdt < 55) wdt = 55;
	  }
	  return wdt;
	} } // Position ?Should it be just calculated?

  public string Description {
		get {
			if (!string.IsNullOrEmpty(Message)) return Message.Replace("\\n", "\n");

			string priceTag = "free";
			if (price != 0) priceTag = $"${price / 10}.{price % 10}0";
			string start = StartTime switch {
				0 => "12AM",
				1 => "1AM",
				2 => "2AM",
				3 => "3AM",
				4 => "4AM",
				5 => "5AM",
				6 => "6AM",
				7 => "7AM",
				8 => "8AM",
				9 => "9AM",
				10 => "10AM",
				11 => "11AM",
				12 => "12PM",
				13 => "1PM",
				14 => "2PM",
				15 => "3PM",
				16 => "4PM",
				17 => "5PM",
				18 => "6PM",
				19 => "7PM",
				20 => "8PM",
				21 => "9PM",
				22 => "10PM",
				23 => "11PM",
				_ => "",
			};
			string end = EndTime switch {
				0 => "12AM",
				1 => "1AM",
				2 => "2AM",
				3 => "3AM",
				4 => "4AM",
				5 => "5AM",
				6 => "6AM",
				7 => "7AM",
				8 => "8AM",
				9 => "9AM",
				10 => "10AM",
				11 => "11AM",
				12 => "12PM",
				13 => "1PM",
				14 => "2PM",
				15 => "3PM",
				16 => "4PM",
				17 => "5PM",
				18 => "6PM",
				19 => "7PM",
				20 => "8PM",
				21 => "9PM",
				22 => "10PM",
				23 => "11PM",
				_ => "",
			};
			switch (type) {
				case LocationType.ATM:
					return "It is an ATM to deposit money";

				case LocationType.Bank:
					return $"{Name} Bank. I can get an ATM card here for $10\n Opens at {start} and closes at {end}";

				case LocationType.Shop:
					string what = ItemDelivered switch {
						ItemDelivered.Food => "Food for",
						ItemDelivered.Drink => "Drinks for",
						ItemDelivered.Toilet => "",
						ItemDelivered.Razor => "I can buy a razor here for",
						ItemDelivered.Clothes => "I can buy clothes here for ",
						ItemDelivered.Suits => "",
						ItemDelivered.Shaving => "I can shave here for",
						ItemDelivered.Tickets => "I can buy metro tickets here, ",
						ItemDelivered.Broom => "I can buy a broom here for",
						ItemDelivered.FoodAndDrink => "Food and drinks for",
						ItemDelivered.Laundry => "I can wash my clothes here for",
						_ => "",
					};

					if (StartTime == 0 && EndTime == 0)
						return $"{Name}\nAlways open\n{what} {priceTag}";
					else
						return $"{Name}\nOpens at {start} and closes at {end}\n{what} {priceTag}";


				case LocationType.Trashcan:
					return "A trashcan, I can throw away all garbage here";

				case LocationType.Garden:
					if (amount == 0) return "I think I can use this area to plant vegetables...";
					if (price < 100) return $"There are {amount} carrots planted and growing ({price}% grown)";
					return $"There are {amount} carrots ready to harvest";

				case LocationType.Hotel:
					return $"{Name}\nI can sleep here from {start} to {end} for {priceTag}";

				case LocationType.Apartment:
					if (RentedDays > 0) return $"I rented a place here, I can use it for another {RentedDays} day{(RentedDays == 1 ? "" : "s")}.";
					return $"I can rent this place for {priceTag} and stay {amount} day{(amount == 1 ? "" : "s")}";

				case LocationType.Job:
					int numh = EndTime - StartTime;
					if (numh < 0) numh = 24 - numh;
					return $"I can work here for {numh} hours and gain {priceTag}.";

				case LocationType.Dump:
					break;
				case LocationType.Sign:
					return Message.Replace("\\n", "\n"); // Fix newlines, they will be stored not as encoded char but as string representation
				case LocationType.Recycler:
					if (StartTime == 0 && EndTime == 0)
						return $"{Name}\nI can sell {ItemConsumed} here for {priceTag}, any time";
					else
						return $"{Name}\nI can sell {ItemConsumed} here for {priceTag} between {start} and {end}";

				case LocationType.Bench:
					return $"A bench where I can rest";

				case LocationType.Crossroad:
					return $"I can go to {Road.ToString().Replace('_', ' ')} from here.";

				case LocationType.Metro:
					return "This is the metro, I need a ticket to use it.";
				case LocationType.Map:
					return "A map showing where I am in the city.";
				case LocationType.School:
					if (amount == 0) return "An elementary school to study and learn.";
					else if (amount == 1) return "A middle school to study and learn.";
					else if (amount == 2) return "A high school to study and learn.";
					else return "A college to study and learn.";
				case LocationType.Gym:
					return $"It is a gym, it costs {priceTag}.";
				case LocationType.Fountain:
					return $"A fountain of fresh water, I can drink or wash myself Iif I have soap.";
			}


			return "locations...";

		}
	}

  [Export] public LocationType type; // type of location
  [Export] public int StartTime, EndTime; // opening hours
  [Export] public int price;  // Cost (multiplied by 10 to account for decimals) or money given (for selling or working)
  [Export] public ItemDelivered ItemDelivered; // Items delivered ?do we need it? I think we can just use the locationtype
  [Export] public PickableItem ItemConsumed; // What can be sold here
  [Export] public int amount; // Quantity delivered (for food and drinks, usually, but also the numebr of days to rent the apartment)
  // Requirements to enter/use
  [Export] public int maxSmell = 100; // Max smell possible to enter
  [Export] public int maxBeard = 100; // Max beard possible to enter
  [Export] public Clothes minClothes = Clothes.Rags; // Min level of clothes required to enter
  [Export] public int minEducation = 0; // Min level of education required to enter
  [Export] public string Message; // A message for signs
  [Export] public RoadName Road; // Destination road
  [Export] public Vector2 Pos; // Destination road position (and YouAreHere location for Map symbols)
  public int RentedDays = 0;
  public List<GrowingCarrot> carrots = new();
}

public enum LocationType {
  ATM = 0, // Exchange cash if you have a card
  Bank = 1, // Used to get a card
  Shop = 2, // Food, Drink, Tickets, Razor, Clothes, Shaving, 
  Trashcan = 3, // A trashcan to throw away stuff
  Garden = 4, // Place to grow food
  Hotel = 5, // Place to sleep one night
  Apartment = 6, // Place to rent for a week
  Job = 7, // Place where you can work
  Dump = 8, // Place where you can find bones or other trash to recycle
  Recycler = 9, // Place where you can sell your scraps
  Sign = 10, // Used just to read some text
  Bench = 11, // Used to rest
  Crossroad = 12, // Used to go to other streets
  Metro = 13, // Special item to go around
  Map = 14, // Special type of sign that shows a map when used
  School = 15, // Used to study (use Amount as level of school: 0 elementary, 1 middle, 2 high, 3 college
  Gym = 16, // Used to improve the fitness level
	Fountain = 17, // Used to drink or to wash yourself in case you have soap
}

public enum RoadName { None=-1, Main_Street=0, Slum_Street=1, Side_Road=2, North_Road=3, Top_Boulevard=4, Suburb_Avenue=5, Help_Road=6 };


public enum ItemDelivered {
  None,
  Food, Drink, Toilet, Razor, Clothes, Suits, Shaving, Tickets, Broom, FoodAndDrink, Laundry, Finance, Soap
}
