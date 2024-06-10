using Godot;
using System.Collections.Generic;

namespace R2R;

public partial class Help : Node {
	#region Properties *****************************************************************************
	[ExportGroup("Player")]
	[Export] public Node2D Player;
	[Export] Sprite2D Body;
	[Export] Sprite2D Face;
	[Export] Node2D Eye, EyesSitClosed;
	[Export] Sprite2D Hat;
	[Export] Sprite2D Beard;
	[Export] Sprite2D Legs;
	[Export] NinePatchRect Balloon;
	[Export] Label BalloonTxt;
	[Export] Sprite2D BroomPlayer;
	[Export] Sprite2D HandBrooming;
  [Export] Texture2D Body1, Body2;
  [Export] Texture2D Hat1, Hat2;

  [ExportGroup("Params")]
	[Export] double moveSpeed;
	[Export] float scrollSpeed;
	[Export] float timeSpeed = 20;


	[ExportGroup("UI")]
	[Export] Label DayNum, DayTime, TotalFunds;
	[Export] Label GlobalMessage;
	[Export] ProgressBar pbFood, pbDrink, pbRest, pbSmell, pbBeard, pbEducation, pbFitness;
	[Export] HBoxContainer ItemsHBox;
	[Export] TextureRect ATMCard;
	[Export] TextureRect[] Tickets;
	[Export] TextureRect Soap;
	[Export] TextureRect Razor;
	[Export] TextureRect Broom;

	[ExportGroup("Metro")]
	[Export] Panel PanelMetro;
	[Export] Button MetroMain;
	[Export] Button MetroSlum;
	[Export] Button MetroSide;
	[Export] Button MetroNorth;
	[Export] Button MetroTop;
	[Export] Button MetroSuburb;
	[Export] Button MetroClose;
	[Export] Theme SelectedButtonTheme;

	[ExportGroup("Environment")]
	[Export] Node2D City;
	[Export] Sprite2D BackgroundStreet;
	[Export] DirectionalLight2D Sun;
	[Export] Sprite2D SkyDay, SkyNight;
	[Export] Gradient SkyGradient, StarsGradient;
	[Export] Sprite2D Background0;
	[Export] Node2D Background1;
	[Export] Sprite2D Background1a, Background1b;
	[Export] Texture2D Mountains, Hills, Skyscrapers, Buildings;
	[Export] TextureRect CityMap;
	[Export] TextureRect YouAreHere;

	[ExportGroup("Items")]
	[Export] TextureRect[] Coins;
	[Export] TextureRect[] Banknotes;
	[Export] Texture2D[] ItemTexts; // 0 Bottle, 1 Can, 2 bone, 3 paper, 4 poop
	[Export] PackedScene[] ItemPrefabs;

	const int PrefabPoop = 0;
	const int PrefabPaper = 1;
	const int PrefabCan = 2;
	const int PrefabBottle = 3;
	const int PrefabCoin = 4;
	const int PrefabBanknote = 5;
	const int PrefabBone = 6;
	const int PrefabCarrot = 7;
	const int PrefabRotCarrot = 8;

	[ExportGroup("City")]
	[Export] Node2D currentRoad;
	[Export] Node2D FrontItems;
	[Export] Node2D BackItems;
	[Export] Node2D Doors;
	[Export] Node2D StreetLights;
	[Export] Node2D PeopleLayer;

	[ExportGroup("NPCs")]
	[Export] PackedScene NPCTemplateGirl1, NPCTemplateGirl2, NPCTemplateGirl3;
	[Export] PackedScene NPCTemplateMan1, NPCTemplateMan2, NPCTemplateMan3, NPCTemplateMan4;
	[Export] PackedScene NPCPolice, NPCRobber, NPCMAGA, NPCZTurd, NPCOrban;
	[Export] NinePatchRect NPCBalloon1, NPCBalloon2;
	[Export] Label NPCBalloonTxt1, NPCBalloonTxt2;

	[ExportGroup("Sounds")]
	[Export] AudioStreamPlayer MusicPlayer, SoundPlayer, PlayerPlayer, NPCPlayer;
	[Export] AudioStream MusicC64;
	[Export] AudioStream StepSound1, StepSound2, PickupSound, NoSound, SuccessSound, FailureSound;
	[Export] AudioStream FartSound, DogBarkSound, EatSound, DrinkSound, EatDrinkSound, ShowerSound;
	[Export] AudioStream LaundrySound, BroomSound, CashSound, SleepSound, StudySound;
	[Export] AudioStream Work1Sound, Work2Sound, Work3Sound, WorkoutSound, WashSound;
	[Export] public AudioStream[] FNo, MNo, FYes, MYes, FDisgust, MDisgust, FApproval, MApproval;

	[ExportGroup("Debug")]
	//	[Export] public Label Debug; // FIXME remove

	readonly List<NPC> NPCs = new();

	#endregion Properties *****************************************************************************


	#region Variables **************************************************************

	public enum Status { Intro, Starting, Playing, Dying, GameOver, Help, Win };

	public Status status = Status.Help;
	RandomNumberGenerator rnd = new();
	int dayNum = 1;
	double dayTime = 20 / 24.0; // 0 .. 1
	double waitUntil = -1;
	double waitUntilNextDay = -1;
	bool sleeping = false;
	double sleepStartTime = 0;
	bool hospital = false;
	bool doingGym = false;
	double userGameSpeed = 1;
	double gameSpeed = 5;
	int gameDifficulty = 5;
	bool moving = false;
	double moveDelta = 0;
	Vector2 flipL = new(1, 1);
	Vector2 flipR = new(-1, 1);
	Vector2 moveL = new(1, 0);
	Vector2 moveR = new(-1, 0);
	double doActionDelta = 0;
	double blinkDelta = 0;
	bool enableLights = true;
	bool running = false;
	bool pickup = false;
	bool denial = false;
	bool goingUp = false;
	bool restingOnBench = false;
	public bool sleepingOnBench = false;
	bool jail = false;
	double globalMessageTimeout = 0;

	double food = 100;
	double drink = 100;
	double rest = 100;
	double bodySmell = 25;
	double fitness = 0;
	public Clothes clothes = Clothes.Rags;
	double dirtyClothes = 25;
	public double totalSmell = 0;
	public double beard = 60;
	double education = 0; // 0-24 elem; 25-49 mid; 50-74 high; 75-100 college
	int money = 0; // banknotes * 1 + coins, each coin is 1/10 of a banknote
	int investedMoney = 0;
	bool hasATM = true;
	bool hasRazor = false;
	int hasSoap = 0;
	bool hasBroom = false;
	double broomLevel = 0;
	bool isSweeping = false;
	Pickable broomItem = null;
	int numCoins = 0;
	int numBanknotes = 0;
	int numBottles = 0;
	int numCans = 0;
	int numBones = 0;
	int numPaper = 0;
	int numPoop = 0;
	int numCarrots = 0;
	int numRotCarrots = 0;
	int numTickets = 1;
	const int sizeBottle = 104;
	const int sizeCans = 64;
	const int sizeBone = 64;
	const int sizePaper = 128;
	const int sizePoop = 60;
	readonly List<Location> locations = new();
	readonly List<Location> apartments = new();
	readonly List<Location> gardens = new();

	string textForBaloon = null;
	double deltaBaloon = 0;
	bool findAround = false;
	Location foundLocation = null;
	Location currentHotel = null;
	double prevDayTime = -1;
	float deathDelta = 0;
	RoadName roadToGo = RoadName.None;
	float roadXToGo = 0;

	bool isDogPacified = false;
	double dogBark = 0;
	double throwBone = 0;
	bool marketSpawn = false;
	bool barSpawn = false;
	double npcBalloonDelay1 = 0, npcBalloonDelay2 = 0;
	Node2D npcForBalloon1 = null, npcForBalloon2 = null;
	Vector2 NPCBalloonOffset = new(-310, -420);

	int helpStage = 0;

	enum BeardLevels {
		Walk = 0, Pickup = 1, DenialL = 3, DenialR = 2, Sit = 4
	};

	ConfigFile config = new();

	#endregion Variables **************************************************************

	public override void _Ready() {
		foreach (var item in Tickets) item.Visible = false;
		foreach (var item in Coins) item.Visible = false;
		foreach (var item in Banknotes) item.Visible = false;
		ATMCard.Visible = false;
		Balloon.Visible = false;
		NPCBalloon1.Visible = false;
		NPCBalloon2.Visible = false;
		BalloonTxt.Text = "";
		textForBaloon = null;
		BackgroundStreet.Visible = true;
    Body.Texture = Body1;
    Hat.Texture = Hat1;

    for (int i = 0; i < npcOrders.Length; i++) npcOrders[i] = false;

		ResetAllValues();


    doActionDelta = .5f;
    GlobalMessage.Text = "";
    // Cleanup spawned items
    while (FrontItems.GetChildCount() > 0) {
      var node = FrontItems.GetChild(rnd.RandiRange(0, FrontItems.GetChildCount() - 1));
      node.Free();
    }
    HelpSign.Visible = false;
    maxHelpXPos = 300;
    minHelpXPos = -300;
    restingOnBench = true;
    Player.Position = new(960, 406);
    Eye.Visible = false;
    EyesSitClosed.Visible = false;
    Body.Frame = 3 + GetFitnessLevel();
    Face.Frame = 5;
    Legs.Frame = 8 + 9 * (int)clothes;
    Hat.Frame = 3;
    Beard.Frame = Beardlevel(BeardLevels.Sit);
    helpStage = 0;
    Background0.Position = new(962, 306 - City.Position.Y);
    Background1.Position = new(974, 306 - City.Position.Y);


    ResetHelp(10, -1700, 2700); // FIMXE
		ResetPlayer();
		SpawnItems();
  }


	private void ResetAllValues() {
		dayNum = 1;
		dayTime = 8 / 24.0; // 0 .. 1
		waitUntil = -1;
		sleeping = false;
		hospital = false;
		doingGym = false;
		moving = false;
		moveDelta = 0;
		doActionDelta = 0;
		blinkDelta = 0;
		enableLights = false;
		running = false;
		pickup = false;
		denial = false;
		goingUp = false;
		restingOnBench = false;
		sleepingOnBench = false;
		jail = false;
		food = 100;
		drink = 100;
		rest = 100;
		bodySmell = 10;
		fitness = 0;
		pbFitness.Value = 0;

		dirtyClothes = 5;
		beard = 60;
		education = 0;
		pbEducation.Value = 0;
		money = 500;
		investedMoney = 0;
		hasATM = true; // FIXME
		hasRazor = false;
    hasSoap = 0;
    hasBroom = false;
		broomLevel = 0;
    isSweeping = false;
		broomItem = null;
		numCoins = 0;
		numBanknotes = 0;
		numBottles = 0;
		numCans = 0;
		numBones = 0;
		numPaper = 0;
		numPoop = 0;
		numCarrots = 0;
		numTickets = 0;

		deltaBaloon = 0;
		textForBaloon = null;
		Balloon.Visible = false;
		NPCBalloon1.Visible = false;
		NPCBalloon2.Visible = false;
		npcBalloonDelay1 = 0;
		npcBalloonDelay2 = 0;
		findAround = false;
		foundLocation = null;
		prevDayTime = -1;
		deathDelta = 0;
		roadToGo = RoadName.None;
		roadXToGo = 0;

		isDogPacified = false;
		dogBark = 0;
		throwBone = 0;
		marketSpawn = false;
		barSpawn = false;

		currentHotel = null;

    Soap.Visible = false;
		Razor.Visible = false;
    Broom.Visible = false;
		BroomPlayer.Visible = false;
		HandBrooming.Visible = false;
		ATMCard.Visible = false;

		joyJustPressed = false;
		playedMusic = false;
		lastSpawnMain = -1;
		lastSpawnSlum = -1;
		lastSpawnTop = -1;
		lastSpawnNorth = -1;
		lastSpawnSide = -1;
		lastSpawnSuburb = -1;

		foreach (var a in apartments) a.RentedDays = 0;
		apartments.Clear();
		foreach (var g in gardens) {
			g.amount = 0;
			foreach (var c in g.carrots) g.Free();
			g.carrots.Clear();
		}
		gardens.Clear();

		foreach (var npc in NPCs) npc.Delete();
		NPCs.Clear();
		if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
		enemyDelay = 10;
		enemyType = EnemyType.None;
		removeEnemy = false;

		RemoveAllItems();
		ArrangeWallet(0);
		ResetPlayer();
		SetRoad();
	}

  int winWidth = 0, winHeight;
  void SwitchFullscreen() {
    if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed) {
      var wsize = DisplayServer.WindowGetSize(0);
      winWidth = wsize.X; winHeight = wsize.Y;
    }

    DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

    if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed && winWidth != 0) {
      DisplayServer.WindowSetSize(new(winWidth, winHeight), 0);
    }
  }



  bool joyJustPressed = false;

	public override void _Process(double delta) {
		bool joyStart = Input.IsJoyButtonPressed(0, JoyButton.Start);
		bool rb = Input.IsJoyButtonPressed(0, JoyButton.RightShoulder);
		bool lb = Input.IsJoyButtonPressed(0, JoyButton.LeftShoulder);
		bool joyBack = Input.IsJoyButtonPressed(0, JoyButton.Back);


    if (fixtBallonSize) {
      fixtBallonSize = false;
      var lsize = BalloonTxt.Size;
      lsize.X += 36 * 2;
      lsize.Y += 36 * 2;
      Balloon.Size = lsize;
      lsize = NPCBalloonTxt1.Size;
      lsize.X += 36 * 2;
      lsize.Y += 36 * 2;
      NPCBalloon1.Size = lsize;
      lsize = NPCBalloonTxt2.Size;
      lsize.X += 36 * 2;
      lsize.Y += 36 * 2;
      NPCBalloon2.Size = lsize;
    }

    if (textForBaloon != null && deltaBaloon > 0) {
      if (BalloonTxt.Text.Length < textForBaloon.Length) {
        BalloonTxt.Text += textForBaloon[BalloonTxt.Text.Length];
      }
      else if (!fixtBallonSize) fixtBallonSize = true;
      deltaBaloon -= delta;
      if (deltaBaloon <= 0) {
        Balloon.Visible = false;
        BalloonTxt.Text = "";
        textForBaloon = null;
      }
    }

    HandleTime(delta);

    ProcessHelp(delta);


		if (Input.IsActionJustPressed("Esc") || (joyStart && !joyJustPressed)) {
      GetTree().ChangeSceneToFile("res://Game.tscn");
    }

		if (Input.IsActionJustPressed("F11") || (joyBack && !joyJustPressed)) {
			joyJustPressed = true;
			SwitchFullscreen();
		}

		return; // FIXME




//		SpawnNPC(d);
//		HandleEnemies(d);
//		ManageNPCs(d);


	}

	bool playedMusic = false;

	Color SunColor = Color.FromHtml("fcf2a7");

	int prevHour = -1;
	void HandleTime(double delta) {
		int prev = (int)(dayTime * 240);
		dayTime += delta * timeSpeed / 3600;
		if (dayTime > 1) {
			dayTime -= 1.0;
			fitness--;
			if (fitness < 0) fitness = 0;
			pbFitness.Value = fitness;
			int prevWeek = dayNum / 7;
			dayNum++;
			DayNum.Text = $"Day {dayNum}, week {dayNum / 7 + 1}";
			isDogPacified = false;
			currentHotel = null;
			if (prevWeek != dayNum / 7) {
				money += (int)(investedMoney * .1);
				TotalFunds.Text = FormatMoney(money);
			}
			foreach (var a in apartments) {
				if (a.RentedDays > 0) a.RentedDays--;
			}
			if (waitUntilNextDay != -1) {
				waitUntil = waitUntilNextDay;
				waitUntilNextDay = -1;
			}
		}
		int now = (int)(dayTime * 24 * 4);
		int h = now / 4;
		int m = (now % 4) * 15;
		if (prev != now) {
			if (h == 12) {
				DayTime.Text = $"{12}:{m:D2} PM ";
			}
			else if (h > 12) {
				DayTime.Text = $"{h - 12}:{m:D2} PM ";
			}
			else if (h == 0) {
				DayTime.Text = $"{12}:{m:D2} AM ";
			}
			else {
				DayTime.Text = $"{h}:{m:D2} AM ";
			}
		}
		if (h == 7 && m == 0 && !playedMusic) {
			MusicPlayer.Play();
			playedMusic = true;
		}
		if (playedMusic && h == 8 && m == 0) {
			playedMusic = false;
		}

		double dayHour = dayTime * 24;
		if (dayHour < 5 || dayHour > 22) {
			Sun.Energy = .95f;
			Sun.BlendMode = Light2D.BlendModeEnum.Sub;
			Sun.Color = Colors.White;
		}
		else if (dayHour >= 7 && dayHour < 19) {
			Sun.Energy = .05f;
			Sun.BlendMode = Light2D.BlendModeEnum.Add;
			Sun.Color = SunColor;
		}
		else if (dayHour >= 5 && dayHour < 6) { // Ramp up staing negative
			Sun.Energy = (float)(6 - dayHour) * .95f;
			Sun.BlendMode = Light2D.BlendModeEnum.Sub;
			Sun.Color = Colors.White;
		}
		else if (dayHour >= 6 && dayHour < 7) { // Ramp up going positive
			Sun.Energy = (float)(dayHour - 6) * .05f;
			Sun.BlendMode = Light2D.BlendModeEnum.Add;
			Sun.Color = SunColor;
		}
		else if (dayHour >= 19 && dayHour < 20) { // Go down staing positive
			Sun.Energy = (float)(20 - dayHour) * .05f;
			Sun.BlendMode = Light2D.BlendModeEnum.Add;
			Sun.Color = SunColor;
		}
		else if (dayHour >= 20 && dayHour < 22) { // Go down going negative
			Sun.Energy = (float)(dayHour - 20) * .95f * .5f;
			Sun.BlendMode = Light2D.BlendModeEnum.Sub;
			Sun.Color = Colors.White;
		}

		// Carrots grow every hour, require 100 hours to be ready (2 days + 4 hours)
		if (prevHour != h) {
			prevHour = h;
			foreach (var g in gardens) {
				if (g.amount > 0 && g.price < 100) {
					g.price++;
					// Show some plants growing
					foreach (var c in g.carrots) {
						c.SetGrow(g.price * .01f);
					}
				}
			}
		}

		// Lights
		if (now < 5 * 4 || now > 21 * 4) {
			if (!enableLights) {
				enableLights = true;
				foreach (var light in StreetLights.GetChildren()) {
					if (light.GetChild(0) is PointLight2D l)
						l.Enabled = true;
				}
			}
		}
		else if (enableLights) {
			enableLights = false;
			foreach (var light in StreetLights.GetChildren()) {
				if (light.GetChild(0) is PointLight2D l)
					l.Enabled = false;
			}
		}

		// Sky and Stars
		SkyDay.Modulate = SkyGradient.Sample((float)dayTime);
		// 6pm->8pm 4am-5am
		SkyNight.Modulate = StarsGradient.Sample((float)dayTime);

		// Blink
		if (pickup || denial || goingUp || sleepingOnBench || status != Status.Playing) return;
		blinkDelta -= delta;
		if (blinkDelta < 0 && !Eye.Visible && !EyesSitClosed.Visible) {
			if (Face.Frame == 5) EyesSitClosed.Visible = true;
			else Eye.Visible = true;
		}
		else if (blinkDelta < -.2) {
			if (Face.Frame == 5) EyesSitClosed.Visible = false;
			else Eye.Visible = false;
			blinkDelta = rnd.RandfRange(1.5f, 3.0f);
		}
	}


	public override void _UnhandledInput(InputEvent evt) {
		if (evt is not InputEventScreenTouch touch) return;

		//		Debug.Text = $"TOUCH: {touch.Position.X:F0}, {touch.Position.Y:F0}";
	}


	#region movement *****************************************************************************************************************************      [movement]

	bool wasUpPressed = false;
	bool wasDownPressed = false;
	bool wasUsePressed = false;
	bool wasLeftPressed = false;
	bool wasRightPressed = false;
	bool fitPosition = false;

	void HandleMovements(double delta) {
		bool up = false;
		bool down = false;
		bool left = false;
		bool right = false;
		bool use = false;
		bool running = false;
		if (doActionDelta <= 0 && waitUntil == -1 && waitUntilNextDay == -1) {
			float jx = Input.GetJoyAxis(0, JoyAxis.LeftX);
			float jy = Input.GetJoyAxis(0, JoyAxis.LeftY);

			if (Input.IsKeyPressed(Key.Up) || Input.IsPhysicalKeyPressed(Key.W) || Input.IsJoyButtonPressed(0, JoyButton.DpadUp) || jy <= -.5f) {
				if (!wasUpPressed) up = true;
				wasUpPressed = true;
			}
			else {
				wasUpPressed = false;
			}
			if (Input.IsKeyPressed(Key.Down) || Input.IsPhysicalKeyPressed(Key.S) || Input.IsJoyButtonPressed(0, JoyButton.DpadDown) || jy >= .5f) {
				if (!wasDownPressed) down = true;
				wasDownPressed = true;
			}
			else {
				wasDownPressed = false;
			}
			if (!CityMap.Visible && !sleepingOnBench) {
				left = (Input.IsKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.A) || Input.IsJoyButtonPressed(0, JoyButton.DpadLeft) || jx <= -.5f);
				right = (Input.IsKeyPressed(Key.Right) || Input.IsPhysicalKeyPressed(Key.D) || Input.IsJoyButtonPressed(0, JoyButton.DpadRight) || jx >= .5f) && !left;
			}

			if (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A)) {
				if (!wasUsePressed) use = true;
				wasUsePressed = true;
			}
			else {
				wasUsePressed = false;
			}
			running = (Input.IsKeyPressed(Key.Shift) || Input.IsJoyButtonPressed(0, JoyButton.B));
		}

		if (up || down) {
			left = false;
			right = false;
		}

		if (restingOnBench) {
			up = false;
			down = false;
			left = false;
			right = false;
		}


		if (left && Player.Scale.X < 0) {
			Player.Scale = flipL;
		}
		else if (right && Player.Scale.X > 0) {
			Player.Scale = flipR;
		}

		if (use && doingGym) {
			ResetPlayer();
			doingGym = false;
			foundLocation = null;
			return;
		}


		if (up || use) {
			if (CityMap.Visible) {
				CityMap.Visible = false;
				return;
			}

			// If we have the broom and there is a pickable just here, use the broom and remove it
			if (use && hasBroom && SearchItems(true, out Pickable p)) {
				isSweeping = true;
				doActionDelta = 1;
				Body.Frame = 4 + GetFitnessLevel();
				BroomPlayer.Visible = true;
				HandBrooming.Visible = true;
				broomItem = p;
				PlayerPlayer.Stream = BroomSound;
				PlayerPlayer.Play();
				return;
			}

			// We should check if there is any spot here we can use/go inside
			foundLocation = null;
			foreach (var loc in locations) {
				if (Mathf.Abs(loc.PositionX - 900) < loc.Width) {
					foundLocation = loc;
					break;
				}
			}
			if (foundLocation == null) {
				if (up) {
					// Say no
					ShowBalloon(rnd.RandiRange(0, 5) switch {
						0 => "Can't go there",
						1 => "Nowhere to go",
						2 => "Nope",
						3 => "Nothing there",
						4 => "Wrong way",
						_ => "No"
					}, 2);
				}
				else {
					ShowBalloon(rnd.RandiRange(0, 5) switch {
						0 => "Nothing to use here",
						1 => "There is nothing here",
						2 => "Nope",
						3 => "Nothing here",
						4 => "Can't use",
						_ => "No"
					}, 2);
				}
				return;
			}
		}


		if (up && foundLocation != null && foundLocation.type != LocationType.Sign) {
			// If possible walk up anim and then action
			goingUp = true;
			doActionDelta = 1;
      Player.Scale = flipL; 
			HideBalloon();
			return;
		}
		else if (left || right || fitPosition) {
			fitPosition = false;
			moving = true;
			double multiplier = (running ? 2.75 : 1.5) * (rest <= 0 ? .5 : 1) * (rest > 90 ? 1.1 : 1) * (1 + fitness * 0.01);
			moveDelta += delta * multiplier;
			if (moveDelta > moveSpeed) {
				moveDelta -= moveSpeed;
				int frame = Legs.Frame + 1 - 9 * (int)clothes;
				if (frame > 3) frame = 0;
				Legs.Frame = frame + 9 * (int)clothes;
				if (frame == 0) {
					PlayerPlayer.Stream = StepSound1;
					PlayerPlayer.Play();
				}
				else if (frame == 2) {
					PlayerPlayer.Stream = StepSound2;
					PlayerPlayer.Play();
				}
			}
			float movement = (float)(delta * scrollSpeed * multiplier);
			currentRoad.Position += (left ? moveL : moveR) * movement;

			// Check if we are inside a crossroad, in case change the vertical position
			float posX = currentRoad.Position.X;
			if (posX > maxHelpXPos) {
				posX = maxHelpXPos;
				currentRoad.Position = new(posX, currentRoad.Position.Y);
				moving = false;
				moveDelta = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
			}
			else if (posX < minHelpXPos) {
				posX = minHelpXPos;
				currentRoad.Position = new(posX, currentRoad.Position.Y);
				moving = false;
				moveDelta = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
			}

			/*
			 -11000   -100
			11000     2024

			11000 4048
			-11000-2096

			 */


			Background0.Position = new(0.09654545454545455f * posX + 962, 306 - City.Position.Y);
			Background1.Position = new(0.27945454545454546f * posX + 974, 306 - City.Position.Y);
		}
		else if (moving || down) {
			moving = false;
			moveDelta = 0;
			Legs.Frame = 0 + 9 * (int)clothes;
		}

		if (down) {
			pickup = true;
			doActionDelta = 1;
			Player.Scale = flipL;
			Body.Frame = 1 + GetFitnessLevel();
			Face.Frame = 1;
			Hat.Frame = 1;
			Legs.Frame = 4 + 9 * (int)clothes;
			Beard.Frame = Beardlevel(BeardLevels.Pickup);
			HideBalloon();
		}

		if (use || (foundLocation != null && foundLocation.type == LocationType.Sign)) {
			if (foundLocation.type == LocationType.Map) {
				ShowMap(foundLocation.Pos);
				foundLocation = null;
				return;
			}
			ShowBalloon(foundLocation.Description, 5);
			foundLocation = null;
			return;
		}

	}

	void SetRoad() {
		int posX = 0;
				City.Position = new(0, 0);
				Sun.Position = new(395, 162);
				Background0.Texture = Mountains;
				Background1a.Texture = Hills;
				Background1b.Texture = Hills;
        MusicPlayer.Stream = MusicC64;

    Background0.Position = new(0.0890909090909091f * posX + 980, 306 - City.Position.Y);
		if (posX > 0)
			Background1.Position = new(0.1781818181818182f * posX, 306 - City.Position.Y);
		else
			Background1.Position = new(0.1781818181818182f * posX + 1960, 306 - City.Position.Y);


		locations.Clear();
		foreach (var node in Doors.GetChildren()) {
			if (node is Location l) {
				locations.Add(l);
			}
		}
		foreach (var node in BackItems.GetChildren()) {
			if (node is Location l) {
				locations.Add(l);
			}
		}
		foreach (var node in FrontItems.GetChildren()) {
			if (node is Location l) locations.Add(l);
		}

		foreach (var npc in NPCs) npc.Delete();
		NPCs.Clear();
		if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
		npcForBalloon1 = null;
		npcForBalloon2 = null;
		Player.ZIndex = 75;
		fitPosition = true;
		enableLights = !enableLights;
	}

	void ShowMap(Vector2 pos) {
		CityMap.Visible = true;
		YouAreHere.Position = pos;
	}

	#endregion movement **************************************************************************  ^movement^

	#region stats ********************************************************************************************************************************      [Stats]

	void HandleStats(double delta) {
		double diff = dayTime - prevDayTime;
		if (prevDayTime > dayTime) {
			prevDayTime = 0;
			return;
		}
		prevDayTime = dayTime;
		if (diff >= dayTime) return;

		double nFood = food - diff * 75 * (running ? 1.5 : 1);
		if (nFood < 0) nFood = 0;
		double nDrink = drink - diff * 150 * (running ? 1.25 : 1);
		if (nDrink < 0) nDrink = 0;
		double restSpeed = dayTime < 6 / 24.0 || dayTime > 22 / 24.0 ? 85 : 60; // During the night the rest decreases faster
		double nRest = rest - diff * restSpeed * (running ? 2 : 1) * (nFood == 0 && nDrink == 0 ? 10 : 1);
		if (nRest < 0) nRest = 0;
		double nBodySmell = bodySmell + diff * 35 * (running ? 2 : 1);
		if (nBodySmell > 100) nBodySmell = 100;
		double nClothesSmell = dirtyClothes + diff * 15;
		if (dirtyClothes > 100) dirtyClothes = 100;
		double nSmell = nBodySmell + nClothesSmell;
		if (nSmell > 100) nSmell = 100;
		totalSmell = nSmell;
		double nBeard = beard + diff * 25;
		if (nBeard > 100) nBeard = 100;

		if (restingOnBench) {
			nRest += delta;
			if (nRest > 100) nRest = 100;
		}
		if (sleepingOnBench && doActionDelta <= 0) {
			nRest += 5 * delta;
			if (nRest > 100) nRest = 100;
		}

		food = nFood;
		pbFood.SetValueNoSignal(food);
		drink = nDrink;
		pbDrink.SetValueNoSignal(drink);
		rest = nRest;
		pbRest.SetValueNoSignal(rest);
		pbSmell.SetValueNoSignal(nSmell);
		bodySmell = nBodySmell;
		dirtyClothes = nClothesSmell;
		beard = nBeard;
		pbBeard.SetValueNoSignal(beard);


	}

	void HandleSleepStats(double delta) {
		double diff = dayTime - prevDayTime;
		if (prevDayTime > dayTime) {
			prevDayTime = 0;
			return;
		}
		prevDayTime = dayTime;
		if (diff >= dayTime) return;

		rest += delta;
		if (rest > 100) rest = 100;
		double nBeard = beard + diff * 25;
		if (nBeard > 100) nBeard = 100;
		pbRest.SetValueNoSignal(rest);
		beard = nBeard;
		pbBeard.SetValueNoSignal(beard);

		bool use = false;
		if (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A)) {
			if (!wasUsePressed) use = true;
			wasUsePressed = true;
		}
		else {
			wasUsePressed = false;
		}
		if (use) {
			double amount = ((dayTime > sleepStartTime) ? dayTime - sleepStartTime : sleepStartTime - dayTime) * 24;
			if (amount > 6) {
				CompleteJob();
			}
			else {
				ResetPlayer();
				SoundPlayer.Stop();
			}
		}
	}





	#endregion stats **************************************************************************   ^Stats^

	#region Inventory ****************************************************************************************************************************      [Inventory]

	private static string FormatMoney(int money) {
		if (money < 1000) return $"${money / 10}.{money % 10}0";
		if (money < 1000000) return $"${money / 10000},{(money / 10) % 1000}.{money % 10}0";
		return $"${money / 10000000},{(money / 10000) % 1000},{(money / 10) % 1000}.{money % 10}0";
	}

	private void RemoveMoney(int amount) {
		while (amount > 0) {
			if (numCoins > 0) {
				if (numCoins <= amount) {
					amount -= numCoins;
					numCoins = 0;
				}
				else {
					numCoins -= amount;
					amount = 0;
				}
			}
			else if (numBanknotes > 0) {
				if (numBanknotes * 10 <= amount) {
					amount -= numBanknotes * 10;
					numBanknotes = 0;
				}
				else if (amount > 0 && amount < 10 && numBanknotes > 0) {
					numBanknotes--;
					numCoins += 10 - amount;
					amount = 0;
				}
				else {
					amount -= 10;
					numBanknotes--;
				}
			}
			if (numCoins == 0 && numBanknotes == 0 && hasATM) {
				money -= amount;
				amount = 0;
			}
		}
		ArrangeWallet(0);
	}

	private bool HasMoney(int amount) {
		int total = numBanknotes * 10 + numCoins;
		if (hasATM) total += money;
		return total >= amount;
	}

	private bool HasMoneyInBank(int amount) {
		return numBanknotes * 10 + numCoins + money >= amount;
	}

	void InventoryRemoveItem(PickableItem itemType, bool onlyOne = false) {
		List<Node> toDelete = new();
		foreach (var item in ItemsHBox.GetChildren()) {
			if (item is TypedTextureRect ttr && ttr.ItemType == itemType) {
				toDelete.Add(item);
				if (onlyOne) break;
			}
		}
		foreach (var node in toDelete) {
			node.Free();
		}
	}
	void RemoveAllItems() {
		List<Node> toDelete = new();
		foreach (var item in ItemsHBox.GetChildren()) {
			if (item is TypedTextureRect _) {
				toDelete.Add(item);
			}
		}
		foreach (var node in toDelete) {
			node.Free();
		}
		numBottles = 0;
		numCans = 0;
		numBones = 0;
		numPaper = 0;
		numPoop = 0;
		numCarrots = 0;
		numRotCarrots = 0;
	}

	int ArrangeWallet(int earnedMoney) {
		// Max is 10 banknotes and 19 coins
		numCoins += earnedMoney;
		while (numBanknotes < 10 && numCoins > 10) {
			numBanknotes++;
			numCoins -= 10;
		}
		if (numCoins > 19) {
			if (hasATM) {
				money += numCoins;
				numCoins = 0;
			}
			else {
				earnedMoney = numCoins - 19;
				numCoins = 19;
				GlobalMessage.Text = $"Some money is lost, get an ATM card!";
				globalMessageTimeout = 5;
			}
		}
		for (int i = 0; i < Coins.Length; i++) {
			Coins[i].Visible = i < numCoins;
		}
		for (int i = 0; i < Banknotes.Length; i++) {
			Banknotes[i].Visible = i < numBanknotes;
		}
		TotalFunds.Text = FormatMoney(money);
		return earnedMoney;
	}

	int CalculateInvSize() {
		return
			numBottles * sizeBottle +
			numCans * sizeCans +
			numBones * sizeBone +
			numPaper * sizePaper +
			numPoop * sizePoop +
			4 * (numBottles + numCans + numBones + numPaper + numPoop);
	}

	double lastSpawnMain = -1;
	double lastSpawnSlum = -1;
	double lastSpawnTop = -1;
	double lastSpawnNorth = -1;
	double lastSpawnSide = -1;
	double lastSpawnSuburb = -1;
	void SpawnItems() {
		// Cleanup previous items
		while (FrontItems.GetChildCount() > 2) {
			var node = FrontItems.GetChild(rnd.RandiRange(0, FrontItems.GetChildCount() - 1));
			node.Free();
		}

		int[] prefabs = {
      PrefabPoop, PrefabCan, PrefabBottle, PrefabBone, PrefabCarrot, PrefabCarrot, PrefabRotCarrot,
      PrefabBanknote, PrefabCoin,PrefabCoin,PrefabCoin,PrefabCoin,
      PrefabBottle,PrefabBottle,PrefabBottle,PrefabBottle,
      PrefabCan,PrefabCan,PrefabCan,PrefabCan,PrefabCan,
      PrefabPaper,PrefabPaper,PrefabPaper,PrefabPaper,PrefabPaper,PrefabPaper,PrefabPaper,PrefabPaper,
      };

		for (int i = 0; i < 1000; i++) {
			int a = rnd.RandiRange(0, prefabs.Length - 1);
			int b = rnd.RandiRange(0, prefabs.Length - 1);
			int tmp = prefabs[a];
			prefabs[a] = prefabs[b];
			prefabs[b] = tmp;
    }

		float pos = minHelpXPos + 900;
		// Go from one end to the other end, the size will depend on the street
		int index = 0;
		while (pos < maxHelpXPos + 800) {
			// Spawn an object using the defined probabilities
			// poop 9%, paper 15%, cans 20%, bottles 25%, coins 25%, banknotes 5%, bones 1%
			Pickable item;
			item = ItemPrefabs[prefabs[index++]].Instantiate() as Pickable;
			FrontItems.AddChild(item);
			item.Position = new(pos, rnd.RandfRange(728, 738));
			if (index >= prefabs.Length) index = 0;

			// Step a random long amount of distance
			var minDist = 150f;
			var maxDist = 300f;
			pos += rnd.RandfRange(minDist, maxDist);
		}

	}

	#endregion Inventory **************************************************************************     ^Inventory^

	#region actions ******************************************************************************************************************************      [Actions]

	void ProcessActions(double delta) {
		if (denial && doActionDelta > 0) {
			Eye.Visible = false;
			EyesSitClosed.Visible = false;
			doActionDelta -= delta * 2;
			int pos = (int)(doActionDelta * 4 + 1) % 4;
			switch (pos) {
				case 0: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup); break; // Front
				case 1: Face.Frame = 3; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialR); break; // Right
				case 2: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup); break; // Front
				case 3: Face.Frame = 4; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialL); break; // Left
			}
			if (doActionDelta <= 0) {
				denial = false;
				pickup = false;
				findAround = false;
				doActionDelta = 0;
				Body.Frame = 0 + GetFitnessLevel();
				Face.Frame = 0;
				Hat.Frame = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
				Beard.Frame = Beardlevel(BeardLevels.Walk);
			}
			return;
		}

		if (pickup && doActionDelta > 0) {
			Eye.Visible = false;
			EyesSitClosed.Visible = false;
			doActionDelta -= delta;
			if (doActionDelta <= .5 && !findAround) {
				findAround = true;
				// Check all items around, if they are pickable and have a global X position close to the player position
				bool foundOne = SearchItems(false, out Pickable p);
				if (!foundOne) {
					pickup = false;
					doActionDelta = 0;
				}
				else {
					if (Pickup(p.ItemType)) p.Free();
				}
			}
			if (doActionDelta <= 0) {
				pickup = false;
				findAround = false;
				doActionDelta = 0;
				Body.Frame = 0 + GetFitnessLevel();
				Face.Frame = 0;
				Hat.Frame = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
				Beard.Frame = Beardlevel(BeardLevels.Walk);
			}
		}

		if (goingUp && doActionDelta > 0) {
      Eye.Visible = false;
			EyesSitClosed.Visible = false;
			doActionDelta -= delta;
			Body.Frame = 2 + GetFitnessLevel();
			Face.Frame = 2;
			Beard.Visible = false;
			Hat.Frame = 2;
			int pos = (int)(doActionDelta * 9) % 4;
			switch (pos) {
				case 0: Legs.Frame = 6 + 9 * (int)clothes; break;
				case 1: Legs.Frame = 5 + 9 * (int)clothes; break;
				case 2: Legs.Frame = 6 + 9 * (int)clothes; break;
				case 3: Legs.Frame = 7 + 9 * (int)clothes; break;
			}
			Player.Position = new(960, 490 - 45 * (1 - (float)doActionDelta));
			Player.ZIndex = (int)(71 - 25 * (1 - (float)doActionDelta));

			if (foundLocation.type == LocationType.Bench || foundLocation.type == LocationType.Crossroad) { // try to go to the center
				if (foundLocation.GlobalPosition.X < Player.GlobalPosition.X - 20) currentRoad.Position += moveL * 2;
				if (foundLocation.GlobalPosition.X > Player.GlobalPosition.X + 20) currentRoad.Position += moveR * 2;
			}
			if (doActionDelta <= 0) {
				goingUp = false;
				findAround = false;
				doActionDelta = 0;
				Body.Frame = 0 + GetFitnessLevel();
				Face.Frame = 0;
				Hat.Frame = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
				Player.Position = new(960, 490);
				Beard.Visible = true;
				Beard.Frame = Beardlevel(BeardLevels.Walk);
				// Call the action here
				if (PerformActionAtLocation()) ResetPlayer();
			}
		}

		if (restingOnBench && (dayTime > 23 / 24.0 || dayTime < 5 / 24.0) && !sleepingOnBench) {
			sleepingOnBench = true;
			Player.ZIndex = 59;
			doActionDelta = 1;
		}

		if (sleepingOnBench && doActionDelta > 0) {
			doActionDelta -= 3 * delta;
			restingOnBench = false;
			Player.Rotation = (1 - (float)doActionDelta) * Mathf.Pi * .5f;
			Eye.Visible = true;
			Body.Frame = 0 + GetFitnessLevel();
			Face.Frame = 0;
			Legs.Frame = 3 + 9 * (int)clothes;
			Beard.Frame = Beardlevel(BeardLevels.Walk);
			Player.Position = new(960, 490 + (1 - (float)doActionDelta) * 5);
			Player.Scale = new(1 - (1 - (float)doActionDelta) * .4f, 1);
		}

		if ((sleepingOnBench || restingOnBench) && (Input.IsActionJustPressed("Use") || Input.IsActionJustPressed("Down"))) {
			ResetPlayer();
		}

		if (isSweeping && doActionDelta > 0) {
			broomLevel -= delta;
			doActionDelta -= delta;
			BroomPlayer.Rotation = Mathf.Sin((float)doActionDelta * 9.45f) * .2f;
			if (doActionDelta <= 0) {
				if (broomItem != null) {
					if (broomItem.ItemType == PickableItem.Coin || broomItem.ItemType == PickableItem.Banknote) Pickup(broomItem.ItemType);
					broomItem.Free();
				}
				// Have NPCs to be happy around
				foreach (var npc in NPCs) {
					int val = broomItem.ItemType switch {
						PickableItem.Coin => 0,
						PickableItem.Banknote => 0,
						PickableItem.Bottle => 1,
						PickableItem.Can => 1,
						PickableItem.Poop => 3,
						PickableItem.Paper => 1,
						PickableItem.Bone => 2,
						PickableItem.Carrot => 1,
						PickableItem.RotCarrot => 2,
						_ => 0
					};
					if (npc.SetHappiness(val, true, Player.GlobalPosition.X)) break;
				}
				broomItem = null;
				isSweeping = false;
				doActionDelta = 0;
				ResetPlayer();
				if (broomLevel <= 0) {
					hasBroom = false;
					Broom.Visible = false;
					ShowBalloon("The broom is gone.\nWas used too much and collapsed.", 2);
				}
			}
			return;
		}


		if (doingGym) {
			fitness += delta * .025 * timeSpeed;
			if (fitness > 100) fitness = 100;
			pbFitness.Value = fitness;
			rest -= delta * .02 * timeSpeed;
			if (rest < 0) rest = 0;
			bodySmell += delta * .01 * timeSpeed;
			if (bodySmell > 100) bodySmell = 100;
			if (dayTime >= foundLocation?.EndTime / 24.0) {
				ResetPlayer();
				doingGym = false;
				foundLocation = null;
			}
		}

		if (doActionDelta <= 0) Beard.Frame = Beardlevel(BeardLevels.Walk);


		if (npcBalloonDelay1 > 0) {
			if (npcForBalloon1 == null) {
				npcBalloonDelay1 = 0;
				NPCBalloon1.Visible = false;
			}
			else {
				npcBalloonDelay1 -= delta;
				NPCBalloon1.Position = npcForBalloon1.GetGlobalTransformWithCanvas().Origin + NPCBalloonOffset;
				if (npcBalloonDelay1 <= 0) {
					NPCBalloon1.Visible = false;
				}
			}
		}
		if (npcBalloonDelay2 > 0) {
			if (npcForBalloon2 == null) {
				npcBalloonDelay2 = 0;
				NPCBalloon2.Visible = false;
			}
			else {
				npcBalloonDelay2 -= delta;
				NPCBalloon2.Position = npcForBalloon2.GetGlobalTransformWithCanvas().Origin + NPCBalloonOffset;
				if (npcBalloonDelay2 <= 0) {
					NPCBalloon2.Visible = false;
				}
			}
		}

	}


	private bool SearchItems(bool brooming, out Pickable pickable) {
		float playerHand = Player.GlobalPosition.X + (brooming ? -5 : -75);
		float extraSize = brooming ? 45 : 16;
		foreach (var item in FrontItems.GetChildren()) {
			if (item is Pickable p) {
				float pickCenter = p.GlobalX - 10;
				float pickSize = p.ItemSize;
				if (Mathf.Abs(pickCenter - playerHand) < pickSize / 2 + extraSize) {
					pickable = p;
					return true;
				}
			}
		}
		pickable = null;
		return false;
	}

	bool Pickup(PickableItem itemType) {
		Texture2D itemTexture = null;
		int objectSize = 0;
		switch (itemType) {
			case PickableItem.Coin:
				if (numCoins >= 19) {
					PlayDenial();
					return false;
				}
				Coins[numCoins].Visible = true;
				numCoins++;
				SoundPlayer.Stream = PickupSound;
				SoundPlayer.Play();
				return true;

			case PickableItem.Banknote:
				if (numBanknotes >= 10) {
					PlayDenial();
					return false;
				}
				Banknotes[numBanknotes].Visible = true;
				numBanknotes++;
				SoundPlayer.Stream = PickupSound;
				SoundPlayer.Play();
				return true;

			case PickableItem.Bottle:
				itemTexture = ItemTexts[0];
				objectSize = sizeBottle;
				break;
			case PickableItem.Can:
				itemTexture = ItemTexts[1];
				objectSize = sizeCans;
				break;
			case PickableItem.Poop:
				itemTexture = ItemTexts[4];
				objectSize = sizePoop;
				bodySmell += 10;
				if (bodySmell > 100) bodySmell = 100;
				break;
			case PickableItem.Paper:
				itemTexture = ItemTexts[3];
				objectSize = sizePaper;
				break;
			case PickableItem.Bone:
				itemTexture = ItemTexts[2];
				objectSize = sizeBone;
				break;
			case PickableItem.Carrot:
				itemTexture = ItemTexts[5];
				objectSize = sizeBone;
				break;
			case PickableItem.RotCarrot:
				itemTexture = ItemTexts[6];
				objectSize = sizeBone;
				break;
		}
		if (itemTexture != null) {
			int invSize = CalculateInvSize();
			if (invSize + objectSize > 1080) {
				PlayDenial();
				return false;
			}
			TypedTextureRect itemToAdd = new() {
				ItemType = itemType,
				Texture = itemTexture,
				ExpandMode = TextureRect.ExpandModeEnum.KeepSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
			};
			ItemsHBox.AddChild(itemToAdd);
			switch (itemType) {
				case PickableItem.Bottle:
					numBottles++;
					break;
				case PickableItem.Can:
					numCans++;
					break;
				case PickableItem.Poop:
					numPoop++;
					break;
				case PickableItem.Paper:
					numPaper++;
					break;
				case PickableItem.Bone:
					numBones++;
					break;
				case PickableItem.Carrot:
					numCarrots++;
					break;
				case PickableItem.RotCarrot:
					numRotCarrots++;
					break;
			}
			SoundPlayer.Stream = PickupSound;
			SoundPlayer.Play();
			return true;
		}
		else {
			GlobalMessage.Text = $"Unknown item type: {itemType}";
			globalMessageTimeout = 3;
			return false;
		}
	}

	void PlayDenial() {
		// Setup an anim to move the head left and right
		denial = true;
		doActionDelta = 3;
		SoundPlayer.Stream = NoSound;
		SoundPlayer.Play();
	}

	bool PerformActionAtLocation() {
		if (foundLocation == null) return false;
		var l = foundLocation;
		foundLocation = null;
		// Check smell, beard, education, time
		if (beard > l.maxBeard) {
			ShowBalloon("I should shave to go there...", 2);
			SoundPlayer.Stream = NoSound;
			SoundPlayer.Play();
			return true;
		}
		double totalSmell = Mathf.Clamp(bodySmell + dirtyClothes, 0, 100);
		if (totalSmell > l.maxSmell) {
			ShowBalloon("I am too stinky to go there...", 2);
			SoundPlayer.Stream = NoSound;
			SoundPlayer.Play();
			return true;
		}
		if (education < l.minEducation) {
			ShowBalloon("I need to have a better education to go there...", 2);
			SoundPlayer.Stream = NoSound;
			SoundPlayer.Play();
			return true;
		}
		if (clothes < l.minClothes) {
			ShowBalloon("I need to be better dressed to go there...", 2);
			SoundPlayer.Stream = NoSound;
			SoundPlayer.Play();
			return true;
		}
		if (l.StartTime != 0 && l.EndTime != 0) {
			if (l.StartTime < l.EndTime) {
				if (dayTime < l.StartTime / 24.0 || dayTime > l.EndTime / 24.0) {
					if (l.type != LocationType.Job && l.type != LocationType.School) {
						ShowBalloon("It is closed.", 2);
						return true;
					}
				}
			}
			else {
				if (dayTime > l.EndTime / 24.0 && dayTime < l.StartTime / 24.0) {
					if (l.type != LocationType.Job && l.type != LocationType.School) {
						ShowBalloon("It is closed.", 2);
						return true;
					}
				}
			}
		}
		int earnedMoney = 0;
		bool resetPlayer = true;
		ResultSound success = ResultSound.None;
		switch (l.type) {
			case LocationType.ATM: {
					if (hasATM) {
						money += numBanknotes * 10 + numCoins;
						numBanknotes = 0;
						numCoins = 0;
						foreach (var i in Coins) i.Visible = false;
						foreach (var i in Banknotes) i.Visible = false;
						TotalFunds.Text = FormatMoney(money);
						StopEnemies();
					}
					else {
						ShowBalloon("I need an ATM card...", 2);
						success = ResultSound.None;
					}
				}
				break;
			case LocationType.Bank: {
					if (hasATM) {
						money += numBanknotes * 10 + numCoins;
						numBanknotes = 0;
						numCoins = 0;
						foreach (var i in Coins) i.Visible = false;
						foreach (var i in Banknotes) i.Visible = false;
						TotalFunds.Text = FormatMoney(money);
						success = ResultSound.ATM;
						StopEnemies();
					}
					else if (HasMoneyInBank(10)) {
						hasATM = true;
						ATMCard.Visible = true;
						ShowBalloon("Got an ATM card", 4);
						RemoveMoney(10);
						success = ResultSound.GenericSuccess;
						StopEnemies();
					}
					else {
						ShowBalloon("I need $10 to get an ATM card", 4);
						success = ResultSound.Failure;
					}
				}
				break;
			case LocationType.Shop: {
					if (HasMoney(l.price)) {
						switch (l.ItemDelivered) {
							case ItemDelivered.Food:
								food += l.amount;
								if (food > 100) food = 100;
								ShowBalloon("Noutricious!", 1);
								RemoveMoney(l.price);
								success = ResultSound.Eat;
								StopEnemies();
								break;

							case ItemDelivered.Drink:
								drink += l.amount;
								if (drink > 100) drink = 100;
								ShowBalloon("Refreshing!", 1);
								RemoveMoney(l.price);
								success = ResultSound.Drink;
								StopEnemies();
								break;

							case ItemDelivered.Toilet:
								RemoveMoney(l.price);
								waitUntil = dayTime + .5 / 24.0;
								if (waitUntil >= 1) waitUntil -= 1;
								if (waitUntil < dayTime) {
									waitUntilNextDay = waitUntil;
									waitUntil = -1;
								}
								Player.Visible = false;
								success = ResultSound.Toilet;
								resetPlayer = false;
								StopEnemies();
								break;

							case ItemDelivered.Razor:
								if (hasRazor) ShowBalloon("I already have a razor", 2);
								else {
									RemoveMoney(l.price);
									hasRazor = true;
									Razor.Visible = true;
									success = ResultSound.GenericSuccess;
									StopEnemies();
								}
								break;

							case ItemDelivered.Clothes:
								RemoveMoney(l.price);
								dirtyClothes = 0;
								ShowBalloon("That is an upgrade!", 2);
								success = ResultSound.GenericSuccess;
								StopEnemies();
								break;

							case ItemDelivered.Suits:
								if (clothes == Clothes.Classy) ShowBalloon("I already have these clothes!", 2);
								else {
									RemoveMoney(l.price);
									dirtyClothes = 0;
									ShowBalloon("I can own the world now!", 2);
									success = ResultSound.GenericSuccess;
									StopEnemies();
								}
								break;

							case ItemDelivered.Shaving:
								RemoveMoney(l.price);
								beard = 0;
								if (bodySmell > 30) bodySmell = 30;
								ShowBalloon("Nice and clean!", 2);
								success = ResultSound.GenericSuccess;
								StopEnemies();
								break;

							case ItemDelivered.Tickets:
								if (numTickets == 10) ShowBalloon("I already have many metro tickets!", 2);
								else {
									RemoveMoney(l.price);
									numTickets++;
									for (int i = 0; i < 10; i++) Tickets[i].Visible = i < numTickets;
									success = ResultSound.GenericSuccess;
									StopEnemies();
								}
								break;

							case ItemDelivered.Broom:
								if (hasBroom) ShowBalloon("I already have a broom", 2);
								else {
									RemoveMoney(l.price);
									hasBroom = true;
									broomLevel = 100;
									Broom.Visible = true;
									success = ResultSound.GenericSuccess;
									StopEnemies();
								}
								break;

							case ItemDelivered.FoodAndDrink:
								food += l.amount;
								drink += l.amount;
								if (food > 100) food = 100;
								if (drink > 100) drink = 100;
								ShowBalloon("Good meal!", 1);
								RemoveMoney(l.price);
								success = ResultSound.EatAndDrink;
								StopEnemies();
								break;

							case ItemDelivered.Laundry:
								RemoveMoney(l.price);
								waitUntil = dayTime + 1 / 24.0;
								if (waitUntil >= 1) waitUntil -= 1;
								if (waitUntil < dayTime) {
									waitUntilNextDay = waitUntil;
									waitUntil = -1;
								}
								Player.Visible = false;
								foundLocation = l;
								success = ResultSound.Laundry;
								resetPlayer = false;
								StopEnemies();
								break;

							case ItemDelivered.Finance:
								RemoveMoney(l.price);
								investedMoney += 100000;
								success = ResultSound.ATM;
								StopEnemies();
								break;

							case ItemDelivered.Soap:
                break;
						}

					}
					else {
						ShowBalloon($"I do not have {FormatMoney(l.price)}...", 2);
						success = ResultSound.Failure;
					}
				}
				break;

			case LocationType.Trashcan: {
					var all = ItemsHBox.GetChildren();
					if (all.Count == 0) ShowBalloon($"Nothing to throw away...", 2);
					else {
						bool wasPoop = false;
						foreach (var item in all) {
							if (item is TypedTextureRect ttr) {
								if (ttr.ItemType == PickableItem.Paper) numPaper--;
								if (ttr.ItemType == PickableItem.Bottle) numBottles--;
								if (ttr.ItemType == PickableItem.Bone) numBones--;
								if (ttr.ItemType == PickableItem.Carrot) numCarrots--;
								if (ttr.ItemType == PickableItem.RotCarrot) numRotCarrots--;
								if (ttr.ItemType == PickableItem.Can) numCans--;
								if (ttr.ItemType == PickableItem.Poop) { numPoop--; wasPoop = true; }
							}
							item.Free();
						}
						ShowBalloon($"All garbage is now in the trash", 2);
						success = ResultSound.GenericSuccess;
						// In case some poop was picked up and thrown away, and the player is in "poor mode" have a NPC to give a coin
						if (wasPoop) {
							foreach (var npc in NPCs) {
								if (npc.SetHappiness(2, true, Player.GlobalPosition.X)) break;
							}
						}
						StopEnemies();
					}
				}
				break;

			case LocationType.Garden:
				break;

			case LocationType.Hotel: {
					// Sleep if enough money, get washed, get shaved only if you have razor
					if (HasMoney(l.price) || currentHotel == l) {
						if (currentHotel != l) RemoveMoney(l.price);
						waitUntil = l.EndTime / 24.0;
						if (waitUntil < dayTime) {
							waitUntilNextDay = waitUntil;
							waitUntil = -1;
						}
						Player.Visible = false;
						foundLocation = l;
						currentHotel = l;
						sleeping = true;
						sleepStartTime = dayTime;
						resetPlayer = false;
						success = ResultSound.Sleep;
						StopEnemies();
					}
					else {
						ShowBalloon($"I do not have {FormatMoney(l.price)}...", 2);
						success = ResultSound.Failure;
					}
				}
				break;

			case LocationType.Apartment:
				Location aprt = null;
				foreach (var a in apartments) {
					if (l == a) {
						aprt = a; break;
					}
				}
				bool stayInApartment = false;
				if (aprt != null && aprt.RentedDays > 0) {
					// Already rented?
					stayInApartment = true;
				}
				else if (HasMoney(l.price)) {
					RemoveMoney(l.price);
					if (aprt == null) apartments.Add(l);
					l.RentedDays = l.amount;
					stayInApartment = true;
				}
				else {
					ShowBalloon($"I do not have {FormatMoney(l.price)} to rent the apartment for {l.RentedDays} days...", 2);
					success = ResultSound.Failure;
				}
				if (stayInApartment) {
					int time = Mathf.CeilToInt(dayTime * 24) + 1;
					if (time < 7) waitUntil = 7 / 24.0;
					else if (time < 12) waitUntil = 12 / 24.0;
					else if (time < 18) waitUntil = 18 / 24.0;
					else if (time < 18) waitUntil = 18 / 24.0;
					else waitUntil = 6 / 24.0;
					if (waitUntil < dayTime) {
						waitUntilNextDay = waitUntil;
						waitUntil = -1;
					}
					Player.Visible = false;
					foundLocation = l;
					sleeping = true;
					sleepStartTime = dayTime;
					resetPlayer = false;
					success = ResultSound.Sleep;
					StopEnemies();
				}
				break;

			case LocationType.Job:
				// Work for the specified amount of time, then get money
				// Check if we are in the very first hour of possible work, if not say it is too late
				if ((int)(dayTime * 24) != l.StartTime) {
					ShowBalloon($"I cannot start working here now.\nI should go here at {FormatTime(l.StartTime)}", 4);
					success = ResultSound.Failure;
					break;
				}
				waitUntil = l.EndTime / 24.0;
				if (waitUntil < dayTime) {
					waitUntilNextDay = waitUntil;
					waitUntil = -1;
				}
				Player.Visible = false;
				foundLocation = l;
				resetPlayer = false;
				success = l.amount switch {
					0 => ResultSound.Work1,
					1 => ResultSound.Work2,
					2 => ResultSound.Work3,
					_ => ResultSound.Work1,
				};
				StopEnemies();
				break;

			case LocationType.Dump:
				break;

			case LocationType.Recycler: {
					// Sell scraps and get money
					switch (l.ItemConsumed) {
						case PickableItem.Bottle:
							earnedMoney += l.price * numBottles;
							numBottles = 0;
							InventoryRemoveItem(PickableItem.Bottle);
							earnedMoney = ArrangeWallet(earnedMoney);
							ShowMoneyPopup(earnedMoney);
							success = ResultSound.GenericSuccess;
							StopEnemies();
							break;

						case PickableItem.Can:
							earnedMoney += l.price * numCans;
							numCans = 0;
							InventoryRemoveItem(PickableItem.Can);
							earnedMoney = ArrangeWallet(earnedMoney);
							ShowMoneyPopup(earnedMoney);
							success = ResultSound.GenericSuccess;
							StopEnemies();
							break;

						case PickableItem.Paper:
							earnedMoney += l.price * numPaper;
							numPaper = 0;
							InventoryRemoveItem(PickableItem.Paper);
							earnedMoney = ArrangeWallet(earnedMoney);
							ShowMoneyPopup(earnedMoney);
							success = ResultSound.GenericSuccess;
							StopEnemies();
							break;

						case PickableItem.AllRecyclable:
							earnedMoney += l.price * (numBottles + numCans + numPaper + numCarrots + numBones);
							numBottles = 0;
							numCans = 0;
							numPaper = 0;
							numCarrots = 0;
							numBones = 0;
							InventoryRemoveItem(PickableItem.Bottle);
							InventoryRemoveItem(PickableItem.Can);
							InventoryRemoveItem(PickableItem.Paper);
							InventoryRemoveItem(PickableItem.Carrot);
							InventoryRemoveItem(PickableItem.Bone);
							earnedMoney = ArrangeWallet(earnedMoney);
							ShowMoneyPopup(earnedMoney);
							success = ResultSound.GenericSuccess;
							StopEnemies();
							break;

					}
				}
				break;

			case LocationType.Bench: {
					if (l.GetChildCount() > 0) {
						ShowBalloon("I cannot sit here now, the bench is already used", 2);
						resetPlayer = true;
					}
					else {
						restingOnBench = true;
						Player.Position = new(960, 406);
						Eye.Visible = false;
						EyesSitClosed.Visible = false;
						Body.Frame = 3 + GetFitnessLevel();
						Face.Frame = 5;
						Legs.Frame = 8 + 9 * (int)clothes;
						Hat.Frame = 3;
						Beard.Frame = Beardlevel(BeardLevels.Sit);
						resetPlayer = false;
					}
				}
				break;

			case LocationType.Crossroad: {
					JumpToRoad(l.Pos.X, l.Road, false);
					Player.Visible = false;
					resetPlayer = false;
					StopEnemies();
				}
				break;

			case LocationType.Metro: {
					if (numTickets > 0) {
						StopEnemies();
						// Show popup asking for road, then the selection should do the next action
						ShowMetroMenu();
					}
					else {
						ShowBalloon("I do not have any metro ticket!", 2);
					}
				}
				break;

			case LocationType.Map: {
					// Show map with current location
					ShowMap(l.Pos);
				}
				break;

			case LocationType.School: {
					// Study for the specified amount of time, then get money
					if (!HasMoney(l.price)) {
						ShowBalloon($"I do not have {FormatMoney(l.price)} to pay the school...", 2);
						success = ResultSound.Failure;
						break;
					}
					// Can we get more edication here?
					if ((l.amount == 0 && education > 24) ||
					(l.amount == 1 && education > 24) ||
					(l.amount == 2 && education > 24)) {
						success = ResultSound.Failure;
						break;
					}
					if (education >= 100) {
						ShowBalloon($"I already have a perfect education!", 2);
						break;
					}

					// Check if we are in the very first hour of possible work, if not say it is too late
					if ((int)(dayTime * 24) != l.StartTime) {
						if (l.StartTime < l.EndTime) {
							if (dayTime * 24 < l.StartTime) ShowBalloon($"Lessons are not yet started.\nI should be there at {FormatTime(l.StartTime)}", 4);
							else if (dayTime * 24 < l.EndTime) ShowBalloon($"Lessons are already started.\nI can try tomorrow at {FormatTime(l.StartTime)}", 4);
							else ShowBalloon($"Lessons are completed for today. I can try again tomorrow", 2);
						}
						else {
							if (dayTime * 24 < l.EndTime) ShowBalloon($"Lessons are already started.\nI can try tomorrow at {FormatTime(l.StartTime)}", 4);
							else if (dayTime * 24 < l.StartTime) ShowBalloon($"Lessons are not yet started.\nI should be there at {FormatTime(l.StartTime)}", 4);
							else ShowBalloon($"Lessons are completed for today. I can try again tomorrow", 2);
						}
						break;
					}
					waitUntil = l.EndTime / 24.0;
					if (waitUntil < dayTime) {
						waitUntilNextDay = waitUntil;
						waitUntil = -1;
					}
					Player.Visible = false;
					foundLocation = l;
					resetPlayer = false;
					success = ResultSound.Study;
					StopEnemies();
				}
				break;

			case LocationType.Gym:
				if (HasMoney(l.price)) {
					RemoveMoney(l.price);
					doingGym = true;
					Player.Visible = false;
					foundLocation = l;
					resetPlayer = false;
					success = ResultSound.Workout;
					StopEnemies();
				}
				else {
					ShowBalloon("I do not have enough money to enter.", 2);
					success = ResultSound.Failure;
				}
				break;


      case LocationType.Fountain:
					drink += l.amount;
					if (drink > 100) drink = 100;
					ShowBalloon("Refreshing!", 1);
					RemoveMoney(l.price);
					success = ResultSound.Drink;
					StopEnemies();
	      break;

    }

    switch (success) {
			case ResultSound.None:
				return resetPlayer;

			case ResultSound.GenericSuccess:
				SoundPlayer.Stream = SuccessSound;
				break;
			case ResultSound.Failure:
				SoundPlayer.Stream = FailureSound;
				break;
			case ResultSound.Eat:
				SoundPlayer.Stream = EatSound;
				break;
			case ResultSound.Drink:
				SoundPlayer.Stream = DrinkSound;
				break;
			case ResultSound.EatAndDrink:
				SoundPlayer.Stream = EatDrinkSound;
				break;
			case ResultSound.Toilet:
				SoundPlayer.Stream = ShowerSound;
				break;
			case ResultSound.Laundry:
				SoundPlayer.Stream = LaundrySound;
				break;
			case ResultSound.Broom:
				SoundPlayer.Stream = BroomSound;
				break;
			case ResultSound.ATM:
				SoundPlayer.Stream = CashSound;
				break;
			case ResultSound.Sleep:
				SoundPlayer.Stream = SleepSound;
				break;
			case ResultSound.Work1:
				SoundPlayer.Stream = Work1Sound;
				break;
			case ResultSound.Work2:
				SoundPlayer.Stream = Work2Sound;
				break;
			case ResultSound.Work3:
				SoundPlayer.Stream = Work3Sound;
				break;
			case ResultSound.Study:
				SoundPlayer.Stream = StudySound;
				break;
			case ResultSound.DogBark:
				SoundPlayer.Stream = DogBarkSound;
				break;
			case ResultSound.Workout:
				SoundPlayer.Stream = WorkoutSound;
				break;
			case ResultSound.Wash:
				SoundPlayer.Stream = WashSound;
				break;
		}
		SoundPlayer.Play();

		return resetPlayer;
	}

	private int GetFitnessLevel() {
		return (((int)(fitness / 34)) % 3) * 5;
	}


	public void CompleteJob() {
		if (foundLocation == null) {
			GlobalMessage.Text = $"Location is missing when ending the job!";
			globalMessageTimeout = 5;
		}

		if (foundLocation.type == LocationType.School) {
			education += 2;
			pbEducation.Value = education;
			ShowBalloon($"I have more knowledge now!", 2);
		}
		else if (foundLocation.type == LocationType.Hotel) {
			rest = 100;
			drink = 100;
			bodySmell = 0;
			if (hasRazor) beard = 0;
			hasRazor = false;
			Razor.Visible = false;
			SoundPlayer.Stream = SuccessSound;
			SoundPlayer.Play();
			ShowBalloon("I feel rested!", 1);
			sleeping = false;
		}
		else if (foundLocation.type == LocationType.Shop && foundLocation.ItemDelivered == ItemDelivered.Toilet) {
			bodySmell = 0;
			if (hasRazor) beard = 0;
			hasRazor = false;
			Razor.Visible = false;
			SoundPlayer.Stream = SuccessSound;
			SoundPlayer.Play();
			ShowBalloon("I am clean!", 1);
		}
		else if (foundLocation.type == LocationType.Shop && foundLocation.ItemDelivered == ItemDelivered.Laundry) {
			dirtyClothes = 0;
			ShowBalloon("My clothes are clean now!", 1);
		}
		else if (foundLocation.type == LocationType.Fountain) {
			bodySmell = 0;
			if (dirtyClothes > 50) ShowBalloon("I am fresh and clean, but I should wash also my clothes in a laundry.", 1);
			else ShowBalloon("I am fresh and clean!", 1);
    }
		else {
			ArrangeWallet(foundLocation.price);
			ShowBalloon($"I got $ {FormatMoney(foundLocation.price)}", 2);
		}
		Player.Visible = true;
		ResetPlayer();
		waitUntil = -1;
		foundLocation = null;
		SoundPlayer.Stop();
	}

	public bool IsPlayerReacheable() {
		if (status == Status.Win) return false;

		return (status == Status.Playing && !jail &&
			(foundLocation == null ||
			(foundLocation.type != LocationType.Bench && foundLocation.type != LocationType.Garden && waitUntil != -1 && waitUntilNextDay != -1 && doActionDelta > 0)));
	}

	#endregion actions **************************************************************************     ^Actions^

	#region Appearance ***************************************************************************************************************************      [Appearance]

	void ResetPlayer() {
		restingOnBench = false;
		sleepingOnBench = false;
		pickup = false;
		jail = false;
		sleeping = false;
		waitUntil = -1;
		waitUntilNextDay = -1;
		doActionDelta = 0;
		Body.Frame = 0 + GetFitnessLevel();
		Face.Frame = 0;
		Hat.Frame = 0;
		Legs.Frame = 0 + 9 * (int)clothes;
		Player.Position = new(960, 490);
    Player.ZIndex = 75;
		Player.Scale = Vector2.One;
		Player.Rotation = 0;
		Eye.Visible = false;
		Legs.Visible = true;
		Beard.Frame = Beardlevel(BeardLevels.Walk);
		Player.Visible = true;
		BroomPlayer.Visible = false;
		HandBrooming.Visible = false;
		SoundPlayer.Stop();
	}

	// Beard:  Walk = 0, Pickup = 1, DenialL = 3, DenialR = 2, Sit = 4

	int Beardlevel(BeardLevels level) {
		if (beard < 20 || goingUp) return 0;
		else if (beard < 40) {
			if (doActionDelta == 0 || sleepingOnBench) return 1;
			if (denial) return (int)level * 4;
			if (pickup) return 4;
			if (restingOnBench) return 16;
		}
		else if (beard < 60) {
			if (doActionDelta == 0 || sleepingOnBench) return 2;
			if (denial) return 1 + (int)level * 4;
			if (pickup) return 5;
			if (restingOnBench) return 17;
		}
		// Default case big beard
		if (denial) return 2 + (int)level * 4;
		if (pickup) return 6;
		if (restingOnBench) return 18;
		return 3;
	}


	#endregion Appearance **************************************************************************


	#region Balloon ******************************************************************************************************************************      [Balloon]

	void ShowMoneyPopup(int amount) {
		bool lost = amount < 0;
		if (lost) amount = -amount;
		int b = amount / 10;
		int c = amount % 10;
		string text = "";
		if (amount == 0) text = $"I got nothing!";
		else if (c == 0) text = $"I got {b} credits";
		else if (b == 0) text = $"I got {c} coins";
		else text = $"I got {b}.{c} credits";
		if (lost) text += ", something was lost";
		deltaBaloon = 5;
		ShowBalloon(text, 5);
	}

	bool fixtBallonSize = false;

	void ShowBalloon(string text, float time) {
		if (string.IsNullOrEmpty(text)) {
			Balloon.Visible = false;
			return;
		}
		textForBaloon = text;
		deltaBaloon = time + text.Length * .025;
		Balloon.Visible = true;
		Balloon.Size = new(400, 250);
		BalloonTxt.Text = textForBaloon;
		Vector2 lSize = BalloonTxt.GetThemeFont("font").GetStringSize(textForBaloon, HorizontalAlignment.Left, -1, BalloonTxt.GetThemeFontSize("font_size"));
		float size = Mathf.Sqrt(lSize.X * lSize.Y);
		lSize.X = size * 400 / 250f + 36 * 2;
		lSize.Y = size * 250 / 400f + 36 * 2;
		if (lSize.X < 300) lSize.X = 300;
		if (lSize.Y < 200) lSize.Y = 200;
		Balloon.Size = new(lSize.X, lSize.Y);
		BalloonTxt.Text = "";

		float top = 340 - Balloon.Size.Y;
		if (top < 10) top = 10;
		Balloon.Position = new(880 - Balloon.Size.X, top);
		fixtBallonSize = true;
	}

	public void ShowNPCBalloon(string text, Node2D npc) {
		if (string.IsNullOrEmpty(text) || status != Status.Playing) return;

		bool useBalloon1 = npcBalloonDelay1 <= 0;

		NinePatchRect NPCBalloon = useBalloon1 ? NPCBalloon1 : NPCBalloon2;
		Label NPCBalloonTxt = useBalloon1 ? NPCBalloonTxt1 : NPCBalloonTxt2;

		NPCBalloon.Visible = true;
		NPCBalloon.Size = new(400, 250);
		NPCBalloonTxt.Text = text;
		Vector2 lSize = BalloonTxt.GetThemeFont("font").GetStringSize(text, HorizontalAlignment.Left, -1, BalloonTxt.GetThemeFontSize("font_size"));
		float size = Mathf.Sqrt(lSize.X * lSize.Y);
		lSize.X = size * 400 / 250f + 36 * 2;
		lSize.Y = size * 250 / 400f + 36 * 2;
		if (lSize.X < 300) lSize.X = 300;
		if (lSize.Y < 200) lSize.Y = 200;
		NPCBalloon.Size = new(lSize.X, lSize.Y);

		float top = 340 - NPCBalloon.Size.Y;
		if (top < 10) top = 10;
		NPCBalloon.Position = new(880 - NPCBalloon.Size.X, top);
		fixtBallonSize = true;

		if (useBalloon1) {
			npcBalloonDelay1 = 2;
			npcForBalloon1 = npc;
		}
		else {
			npcBalloonDelay2 = 2;
			npcForBalloon2 = npc;
		}
	}

	void HideBalloon() {
		Balloon.Visible = false;
		BalloonTxt.Text = "";
		textForBaloon = null;
	}

	void ShowMetroMenu() {
		PanelMetro.Visible = true;
	}

	static string FormatTime(int time) {
		if (time == 0) return "Midnight";
		if (time == 12) return "Noon";
		if (time < 12) return $"{time}AM";
		else return $"{time - 12}PM";
	}

	int panelMetroSelectedButton = 0;

	void JumpToRoad(float x, RoadName r, bool useTicket) {
		roadToGo = r;
		roadXToGo = x;
		doActionDelta = 1;
		if (useTicket) {
			numTickets--;
			for (int i = 0; i < 10; i++) Tickets[i].Visible = i < numTickets;
		}
		PanelMetro.Visible = false;
	}

	// Called by the buttons in the UI
	public void JumpToRoad(int rIndex) {
		if (rIndex == 0) JumpToRoad(0, RoadName.Main_Street, true);
		else if (rIndex == 1) JumpToRoad(0, RoadName.Slum_Street, true);
		else if (rIndex == 2) JumpToRoad(0, RoadName.Side_Road, true);
		else if (rIndex == 3) JumpToRoad(0, RoadName.North_Road, true);
		else if (rIndex == 4) JumpToRoad(0, RoadName.Top_Boulevard, true);
		else if (rIndex == 5) JumpToRoad(0, RoadName.Suburb_Avenue, true);
		else if (rIndex == 6) PanelMetro.Visible = false;
	}

	#endregion Balloon **************************************************************************     ^Balloon^


	#region NPCs *********************************************************************************************************************************      [NPCs]

	int numNPCs = 0;
	double spawnDelay = 2;
	readonly bool[] npcOrders = new bool[5];
	int NPCsPerDayTime() {
		// 0->4   0
		// 5->8   1
		// 9->12  2
		// 12->16 3
		// 17->20 2
		// 20->23 1
		int num = 3;
		if (dayTime > 23 / 24.0) {
			return 0;
		}
		else if (dayTime > 20 / 24.0) {
			return 3 + num;
		}
		else if (dayTime > 16 / 24.0) {
			return 3 + num;
		}
		else if (dayTime > 12 / 24.0) {
			return 3 + num;
		}
		else if (dayTime > 8 / 24.0) {
			return 2 + num;
		}
		else if (dayTime > 4) {
			return 1 + num;
		}
		return 0;
	}

	readonly List<NPC> toProcess = new();
	void ManageNPCs(double d) {
		toProcess.Clear();
		toProcess.AddRange(NPCs);
		if (enemy != null && removeEnemy) {
			removeEnemy = false;
			enemy.Free();
			enemy = null;
		}
	}

	void RemoveAllNPCs() {
		foreach (var npc in NPCs) npc.Delete();
		NPCs.Clear();
		if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
		removeEnemy = false;
	}

	void SpawnNPC(double delta) {
		return; // FIXME they should spawn after a while


		if (spawnDelay > 0) {
			spawnDelay -= delta;
			return;
		}

		int pos = 0;
		for (int i = 0; i < npcOrders.Length; i++) {
			if (!npcOrders[i]) {
				pos = i;
				break;
			}
		}
		if (pos == -1) {
			GlobalMessage.Text = $"Cannot find a place to set the order for the NPC: {npcOrders[0]},{npcOrders[1]},{npcOrders[2]},{npcOrders[3]},{npcOrders[4]}";
			globalMessageTimeout = 5;
			return;
		}
		npcOrders[pos] = true;

		int expected = NPCsPerDayTime();
		if (numNPCs < expected) {
			var r = rnd.Randf();
			PackedScene npcScene;
			bool female = false;
			if (r < .125) npcScene = NPCTemplateMan1;
			else if (r < .25) npcScene = NPCTemplateMan2;
			else if (r < .375) npcScene = NPCTemplateMan3;
			else if (r < .5) npcScene = NPCTemplateMan4;
			else if (r < .66) { npcScene = NPCTemplateGirl1; female = true; }
			else if (r < .83) { npcScene = NPCTemplateGirl2; female = true; }
			else { npcScene = NPCTemplateGirl3; female = true; }

			var npc = npcScene.Instantiate() as NPC;
			NPCs.Add(npc);
			npc.Spawn(null, PeopleLayer, pos, female);
			numNPCs++;
			spawnDelay = 1;
		}
	}

	public void NPCGone(NPC npc) {
		npcOrders[npc.order] = false;
		NPCs.Remove(npc);
		npc.Delete();
		numNPCs--;
		spawnDelay = rnd.RandfRange(1f, 3f);
	}

	public void GiveToPlayer(int happiness) {
		if (happiness < 4) Pickup(PickableItem.Coin);
		else Pickup(PickableItem.Banknote);
	}

	internal void AddDogPoop(float xPos) {
		NPCPlayer.Stream = FartSound;
		NPCPlayer.Play();
		var item = ItemPrefabs[PrefabPoop].Instantiate() as Pickable;
		FrontItems.AddChild(item);
		item.GlobalPosition = new(xPos, rnd.RandfRange(728, 738));
	}


	double enemyDelay = 30;
	EnemyType enemyType = EnemyType.None;
	Enemy enemy = null;
	bool removeEnemy = false;

	void HandleEnemies(double delta) {
		// We can have only one enemy at time
		if (enemy != null || status != Status.Playing) return;

		// How much time between spawns?
		enemyDelay -= delta;
		//		Debug.Text = $"{enemyDelay:F1}";
		if (enemyDelay > 0) return;



		// What type of enemy to spawn?
		float probPolice = enemyProbs[0][gameDifficulty];
		float probRobber = enemyProbs[1][gameDifficulty];
		float probMaga = enemyProbs[2][gameDifficulty];
		float probZTurd = enemyProbs[3][gameDifficulty];
		float probOrban = enemyProbs[4][gameDifficulty];
		if (dayTime < 6 / 24.0 || dayTime > 22 / 24.0) probPolice += 10;
		float tot = probPolice + probRobber + probMaga + probZTurd + probOrban + (10 - gameDifficulty);
		float rndEnemy = rnd.RandfRange(0, tot);
		if (rndEnemy < probPolice) {
			enemyType = EnemyType.Police;
			enemy = NPCPolice.Instantiate() as Enemy;
		}
		else if (rndEnemy < probPolice + probRobber) {
			enemyType = EnemyType.Robber;
			enemy = NPCRobber.Instantiate() as Enemy;
		}
		else if (rndEnemy < probPolice + probRobber + probMaga) {
			enemyType = EnemyType.Maga;
			enemy = NPCMAGA.Instantiate() as Enemy;
		}
		else if (rndEnemy < probPolice + probRobber + probMaga + probZTurd) {
			enemyType = EnemyType.ZTurd;
			enemy = NPCZTurd.Instantiate() as Enemy;
		}
		else if (rndEnemy < probPolice + probRobber + probMaga + probZTurd + probOrban) {
			enemyType = EnemyType.Orban;
			enemy = NPCOrban.Instantiate() as Enemy;
		}
		else {
			enemyType = EnemyType.None;
			enemy = null;
			enemyDelay = rnd.RandfRange(10f, 20f);
		}

		// Spawn the actual enemy
		if (enemy != null) {
			PeopleLayer.AddChild(enemy);
			enemy.Spawn(null, enemyType);
		}

		// Each enemy will try to go to the player for a while, then will run away in case too much time passed or the player enters somewhere
	}

	readonly float[][] enemyProbs = {
	new float[] { 8, 7, 6, 5, 5, 5, 3, 3, 2, 2 }, // probPolice 
	new float[] { 1, 2, 4, 5, 4, 3, 3, 3, 2, 2 }, // probRobber 
	new float[] { 0, 1, 2, 3, 3, 3, 4, 5, 3, 5 }, // probMaga  
	new float[] { 0, 0, 0, 0, 3, 4, 4, 5, 3, 5 }, // probZTurd 
	new float[] { 0, 0, 0, 0, 0, 1, 2, 3, 5, 8 }, // probOrban 
	};

	void StopEnemies() {
		if (enemy == null) return;
		enemy.StopAndFlee();
	}

	public void EnemyGone() {
		removeEnemy = true;
		switch (enemyType) {
			case EnemyType.None:
				enemyDelay = rnd.RandfRange(20f, 30f);
				break;
			case EnemyType.Police:
				enemyDelay = rnd.RandfRange(20f, 30f);
				break;
			case EnemyType.DrunkGuy:
				enemyDelay = rnd.RandfRange(20f, 30f);
				break;
			case EnemyType.Robber:
				enemyDelay = rnd.RandfRange(20f, 40f);
				break;
			case EnemyType.Maga:
				enemyDelay = rnd.RandfRange(30f, 40f);
				break;
			case EnemyType.ZTurd:
				enemyDelay = rnd.RandfRange(30f, 40f);
				break;
			case EnemyType.Orban:
				enemyDelay = rnd.RandfRange(40f, 70f);
				break;
		}
		enemyDelay += (9 - gameDifficulty);

		//		Debug.Text = $"{enemyDelay:F1}";
	}

	public void RobPlayer(EnemyType what) {
		// 0 money
		// 1 atm
		// 2 clothes
		// 3 trash
		// 4 education
		// 5 razor, broom, tickets
		switch (what) {
			case EnemyType.Police: break;
			case EnemyType.DrunkGuy: break;

			case EnemyType.Robber: // money
				if (numBanknotes > 0 || numCoins > 0) {
					ShowBalloon($"I got robbed of {FormatMoney(numBanknotes * 10 + numCoins)}...", 1);
					numBanknotes = 0;
					numCoins = 0;
					ArrangeWallet(0);
				}
				break;

			case EnemyType.Maga: // steals 10% of education and ATM
				if (education > 0 && hasATM) {
					ShowBalloon($"I lost some of my education ({education * .1:F0}% and my ATM card.\nWhat a loser this guy was!", 2);
					education *= .1;
					pbEducation.Value = education;
					hasATM = false;
					ATMCard.Visible = false;
				}
				else if (education > 0) {
					ShowBalloon($"I lost some of my education ({education * .1:F0}%\nWhat a loser this guy was!", 2);
					education *= .1;
					pbEducation.Value = education;
				}
				else if (hasATM) {
					hasATM = false;
					ATMCard.Visible = false;
				}
				break;

			case EnemyType.ZTurd:
				if (hasBroom || hasRazor || numTickets > 0) {
					string msg = "I got robbed of ";
					if (hasBroom) {
						msg += "broom";
					}
					if (hasRazor) {
						if (hasBroom) msg += ", razor";
						else msg += "razor";
					}
					if (numTickets > 0) {
						if (hasBroom && hasRazor) msg += ", and metro tickets";
						else if (hasBroom || hasRazor) msg += " and metro tickets";
						else msg += "metro tickets";
					}
					msg += "!\nWhat a primitive this guy was!";
					ShowBalloon(msg, 2);
				}
				hasBroom = false;
				broomLevel = 0;
				Broom.Visible = false;
				hasRazor = false;
				Razor.Visible = false;
				numTickets = 0;
				for (int i = 0; i < 10; i++) Tickets[i].Visible = false;
				break;

			case EnemyType.Orban:
				ShowBalloon("This guy stoled everything I had!\nHe should be kicked out of civilized countries!", 2);
				numBanknotes = 0;
				numCoins = 0;
				hasBroom = false;
				broomLevel = 0;
				Broom.Visible = false;
				hasRazor = false;
				Razor.Visible = false;
				hasATM = false;
				ATMCard.Visible = false;
				numTickets = 0;
				for (int i = 0; i < 10; i++) Tickets[i].Visible = false;
				RemoveAllItems();
				ArrangeWallet(0);
				break;
		}
	}

	#endregion NPCs ************************************************************************************************************************



	#region Help *********************************************************************************************************************************      [Help]

	[ExportGroup("Help")]
	[Export] Node2D HelpSign;


	double helpDelta = 0;
	double helpSpeed = 0;
	bool helpWrite = true;
	bool helpReset = false;
	float maxHelpXPos = 300;
	float minHelpXPos = -300;
	bool actionPosible = false;
	int prevPos = -1;
	bool wasUp = false, wasDown = false;
	int numPicked = 0;


  void ResetHelp(int stage, int min, int max, bool resetText = true) {
    helpStage = stage;
    helpDelta = 0;
    helpWrite = true;
    actionPosible = false;
    prevPos = -1;
		if (resetText) GlobalMessage.Text = "";
		minHelpXPos = min;
    maxHelpXPos = max;
  }

	[Export] Label Debug;

  void ProcessHelp(double d) {
		Debug.Text = $"{((int)currentRoad.Position.X)}  {doActionDelta:F1}  {helpWrite}  {actionPosible}   ({helpStage})";

    if (helpStage == 6 && drink > 10) {
      drink -= 5 * d;
			pbDrink.Value = drink;
    }
    if (helpStage == 7 && food > 10) {
      food -= 5 * d;
      pbFood.Value = food;
    }

    if (denial && doActionDelta > 0) {
      Eye.Visible = false;
      EyesSitClosed.Visible = false;
      doActionDelta -= d* 2;
      int pos = (int)(doActionDelta * 4 + 1) % 4;
      switch (pos) {
        case 0: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup); break; // Front
        case 1: Face.Frame = 3; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialR); break; // Right
        case 2: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup); break; // Front
        case 3: Face.Frame = 4; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialL); break; // Left
      }
      if (doActionDelta <= 0) {
        denial = false;
        pickup = false;
        findAround = false;
        doActionDelta = 0;
        Body.Frame = 0 + GetFitnessLevel();
        Face.Frame = 0;
        Hat.Frame = 0;
        Legs.Frame = 0 + 9 * (int)clothes;
        Beard.Frame = Beardlevel(BeardLevels.Walk);
      }
      return;
    }

    if (doActionDelta > 0) {
      doActionDelta -= d;
			if (goingUp) {
				Eye.Visible = false;
				EyesSitClosed.Visible = false;
				Body.Frame = 2 + GetFitnessLevel();
				Face.Frame = 2;
				Beard.Visible = false;
				Hat.Frame = 2;
				int pos = (int)(doActionDelta * 9) % 4;
				switch (pos) {
					case 0: Legs.Frame = 6 + 9 * (int)clothes; break;
					case 1: Legs.Frame = 5 + 9 * (int)clothes; break;
					case 2: Legs.Frame = 6 + 9 * (int)clothes; break;
					case 3: Legs.Frame = 7 + 9 * (int)clothes; break;
				}
				Player.Position = new(960, 490 - 45 * (1 - (float)doActionDelta));
				Player.ZIndex = (int)(71 - 25 * (1 - (float)doActionDelta));

				if (doActionDelta <= 0) {
					if (helpStage == 6) { // Fountain
						goingUp = false;
            helpReset = true;
						doActionDelta = .5;
						ShowBalloon("Refreshing!", 1);
						pbDrink.Value = 100;
						drink = 100;
						ResetHelp(7, -1600, 1500);
						return;
					}
					else if (helpStage == 7) { // HotDog
						goingUp = false;
            helpReset = true;
						doActionDelta = .5;
						numBanknotes = 0;
            Banknotes[0].Visible = false;
            ShowBalloon("Noutricious!", 1);
						pbFood.Value = 100;
						food = 100;
						ResetHelp(8, -1700, 1700);
						SpawnItems();
						return;
					}
					else if (helpStage == 9) { // Trashcan
						goingUp = false;
            helpReset = true;
						doActionDelta = .5;
            RemoveAllItems();
            ResetHelp(10, -1700, 2700);
						return;
					}
					else if (helpStage == 11) { // Clothes
						goingUp = false;
            helpReset = true;
						doActionDelta = .5;
            ResetHelp(12, 0, 3000);
            Body.Texture = Body2;
            Hat.Texture = Hat2;
            return;
					}
        }
      }
			else if (doActionDelta <= 0) {
				if (pickup || helpReset) {
					ResetPlayer();
					CityMap.Visible = false;
					helpReset = false;
				}
			}
			return;
    }

    // Here will happen the magic
    var use = (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A));

    if (helpWrite) {
			string h = help[helpStage];
			helpDelta += d * helpSpeed * (use ? 40 : 1);
			int pos = (int)helpDelta;
			if (prevPos != pos) {
				prevPos++;
				if (prevPos < h.Length) {
					char c = h[prevPos];
					if (c == ' ') helpSpeed = 10;
					else if (c == ',') helpSpeed = 5;
					else if (c == '.') helpSpeed = .5;
					else if (c == '*') { helpSpeed = 20; actionPosible = true; return; }
					else helpSpeed = 20;
					GlobalMessage.Text += h[prevPos];
					if (prevPos != pos) { helpSpeed = 100; helpDelta -= d * helpSpeed * (use ? 40 : 1); }
          }
          else {
					actionPosible = true;
					helpWrite = false;
				}
			}
		}

		// If we are here we are waiting for an action
		if (!actionPosible) return;

    float jx = Input.GetJoyAxis(0, JoyAxis.LeftX);
    float jy = Input.GetJoyAxis(0, JoyAxis.LeftY);
    var left = (Input.IsKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.A) || Input.IsJoyButtonPressed(0, JoyButton.DpadLeft) || jx <= -.5f);
    var right = (Input.IsKeyPressed(Key.Right) || Input.IsPhysicalKeyPressed(Key.D) || Input.IsJoyButtonPressed(0, JoyButton.DpadRight) || jx >= .5f) && !left;
		var down = (Input.IsKeyPressed(Key.Down) || Input.IsPhysicalKeyPressed(Key.S) || Input.IsJoyButtonPressed(0, JoyButton.DpadDown) || jy >= .5f);
		var up = (Input.IsKeyPressed(Key.Up) || Input.IsPhysicalKeyPressed(Key.W) || Input.IsJoyButtonPressed(0, JoyButton.DpadUp) || jy <= -.5f);
    running = (Input.IsKeyPressed(Key.Shift) || Input.IsJoyButtonPressed(0, JoyButton.B));

		if (down) {
			if (wasDown) down = false;
			else wasDown = true;
		}
		else if (!down && wasDown) wasDown = false;
		if (up) {
			if (wasUp) up = false;
			else wasDown = true;
		}
		else if (!up && wasUp) wasUp = false;


    switch (helpStage) {
			case 0: // Waiting for going down
        if (down) {
					ResetPlayer();
					ResetHelp(1, -300, 300);
        }
        break;

			case 1: // Go left and right
        HelpMove(d, left, right);
				if (left || right) {
          doActionDelta -= d;
          if (doActionDelta < -2) {
            ResetHelp(2, -300, 300);
            Pickable item = ItemPrefabs[PrefabBanknote].Instantiate() as Pickable;
            FrontItems.AddChild(item);
            item.Position = new(600, rnd.RandfRange(728, 738));
          }
        }
        break;

			case 2: // Pickup banknote
        HelpMove(d, left, right);
				if (down && HelpPickup()) {
          ResetHelp(3, -300, 400);
					doActionDelta = .5;
					helpReset = true;
        }
        break;

			case 3: // Show sign
        HelpMove(d, left, right);
        HelpSign.Visible = true; // Make it visible after the text appears, and allow moving
        ResetHelp(4, -300, 700, false);
        break;

			case 4: // Read sign
        HelpMove(d, left, right);
				if (use) {
          foreach (var loc in locations) {
            if (loc.Visible && loc.type == LocationType.Map && Mathf.Abs(loc.GlobalPosition.X - Player.GlobalPosition.X) < loc.Width) {
              ShowMap(loc.Pos);
              ResetHelp(5, 0, 700);
              doActionDelta = .2;
              break;
            }
          }
        }
        break;

      case 5: // Close map
        if (use) {
          ResetHelp(6, 0, 1500);
          helpReset = true;
					doActionDelta = .1;
        }
        break;

      case 6: // Statistics, drink from fountain
        HelpMove(d, left, right);
        if (up) {
          foreach (var loc in locations) {
            if (loc.Visible && loc.type == LocationType.Fountain && Mathf.Abs(loc.GlobalPosition.X - Player.GlobalPosition.X) < loc.Width) {
							goingUp = true;
							doActionDelta = 1;
              break;
            }
          }
        }
        break;

			case 7: // HotDogs
        HelpMove(d, left, right);
        if (up) {
          foreach (var loc in locations) {
            if (loc.Visible && loc.type == LocationType.Shop && Mathf.Abs(loc.GlobalPosition.X - Player.GlobalPosition.X) < loc.Width) {
              // Make food good and go next help
              goingUp = true;
              doActionDelta = 1;
              break;
            }
          }
        }
        break;

			case 8: // Pick items
        HelpMove(d, left, right);
        if (down && HelpPickup()) {
					numPicked++;
					if (numPicked > 5) {
            ResetHelp(9, -1700, 1700);
          }
        }
        break;

			case 9: // Use the trashcan
        HelpMove(d, left, right);
				if (down) HelpPickup();
        if (up) {
          foreach (var loc in locations) {
            if (loc.Visible && loc.type == LocationType.Trashcan && Mathf.Abs(loc.GlobalPosition.X - Player.GlobalPosition.X) < loc.Width) {
              goingUp = true;
              doActionDelta = 1;
              break;
            }
          }
        }


        break;

			case 10: // Description about work
        HelpMove(d, left, right);
				if (!helpWrite) {
					doActionDelta = .5;
          ResetHelp(11, -1700, 2700);
        }
        break;

			case 11: // Buy clothes
        HelpMove(d, left, right);
        if (up) {
          foreach (var loc in locations) {
            if (loc.Visible && loc.type == LocationType.Shop && loc.ItemDelivered == ItemDelivered.Clothes && Mathf.Abs(loc.GlobalPosition.X - Player.GlobalPosition.X) < loc.Width) {
              goingUp = true;
              doActionDelta = 1;
              break;
            }
          }
        }
        break;

			case 12: // Bard shaving
        HelpMove(d, left, right);

				// FIXME find out why we lost the beard

        break;



    }
  }

  private bool HelpPickup() {
		pickup = true;
    doActionDelta = .5;
    Player.Scale = flipL;
    Body.Frame = 1 + GetFitnessLevel();
    Face.Frame = 1;
    Hat.Frame = 1;
    Legs.Frame = 4 + 9 * (int)clothes;
    Beard.Frame = Beardlevel(BeardLevels.Pickup);
    bool foundOne = SearchItems(false, out Pickable p);
    if (foundOne && Pickup(p.ItemType)) {
			p.Free();
			return true;
		}
		return false;
  }

  private void HelpMove(double d, bool left, bool right) {
    if (left && Player.Scale.X < 0) {
      Player.Scale = flipL;
    }
    else if (right && Player.Scale.X > 0) {
      Player.Scale = flipR;
    }

    if (left || right) {
      moving = true;
      double multiplier = (running ? 2.75 : 1.5) * (rest <= 0 ? .5 : 1) * (rest > 90 ? 1.1 : 1) * (1 + fitness * 0.01);
      moveDelta += d * multiplier;
      if (moveDelta > moveSpeed) {
        moveDelta -= moveSpeed;
        int frame = Legs.Frame + 1 - 9 * (int)clothes;
        if (frame > 3) frame = 0;
        Legs.Frame = frame + 9 * (int)clothes;
        if (frame == 0) {
          PlayerPlayer.Stream = StepSound1;
          PlayerPlayer.Play();
        }
        else if (frame == 2) {
          PlayerPlayer.Stream = StepSound2;
          PlayerPlayer.Play();
        }
      }
      float movement = (float)(d * scrollSpeed * multiplier);
      currentRoad.Position += (left ? moveL : moveR) * movement;

      float posX = currentRoad.Position.X;
			if (posX > maxHelpXPos) {
        posX = maxHelpXPos;
        currentRoad.Position = new(posX, currentRoad.Position.Y);
        moving = false;
        moveDelta = 0;
        Legs.Frame = 0 + 9 * (int)clothes;
      }
      else if (posX < minHelpXPos) {
        posX = minHelpXPos;
        currentRoad.Position = new(posX, currentRoad.Position.Y);
        moving = false;
        moveDelta = 0;
        Legs.Frame = 0 + 9 * (int)clothes;
      }
      Background0.Position = new(0.09654545454545455f * posX + 962, 306 - City.Position.Y);
      Background1.Position = new(0.27945454545454546f * posX + 974, 306 - City.Position.Y);
    }
  }

  #endregion Help *********************************************************************************************************************** ^Help^



  string[] help = {
	/* 0 */	"Hello!\n (press A or ctrl to speed up;  press Esc or Start to exit).\n" +
					"You are this guy sit on the bench. You have literally nothing.\nGet up by going down (S key or Cursor down or move down with controller)*",

	/* 1 */	"Now that you are up, you can move left and right *(controller or keyboard).\n" +
					"You can press shift (or B on the controller) to move faster.",

	/* 2 */	"If you look on the left, there is a $1 banknote on the ground*.\n"+
					"Move over it and then go down again to pick it up.",

	/* 3 */	"There are many locations inside the game,\nfor example here is a sign ",
	/* 4 */	"that shows* the current road and the map of the city.\nGo under it and press the action key (Ctrl, Enter, or button A on the controller)",
	
	/* 5 */	"*Click to close the map, or use Ctrl, Enter, or A on the Controller.",

	/* 6 */	"If you look at the bottom of your screen, you will see that you have some statistics*.\nFor example you are starting to get tirsty.\nThere is a fountain just here, more to the left.\nMove under it and then go up to drink.",

	/* 7 */	"Now you are good with drinks, but you may want also some food.\n*Move on the right and you will find a hot dog shop (remember that you can move fast with Shift or B).\nThe cost is exactly $1 so you can pay for it.\nYou need to sustain yourself (food, drink, and rest) to avoid to die.",

	/* 8 */	"You can find items on the ground.\nIt is mostly trash that can be recycled.\n Pick as many items you want.",

	/* 9 */	"*If you continue picking items you will discover that you cannoy pick up forever.\nYour space is limited and some items will not fit in your inventory.\nEither you can recycle some items (cans, bottles, paper),\neither you can throw then in a trashcan.\nThere is a transhcan close to the hot dogs stand, use it",

	/* 10 */	"*You can also find places where you can work.\nAnd you need to respect some requirements for each job:\nclothes, shaved beard, education, and decent smell.",

  /* 11 */  "*Clothes: you cannot work in an office with rags.\nThere are shops where you can find and buy clothes.\nOn the very left there is a clothes shop.\nGo there and buy some (they are free.)",

  /* 10 */  "Beard: resturants do not like long beards for the personnel.\nYou can either go to a barber shop or buy a razor and use it in your home or an hotel.\nFIXME add barber shop",

  /* 11 */  "Education: some jobs require you to have a decent education.\nYou start at zero, and you have to do the learning from elemntary schools up to college.\nThere are different schools in the city for all levels.",

	/* 12 */	"Smell: you have to take showers and wash your clothes.\nYou need soap to wash yourself and you can use laundry machines for your clothes.",

	/* 13 */	"Beware that the city is full of people, some will like you some will not. In case you do good deeds they may appreciate you more.",
	/* 14 */	"There are also bad people in the city. Try to avoid them by running away or entering locations.",
	/* 15 */	"You can also go to the gym to get a better fit and walk faster",
	/* 16 */	"Enjoy the game and try to reach 1 million $ to win!"
  };

}


/*
 Inventory

Have known size for all object types
Have a list, somewhere, of the items picked up
Before adding a new item, check if it will fit

 
 
 */

