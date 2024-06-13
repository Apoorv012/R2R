using Godot;
using Godot;
using System;
using System.Collections.Generic;

namespace R2R;

public partial class Game : Node, IGame {
	#region Properties *****************************************************************************
	[ExportGroup("Player")]
	[Export] public Node2D Player;
	[Export] Sprite2D Body;
	[Export] Texture2D Body1, Body2, Body3;
	[Export] Texture2D Hat1, Hat2, Hat3;
	[Export] Sprite2D Face;
	[Export] Node2D Eye, EyesSitClosed;
	[Export] Sprite2D Hat;
	[Export] Sprite2D Beard;
	[Export] Sprite2D Legs;
	[Export] NinePatchRect Balloon;
	[Export] Label BalloonTxt;
	[Export] Sprite2D BroomPlayer;
	[Export] Sprite2D HandBrooming;

	[ExportGroup("Params")]
	[Export] double moveSpeed;
	[Export] float scrollSpeed;
	[Export] float timeSpeed = 20;


	[ExportGroup("UI")]
	[Export] Label DayNum, DayTime, TotalFunds;
	[Export] Label GlobalMessage;
	[Export] ProgressBar pbFood, pbDrink, pbRest, pbSmellB, pbSmellC, pbBeard, pbEducation, pbFitness;
	[Export] Label pbSmellL;
	[Export] HBoxContainer ItemsHBox;
	[Export] TextureRect ATMCard;
	[Export] TextureRect[] Tickets;
	[Export] TextureRect Soap;
	[Export] TextureRect Razor;
	[Export] TextureRect Broom;
	[Export] ColorRect FadePanel;
	[Export] Button SpeedButton0, SpeedButton1, SpeedButton2, SpeedButton3, OptionsPlayButton, OptionsContinueButton;
	[Export] TextureRect[] HighlightOpts;
	[Export] TextureRect[] HighlightIntro;

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

	[ExportGroup("Options")]
	[Export] CanvasLayer PanelOptions;
	[Export] TextureRect PanelTitle, PanelSpark;
	[Export] Label MusicVolValue;
	[Export] Slider MusicVolSlider;
	[Export] Label EffetsVolValue;
	[Export] Slider EffetsVolSlider;
	[Export] OptionButton DropDownFullscreen, DropDownFPS;
	[Export] Label GameSpeedValue;
	[Export] Slider GameSpeedSlider;
	[Export] Label GameDifficultyValue;
	[Export] Slider GameDifficultySlider;
	[Export] CheckButton UsePlayerHatCB;
	[Export] CheckButton UseCustomMusicCB;

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
	[Export] PackedScene GrowingCarrotPrefab;
	[Export] Texture2D[] Soaps;

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
	[Export] public Road SuburbAvenue;
	[Export] public Road TopBoulevard;
	[Export] public Road NorthRoad;
	[Export] public Road SideRoad;
	[Export] public Road MainStreet;
	[Export] public Road SlumStreet;
	[Export] public Road HelpRoad;
	[Export] Node2D Slum_MarketDoor;
	[Export] Node2D Side_SportBarDoor;
	[Export] Location[] Benches;

	[ExportGroup("NPCs")]
	[Export] PackedScene NPCTemplateGirl1, NPCTemplateGirl2, NPCTemplateGirl3;
	[Export] PackedScene NPCTemplateMan1, NPCTemplateMan2, NPCTemplateMan3, NPCTemplateMan4;
	[Export] PackedScene NPCPolice, NPCRobber, NPCMAGA, NPCZTurd, NPCOrban;
	[Export] Sprite2D DumpDog, BoneForDog;
	[Export] NinePatchRect NPCBalloon1, NPCBalloon2;
	[Export] Label NPCBalloonTxt1, NPCBalloonTxt2;
	[Export] Enemy DrunkGuy;

	[ExportGroup("Sounds")]
	[Export] AudioStreamPlayer MusicPlayer, SoundPlayer, PlayerPlayer, NPCPlayer;
	[Export] AudioStream MusicC64, MusicDisco, MusicBroken, MusicOrchestra, MusicDance, MusicRock;
	[Export] AudioStream StepSound1, StepSound2, PickupSound, NoSound, SuccessSound, FailureSound;
	[Export] AudioStream FartSound, DogBarkSound, EatSound, DrinkSound, EatDrinkSound, ShowerSound;
	[Export] AudioStream LaundrySound, BroomSound, CashSound, SleepSound, StudySound;
	[Export] AudioStream Work1Sound, Work2Sound, Work3Sound, WorkoutSound, WashSound;
	[Export] public AudioStream[] FNo, MNo, FYes, MYes, FDisgust, MDisgust, FApproval, MApproval;

	[ExportGroup("Debug")]

	readonly List<NPC> NPCs = new();

	public Road currentRoad;

	#endregion Properties *****************************************************************************


	#region Variables **************************************************************

	bool firstStart = true;
	public enum Status { Intro, Starting, Playing, Dying, GameOver, Win };

	public Status status = Status.Intro;
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
	bool fading = false;
	RoadName roadToGo = RoadName.None;
	float roadXToGo = 0;

	bool isDogPacified = false;
	double dogBark = 0;
	double throwBone = 0;
	bool marketSpawn = false;
	bool barSpawn = false;
	bool wasInsideCrossroad = false;
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
		Error err = config.Load("user://R2R.cfg");
		if (err == Error.Ok) {
			// Fetch the data for each section.
			int vol = (int)config.GetValue("Sounds", "MusicVolume");
			MusicVolValue.Text = $"{vol:N0}%";
			MusicPlayer.VolumeDb = (float)(.5 * vol - 40);
			vol = (int)config.GetValue("Sounds", "SoundsVolume");
			EffetsVolValue.Text = $"{vol:N0}%";
			SoundPlayer.VolumeDb = (float)(.5 * vol - 40);
			PlayerPlayer.VolumeDb = (float)(.5 * vol - 40);
			NPCPlayer.VolumeDb = (float)(.5 * vol - 40);
			int fullscreen = (int)config.GetValue("Graphics", "Fullscreen");
			DisplayServer.WindowSetMode(fullscreen == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
			winWidth = (int)config.GetValue("Graphics", "WinWidth", 1920);
			winHeight = (int)config.GetValue("Graphics", "WinHeight", 1080);
			int maxFPS = (int)config.GetValue("Graphics", "MaxFPS", 2);
			DropDownFPS.Selected = maxFPS;
			Engine.MaxFps = maxFPS switch {
				0 => 0,
				1 => 144,
				2 => 60,
				3 => 50,
				4 => 30,
				_ => 0
			};
			if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed && winWidth != 0) {
				DisplayServer.WindowSetSize(new(winWidth, winHeight), 0);
			}
			DropDownFullscreen.Selected = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed ? 1 : 0;
			int gs = (int)config.GetValue("Game", "Speed", 4);
			GameSpeedSlider.SetValueNoSignal(gs);
			gameSpeed = gs switch {
				0 => .1,
				1 => .25,
				2 => .5,
				3 => .8,
				4 => 1,
				5 => 1.1,
				6 => 1.2,
				7 => 1.5,
				8 => 2,
				9 => 5,
				_ => 1,
			};
			GameSpeedValue.Text = gs switch {
				0 => "Frozen",
				1 => "Very slow",
				2 => "Slow",
				3 => "Chilly",
				4 => "Normal",
				5 => "Rush",
				6 => "Speedy",
				7 => "Fast",
				8 => "Flash",
				9 => "Insane",
				_ => "Normal",
			};
			gameDifficulty = (int)config.GetValue("Game", "Difficulty", 5);
			GameDifficultySlider.SetValueNoSignal(gameDifficulty);
			GameDifficultyValue.Text = gameDifficulty switch {
				0 => "Shevchenko",
				1 => "Shmyhal",
				2 => "Galushchenko",
				3 => "Kuleba",
				4 => "Umerov",
				5 => "Kamyshin",
				6 => "Budanov",
				7 => "Syrskyi",
				8 => "Zaluzhny",
				9 => "Zelenskyy",
				_ => "Kamyshin",
			};

			Hat.Visible = (bool)config.GetValue("Game", "UseHatForPlayer", true);
			UsePlayerHatCB.SetPressedNoSignal(Hat.Visible);
			UseCustomMusicCB.SetPressedNoSignal((bool)config.GetValue("Game", "UseCustomMusic", false));
		}


		foreach (var hl in HighlightOpts) hl.Visible = false;
		PanelOptions.Visible = false;

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


		for (int i = 0; i < npcOrders.Length; i++) npcOrders[i] = false;

		ResetAllValues();

		DrunkGuy.Spawn(this, EnemyType.DrunkGuy);

		firstStart = false;
		OptionsPlayButton.Text = "Start";
		OptionsContinueButton.Disabled = true;
		SetStatus(Status.Intro);
	}

	void SetStatus(Status s) {
		status = s;
		switch (s) {
			case Status.Intro:
				IntroPanel.Visible = true;
				GameOverPanel.Visible = false;
				GameOverAnim.Active = false;
				WinPanel.Visible = false;
				WinAnim.Active = false;
				doActionDelta = 1f;
        MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
        MusicPlayer.Stream = PickupSound;
				MusicPlayer.Play();
				IntroText.Text = IntroText.Text.Replace("<calc years ago>", (DateTime.Now.Year - 1985).ToString());
				foreach (var h in HighlightIntro) h.Visible = false;
				break;
			case Status.Starting:
				IntroPanel.Visible = false;
				introTextScrollPos = 400;
				GameOverPanel.Visible = false;
				GameOverAnim.Active = false;
				WinPanel.Visible = false;
				WinAnim.Active = false;
				doActionDelta = .5f;
				OptionsPlayButton.Text = "Restart";
				OptionsContinueButton.Disabled = false;
				break;
			case Status.Playing:
				break;
			case Status.Dying:
				break;
			case Status.GameOver:
				GameOverPanel.Visible = true;
				GameOverAnim.Active = true;
				GameOverAnim.Play("GameOver");
				WinPanel.Visible = false;
				WinAnim.Active = false;
				doActionDelta = .5f;
				break;
			case Status.Win:
				GameOverPanel.Visible = false;
				GameOverAnim.Active = false;
				WinPanel.Visible = true;
				WinAnim.Active = true;
				MusicPlayer.Stop();
				doActionDelta = 3;
				break;
		}
	}

	public bool Won => status == Status.Win;

	public bool IsTopBoulevard() {
		if (currentRoad != TopBoulevard) return false;
		int bad = 0;
		if (clothes == Clothes.Rags) bad++;
		if (totalSmell > 80) bad++;
		if (beard > 80) bad++;
		return (bad >= 2);
	}
	public bool IsNorthRoad() {
		if (currentRoad != NorthRoad) return false;
		int bad = 0;
		if (clothes == Clothes.Rags) bad++;
		if (totalSmell > 80) bad++;
		if (beard > 80) bad++;
		return (bad >= 2);
	}

	public bool iSleepingOnBench => sleepingOnBench;


  public Node2D iPlayer => Player;
	public AudioStream[] iFNo => FNo;
	public AudioStream[] iMNo => MNo;
	public AudioStream[] iFYes => FYes;
	public AudioStream[] iMYes => MYes;
	public AudioStream[] iFDisgust => FDisgust;
	public AudioStream[] iMDisgust => MDisgust;
	public AudioStream[] iFApproval => FApproval;
	public AudioStream[] iMApproval => MApproval;

  private void ResetAllValues() {
		dayNum = 1;
		dayTime = 8 / 24.0; // 0 .. 1
    DayNum.Text = $"Day {dayNum}, week {dayNum / 7 + 1}";
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
		money = 0;
		investedMoney = 0;
		hasATM = false;
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
		fading = false;
		roadToGo = RoadName.None;
		roadXToGo = 0;

		isDogPacified = false;
		dogBark = 0;
		throwBone = 0;
		marketSpawn = false;
		barSpawn = false;
		wasInsideCrossroad = false;

		currentHotel = null;

    Soap.Visible = false;
		Razor.Visible = false;
    Broom.Visible = false;
		BroomPlayer.Visible = false;
		HandBrooming.Visible = false;
		ATMCard.Visible = false;
		DumpDog.Visible = false;
		BoneForDog.Visible = false;

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
		numNPCs = 0;
		for (int i = 0; i < npcOrders.Length; i++) npcOrders[i] = false;
    if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
    DrunkGuy.Visible = false;
    drunkGuyBench = null;
    enemyDelay = 10;
		enemyType = EnemyType.None;
		removeEnemy = false;

		RemoveAllItems();
		ArrangeWallet(0);
		ResetPlayer();
		SetClothes(Clothes.Rags);
		SetRoad(SlumStreet, 0);
  }

	bool joyJustPressed = false;

	public override void _Process(double delta) {
		bool joyStart = Input.IsJoyButtonPressed(0, JoyButton.Start);
		bool rb = Input.IsJoyButtonPressed(0, JoyButton.RightShoulder);
		bool lb = Input.IsJoyButtonPressed(0, JoyButton.LeftShoulder);
		bool joyBack = Input.IsJoyButtonPressed(0, JoyButton.Back);



		if (Input.IsActionJustPressed("Z")) { // FIXME
    }


		if (musicToSetDelay > 0) {
			musicToSetDelay -= delta;
			if (musicToSetDelay <= 0) {
        MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
				MusicPlayer.Stream = musicToSet;
				musicToSetDelay = 0;
				musicToSet = null;
      }
    }

    if (!rb && !lb && !joyStart && !joyBack) {
			joyJustPressed = false;
		}
		if (status == Status.Intro) {
			if (PanelOptions.Visible) {
				HandleMenuActions(joyStart, delta);
				return;
			}
			ShowIntro(delta);
			HandleIntroActions(delta);
			return;
		}
		else if (status == Status.Win) {
			doActionDelta -= delta;
			SpawnNPC(delta);
			ManageNPCs(delta);
			HandleEnemies(delta);
      HandleDrunkGuy(delta);
      if (doActionDelta < 0 && (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A))) {
				SetStatus(Status.Intro);
			}
			return;
		}


    if (PanelOptions.Visible) {
			HandleMenuActions(joyStart, delta);
			return;
		}

		if (Input.IsActionJustPressed("Esc") || (joyStart && !joyJustPressed)) {
			joyJustPressed = true;
			PanelOptions.Visible = true;
			foreach (var hl in HighlightOpts) hl.Visible = false;
		}

		if (Input.IsActionJustPressed("F11") || (joyBack && !joyJustPressed)) {
			joyJustPressed = true;
			SwitchFullscreen();
			SaveOptions();
		}

		if (globalMessageTimeout > 0) {
			globalMessageTimeout -= delta;
			if (globalMessageTimeout <= 0) GlobalMessage.Text = "Rags 2 Riches";
		}


		if (rb && !joyJustPressed) {
			joyJustPressed = true;
			GameSpeed(userGameSpeed switch {
				0 => 1,
				1 => 2,
				2 => 3,
				5 => 0,
				_ => 1
			});
		}
		if (lb && !joyJustPressed) {
			joyJustPressed = true;
			GameSpeed(userGameSpeed switch {
				0 => 3,
				1 => 0,
				2 => 1,
				5 => 2,
				_ => 1
			});
		}
		if (!rb && !lb && !joyStart && !joyJustPressed) {
			joyJustPressed = false;
		}

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

		double d = delta * userGameSpeed;

		HandleTime(d);
		if (status == Status.Starting) {
			doActionDelta -= delta;
			if (doActionDelta <= 0) {
				doActionDelta = 0;
				SetStatus(Status.Playing);
			}
		}

		SpawnNPC(d);
		HandleEnemies(d);
    HandleDrunkGuy(d);
    ManageNPCs(d);

		if (status == Status.Dying) {
			Balloon.Visible = false;
			Die(d);
			return;
		}
		else if (status == Status.GameOver) {
			if (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A)) {
				RemoveAllNPCs();
				_Ready();
			}
			return;
		}

		ProcessBaloons(delta); // Balloons time should not be affected by game speed


    if (jail) {
			if (drink < 10) drink = 10;
			if (food < 10) food = 10;
			rest++; if (rest > 100) rest = 100;
			if (bodySmell > 50) bodySmell = 50;
			HandleStats(d);
			if (dayTime > 7 / 24.0 && dayTime < 8 / 24.0) {
				ResetPlayer();
			}
		}
		else if (waitUntilNextDay != -1) {
			if (!sleeping) HandleStats(d);
			else HandleSleepStats(d);
		}
		else if (waitUntil != -1) {
			if (dayTime >= waitUntil) {
				CompleteJob();
			}
			if (!sleeping) HandleStats(d);
			else HandleSleepStats(d);
		}
		else {
			HandleMovements(d);
			ProcessActions(d);
			HandleStats(d);
		}
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
      throwBone = 0;
      BoneForDog.Visible = false;
      DumpDog.Visible = false;
      DumpDog.Frame = 0;
      currentHotel = null;
			if (prevWeek != dayNum / 7) {
				money += (int)(investedMoney * .1);
        TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
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
      MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
      MusicPlayer.Play();
			playedMusic = true;
		}
		if (playedMusic && h == 8 && m == 0) {
			playedMusic = false;
		}

		double dayHour = dayTime * 24;
		if (dayHour < 5 || dayHour > 22) {
			Sun.Energy = .75f;
			Sun.BlendMode = Light2D.BlendModeEnum.Sub;
			Sun.Color = Colors.White;
		}
		else if (dayHour >= 7 && dayHour < 19) {
			Sun.Energy = .05f;
			Sun.BlendMode = Light2D.BlendModeEnum.Add;
			Sun.Color = SunColor;
		}
		else if (dayHour >= 5 && dayHour < 6) { // Ramp up staing negative
			Sun.Energy = (float)(6 - dayHour) * .75f;
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
			Sun.Energy = (float)(dayHour - 20) * .75f * .5f;
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
				foreach (var light in currentRoad.StreetLights.GetChildren()) {
					if (light.GetChild(0) is PointLight2D l)
						l.Enabled = true;
				}
			}
		}
		else if (enableLights) {
			enableLights = false;
			foreach (var light in currentRoad.StreetLights.GetChildren()) {
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

		// Market spawn
		if (!marketSpawn && dayTime > 18 / 24.0 && currentRoad.Name == RoadName.Slum_Street) {
			marketSpawn = true;
			var veggy = ItemPrefabs[rnd.RandiRange(0, 2) == 2 ? PrefabBone : PrefabRotCarrot].Instantiate() as Pickable;
			SlumStreet.FrontItems.AddChild(veggy);
			veggy.GlobalPosition = new(Slum_MarketDoor.GlobalPosition.X + rnd.RandfRange(-100f, 300f), rnd.RandfRange(720, 728));
		}
		if (marketSpawn && dayTime < 1 / 24.0) {
			marketSpawn = false;
		}

		// Bar spawn
		if (!barSpawn && dayTime > 17 / 24.0 && currentRoad.Name == RoadName.Side_Road) {
			barSpawn = true;
			for (int i = 0; i < 4; i++) {
				var trash = ItemPrefabs[rnd.RandiRange(0, 1) == 0 ? PrefabCan : PrefabPaper].Instantiate() as Pickable;
				SideRoad.FrontItems.AddChild(trash);
				trash.GlobalPosition = new(Side_SportBarDoor.GlobalPosition.X + rnd.RandfRange(-300f, 300f), rnd.RandfRange(720, 728));
			}
		}
		if (barSpawn && dayTime < 1 / 24.0) {
			barSpawn = false;
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

		if (restingOnBench || sleepingOnBench || sleeping) {
			up = false;
			down = false;
			left = false;
			right = false;
		}

		if (PanelMetro.Visible) {
			HandlePanelMetro(left, right, up, down, use);
			return;
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
			double multiplier = (running ? 3.25 : 2) * (rest <= 0 ? .5 : 1) * (rest > 90 ? 1.1 : 1) * (1 + fitness * 0.01);
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
			float playerX = Player.GlobalPosition.X;
			bool inside = false;
			for (int i = 0; i < Crossroads.Length; i++) {
				if (playerX > Crossroads[i].X + currentRoad.Position.X && playerX < Crossroads[i].Y + currentRoad.Position.X) {
					inside = true;
					break;
				}
			}
			if (inside && !wasInsideCrossroad) {
				Player.Position = new(960, 520);
				wasInsideCrossroad = true;
			}
			else if (!inside && wasInsideCrossroad) {
				Player.Position = new(960, 490);
				wasInsideCrossroad = false;
			}

			float posX = currentRoad.Position.X;
			if (posX > currentRoad.MaxX) {
				posX = currentRoad.MaxX;
				currentRoad.Position = new(posX, currentRoad.Position.Y);
				moving = false;
				moveDelta = 0;
				Legs.Frame = 0 + 9 * (int)clothes;
			}
			else if (posX < currentRoad.MinX) {
				posX = currentRoad.MinX;
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
			Hat.Frame = 3;
			Legs.Frame = 4 + 9 * (int)clothes;
			Beard.Frame = Beardlevel(BeardLevels.Pickup);
			HideBalloon();
		}

		if (use || (foundLocation != null && foundLocation.type == LocationType.Sign)) {
			if (foundLocation.type == LocationType.Dump && numBones > 0) {
				throwBone = 1;
				BoneForDog.Visible = true;
				BoneForDog.GlobalPosition = Player.GlobalPosition;
				DumpDog.Frame = 2;
				DumpDog.Visible = true;
				InventoryRemoveItem(PickableItem.Bone, true);
				numBones--;
				return;
			}
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

	AudioStream musicToSet = null;
	double musicToSetDelay = 0;

  void SetRoad(Road road, float posX) {
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(MusicPlayer, "volume_db", -20, .5);

    currentRoad = road;
		handlingCrossroads = true;
		Crossroads = road.Crossroads;
		handlingCrossroads = false;
		currentRoad.Position = new(posX, currentRoad.Position.Y);
		musicToSetDelay = 1;
    switch (road.Name) {
			case RoadName.Slum_Street:
				City.Position = new(0, 0);
				Sun.Position = new(395, 162);
				Background0.Texture = Mountains;
				Background1a.Texture = Hills;
				Background1b.Texture = Hills;
        musicToSet = GetMusic(MusicC64);
				break;

			case RoadName.Main_Street:
				City.Position = new(0, 1000);
				Sun.Position = new(395, 162 - 1000);
				Background0.Texture = Skyscrapers;
				Background1a.Texture = Buildings;
				Background1b.Texture = Buildings;
        musicToSet = GetMusic(MusicOrchestra);
				break;

			case RoadName.Side_Road:
				City.Position = new(0, 2000);
				Sun.Position = new(395, 162 - 2000);
				Background0.Texture = Mountains;
				Background1a.Texture = Hills;
				Background1b.Texture = Hills;
        musicToSet = GetMusic(MusicDisco);
				break;

			case RoadName.North_Road:
				City.Position = new(0, 3000);
				Sun.Position = new(395, 162 - 3000);
				Background0.Texture = Skyscrapers;
				Background1a.Texture = Buildings;
				Background1b.Texture = Buildings;
				musicToSet = GetMusic(MusicRock);
				break;

			case RoadName.Top_Boulevard:
				City.Position = new(0, 4000);
				Sun.Position = new(395, 162 - 4000);
				Background0.Texture = Skyscrapers;
				Background1a.Texture = Buildings;
				Background1b.Texture = Buildings;
        musicToSet = GetMusic(MusicDance);
				break;

			case RoadName.Suburb_Avenue:
				City.Position = new(0, 5000);
				Sun.Position = new(395, 162 - 5000);
				Background0.Texture = Mountains;
				Background1a.Texture = Hills;
				Background1b.Texture = Hills;
				musicToSet = GetMusic(MusicBroken);
				break;
    }

    Background0.Position = new(0.0890909090909091f * posX + 980, 306 - City.Position.Y);
		if (posX > 0)
			Background1.Position = new(0.1781818181818182f * posX, 306 - City.Position.Y);
		else
			Background1.Position = new(0.1781818181818182f * posX + 1960, 306 - City.Position.Y);


		locations.Clear();
		foreach (var node in currentRoad.Doors.GetChildren()) {
			if (node is Location l) {
				locations.Add(l);
			}
		}
		foreach (var node in currentRoad.BackItems.GetChildren()) {
			if (node is Location l) {
				locations.Add(l);
			}
		}
		foreach (var node in currentRoad.FrontItems.GetChildren()) {
			if (node is Location l) locations.Add(l);
		}

		foreach (var npc in NPCs) npc.Delete();
		NPCs.Clear();
    numNPCs = 0;
    for (int i = 0; i < npcOrders.Length; i++) npcOrders[i] = false;
		if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
		npcForBalloon1 = null;
		npcForBalloon2 = null;
		Player.ZIndex = 75;
		SpawnItems();
		fitPosition = true;

    // Set forcibly the lights
    int now = (int)(dayTime * 24);
    if (now < 5 || now > 21) {
			enableLights = true;
			foreach (var light in currentRoad.StreetLights.GetChildren()) {
				if (light.GetChild(0) is PointLight2D l)
					l.Enabled = true;
			}
		}
		else {
			enableLights = false;
			foreach (var light in currentRoad.StreetLights.GetChildren()) {
				if (light.GetChild(0) is PointLight2D l)
					l.Enabled = false;
			}
		}
	}

  private AudioStream GetMusic(AudioStream defaultMusic) {
		if (!UseCustomMusicCB.ButtonPressed) return defaultMusic;

    var exePath = OS.GetExecutablePath();
		if (OS.IsDebugBuild()) {
      exePath = "C:/Users/claud/Godot/R2R/";
    }
    int slashPos = exePath.LastIndexOf('/') + 1;
    exePath = exePath[0..slashPos] + "Music/";
    GD.Print($"Checking for music in: {exePath}");
    using var dir = DirAccess.Open(exePath);
    List<string> foundMusics = new();
		foreach (var file in DirAccess.GetFilesAt(exePath)) {
			if (file.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)) {
				foundMusics.Add(exePath + file);
			}
		}
		if (foundMusics.Count == 0) return defaultMusic;

		string music = foundMusics[rnd.RandiRange(0, foundMusics.Count - 1)];
    AudioStream sound;
		if (music[^1] == '3') {
			using var musicFile = FileAccess.Open(music, FileAccess.ModeFlags.Read);
			sound = new AudioStreamMP3 {
				Data = musicFile.GetBuffer((long)musicFile.GetLength())
			};
		}
		else {
			sound = AudioStreamOggVorbis.LoadFromFile(music);
		}
    return sound;
  }

  Vector2[] Crossroads;
	bool handlingCrossroads = false;

	void ShowMap(Vector2 pos) {
		CityMap.Visible = true;
		YouAreHere.Position = pos;
	}

	#endregion movement **************************************************************************  ^movement^

	#region stats ********************************************************************************************************************************      [Stats]

	void HandleStats(double delta) {
		double timeDiff = dayTime - prevDayTime;
		if (prevDayTime > dayTime) {
			prevDayTime = 0;
			return;
		}
		prevDayTime = dayTime;
		if (timeDiff >= dayTime) return;


		bool whileWorking = (foundLocation != null && foundLocation.type == LocationType.Job);
		double difficultyFactor = 0.9 * gameDifficulty + 0.3;
		double difficultyFactorRest = 0.667 * gameDifficulty + 0.5;

    double nFood = food - timeDiff * 75 * (running ? 1.5 : 1) * (whileWorking ? .5 : 1) * difficultyFactor;
    if (nFood < 0) nFood = 0;
		double nDrink = drink - timeDiff * 150 * (running ? 1.25 : 1) * (whileWorking ? .5 : 1) * difficultyFactor;
    if (nDrink < 0) nDrink = 0;
		double restSpeed = dayTime < 6 / 24.0 || dayTime > 22 / 24.0 ? 85 : 60; // During the night the rest decreases faster
		double nRest = rest - timeDiff * restSpeed * (running ? 2 : 1) * (nFood == 0 && nDrink == 0 ? 10 : 1) * difficultyFactorRest;
    if (nRest < 0) nRest = 0;
		double nBodySmell = bodySmell + timeDiff * 35 * (running ? 2 : 1) * difficultyFactor;
    if (nBodySmell > 100) nBodySmell = 100;
		double nClothesSmell = dirtyClothes + timeDiff * 15 * difficultyFactor;
    if (dirtyClothes > 100) dirtyClothes = 100;
		double nSmell = nBodySmell + nClothesSmell;
		if (nSmell > 100) nSmell = 100;
		totalSmell = nSmell;
		double nBeard = beard + timeDiff * 25 * difficultyFactor;
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
		pbSmellB.SetValueNoSignal(nBodySmell);
		pbSmellC.SetValueNoSignal(nClothesSmell);
		pbSmellL.Text = $"{nSmell:N0}%";
		bodySmell = nBodySmell;
		dirtyClothes = nClothesSmell;
		beard = nBeard;
		pbBeard.SetValueNoSignal(beard);

    // Handle death and collapse
    if (food == 0 && drink == 0 && rest == 0) {
			if (foundLocation != null && foundLocation.type == LocationType.Job && HasMoney(100)) {
				// If we working we will go to the Hospital
				if (!hospital) {
					hospital = true;
					roadToGo = RoadName.Main_Street;
					roadXToGo = -1100;
					fading = true;
					FadePanel.Visible = true;
					doActionDelta = 1;
					waitUntil = -1;
				}
			}
			else {
				ResetPlayer();
				SetStatus(Status.Dying);
			}
		}
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



	void Die(double delta) {
		if (deathDelta >= 1) return;
		Player.Rotation = deathDelta * Mathf.Pi * .5f;
		Eye.Visible = true;
		Body.Frame = 0 + GetFitnessLevel();
		Face.Frame = 0;
		Legs.Frame = 3 + 9 * (int)clothes;
		Beard.Frame = Beardlevel(BeardLevels.Walk);
		Player.Position = new(960, 490 + deathDelta * 110);
		Player.Scale = new(1 - deathDelta * .2f, 1);
		deathDelta += (float)delta * 1.5f;

		if (deathDelta >= 1) {
			SetStatus(Status.GameOver);
		}
	}



	#endregion stats **************************************************************************   ^Stats^

	#region Inventory ****************************************************************************************************************************      [Inventory]

	private static string FormatMoney(int money) {
		if (money < 1000) return $"$ {money / 10}.{money % 10}0";
		if (money < 1000000) return $"$ {money / 10000},{(money / 10) % 1000}.{money % 10}0";
		return $"$ {money / 10000000},{(money / 10000) % 1000},{(money / 10) % 1000}.{money % 10}0";
	}

	private string FormatPocket() {
		int amount = numBanknotes * 10 + numCoins;
		return $"$ {amount / 10}.{amount % 10}0";
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
		TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
		if (money >= 10000000) {
			SetStatus(Status.Win);
		}
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
		// Check if we did not spawn already the items for the road (at least 2 hours time between spawns)
		switch (currentRoad.Name) {
			case RoadName.Main_Street:
				if (dayTime < lastSpawnMain + 12 / 24.0) return;
				lastSpawnMain = dayTime;
				break;
			case RoadName.Slum_Street:
				if (dayTime < lastSpawnSlum + 6 / 24.0) return;
				lastSpawnSlum = dayTime;
				break;
			case RoadName.Side_Road:
				if (dayTime < lastSpawnSide + 6 / 24.0) return;
				lastSpawnSide = dayTime;
				break;
			case RoadName.North_Road:
				if (dayTime < lastSpawnNorth + 18 / 24.0) return;
				lastSpawnNorth = dayTime;
				break;
			case RoadName.Top_Boulevard:
				if (dayTime < lastSpawnTop + 24 / 24.0) return;
				lastSpawnTop = dayTime;
				break;
			case RoadName.Suburb_Avenue:
				if (dayTime < lastSpawnSuburb + 2 / 24.0) return;
				lastSpawnSuburb = dayTime;
				break;
		}

		// Cleanup previous items
		while (currentRoad.FrontItems.GetChildCount() > 2) {
			var node = currentRoad.FrontItems.GetChild(rnd.RandiRange(0, currentRoad.FrontItems.GetChildCount() - 1));
			GD.Print($"Cleaning {(node as Pickable).ItemType} from {currentRoad.Name}");
			node.Free();
		}

		float pos = currentRoad.MinX;
		// Go from one end to the other end, the size will depend on the street
		while (pos < currentRoad.MaxX) {
			// Check if we have an items very close, in case skip
			bool tooClose = false;
			foreach (var item in currentRoad.FrontItems.GetChildren()) {
				if (Mathf.Abs((item as Node2D).GlobalPosition.X - pos) < 50) {
					tooClose = true;
					break;
				}
			}
			if (!tooClose) {
				// Spawn an object using the defined probabilities
				// poop 9%, paper 15%, cans 20%, bottles 25%, coins 25%, banknotes 5%, bones 1%
				float prob = rnd.RandfRange(0, 100f);
				Pickable item;
				if (prob < 9) {
					item = ItemPrefabs[PrefabPoop].Instantiate() as Pickable;
				}
				else if (prob < 9 + 15) {
					item = ItemPrefabs[PrefabPaper].Instantiate() as Pickable;
				}
				else if (prob < 9 + 15 + 20) {
					item = ItemPrefabs[PrefabCan].Instantiate() as Pickable;
				}
				else if (prob < 9 + 15 + 20 + 25) {
					item = ItemPrefabs[PrefabBottle].Instantiate() as Pickable;
				}
				else if (prob < 9 + 15 + 20 + 25 + 25) {
					item = ItemPrefabs[PrefabCoin].Instantiate() as Pickable;
				}
				else if (prob < 9 + 15 + 20 + 25 + 25 + 5) {
					item = ItemPrefabs[PrefabBanknote].Instantiate() as Pickable;
				}
				else {
					item = ItemPrefabs[PrefabBone].Instantiate() as Pickable;
				}
				currentRoad.FrontItems.AddChild(item);
				item.Position = new(pos, rnd.RandfRange(728, 738));
			}

			// Step a random long amount of distance
			var minDist = 1000f + 50 * gameDifficulty;
			var maxDist = 2000f + 200 * gameDifficulty;
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
				case 0: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup);		Hat.Frame = 3;	break; // Front
				case 1: Face.Frame = 3; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialR);	Hat.Frame = 4;	break; // Right
				case 2: Face.Frame = 1; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.Pickup);		Hat.Frame = 3;	break; // Front
				case 3: Face.Frame = 4; if (beard > 10) Beard.Frame = Beardlevel(BeardLevels.DenialL);	Hat.Frame = 5;	break; // Left
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
			Hat.Frame = 1;
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

		if (fading && doActionDelta > 0) {
			// 2500 -> -3000
			float x = -3000 + (float)doActionDelta * 5500;
			FadePanel.Position = new(x, -400);
			// 1->.66 .66->.33 .33->0
			if (doActionDelta > .75) FadePanel.Color = new Color(0, 0, 0, (float)(1 - doActionDelta) * 4);
			else if (doActionDelta > .25) {
				if (roadToGo != RoadName.None) {
					FadePanel.Color = new Color(0, 0, 0, 1);
					switch (roadToGo) {
						case RoadName.None: break;
						case RoadName.Main_Street: SetRoad(MainStreet, roadXToGo); break;
						case RoadName.Slum_Street: SetRoad(SlumStreet, roadXToGo); break;
						case RoadName.Side_Road: SetRoad(SideRoad, roadXToGo); break;
						case RoadName.North_Road: SetRoad(NorthRoad, roadXToGo); break;
						case RoadName.Top_Boulevard: SetRoad(TopBoulevard, roadXToGo); break;
						case RoadName.Suburb_Avenue: SetRoad(SuburbAvenue, roadXToGo); break;
					}
					Background0.Position = new(0.09654545454545455f * roadXToGo + 962, 306 - City.Position.Y);
					Background1.Position = new(0.27945454545454546f * roadXToGo + 974, 306 - City.Position.Y);
					roadToGo = RoadName.None;
				}
			}
			else FadePanel.Color = new Color(0, 0, 0, (float)(doActionDelta * 4));

			doActionDelta -= delta * 1.5;
			if (doActionDelta <= 0) {
				FadePanel.Visible = false;
				fading = false;
				doActionDelta = 0;
				foundLocation = null;
				if (hospital) {
					// Do the hospital message here.
					ShowBalloon("I collapsed!\nLuckily they brought me to the hospital.\nIt costed me $10", 4);
					RemoveMoney(100);
					hospital = false;
					if (food < 20) food = 20;
					if (drink < 50) drink = 50;
					if (rest < 20) rest = 20;
				}
				ResetPlayer();
			}
		}

		if (dogBark > 0) {
			dogBark -= delta;
			if (!isDogPacified) {
				// Show dog barking and play sound
				DumpDog.Visible = true;
				DumpDog.Frame = ((int)(dogBark * 5)) % 2;
			}

			if (dogBark <= 0) {
				dogBark = 0;
				DumpDog.Visible = false;
			}
		}

		if (throwBone > 0) {
			throwBone -= delta;

			// Mid point
			float midX = (DumpDog.GlobalPosition.X * 2 + 960) / 3;
			float midY = DumpDog.GlobalPosition.Y - 400; // Intentionally high because is the control point of the Bezier curve

			// Bezier curve
			float t = (float)(1 - throwBone);
			float px = midX + (1 - t) * (1 - t) * (960 - midX) + t * t * (DumpDog.GlobalPosition.X - midX);
			float py = midY + (1 - t) * (1 - t) * (490 - midY) + t * t * (DumpDog.GlobalPosition.Y - midY);

			BoneForDog.GlobalPosition = new(px, py);
			if (throwBone <= 0) {
				throwBone = 0;
				BoneForDog.Visible = false;
				DumpDog.Visible = true;
				DumpDog.Frame = 3;
				isDogPacified = true;
			}
		}

	}


	private bool SearchItems(bool brooming, out Pickable pickable) {
		float playerHand = Player.GlobalPosition.X + (brooming ? -5 : -75);
		float extraSize = brooming ? 45 : 16;
		foreach (var item in currentRoad.FrontItems.GetChildren()) {
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
        TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
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
        TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
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
		GlobalMessage.Text = "Cannot pick it up";
		globalMessageTimeout = 3;
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
            TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
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
            TotalFunds.Text = $"Pocket  {FormatPocket()}            Bank  {FormatMoney(money)}";
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
								foundLocation = l;
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
								SetClothes(Clothes.Cheap);
								dirtyClothes = 0;
								ShowBalloon("That is an upgrade!", 2);
								success = ResultSound.GenericSuccess;
								StopEnemies();
								break;

							case ItemDelivered.Suits:
								if (clothes == Clothes.Classy) ShowBalloon("I already have these clothes!", 2);
								else {
									RemoveMoney(l.price);
									SetClothes(Clothes.Classy);
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
								if (hasSoap >= 4) {
									ShowBalloon("I already have soap", 2);
									break;
								}
                RemoveMoney(l.price);
                hasSoap = 4;
								Soap.Texture = Soaps[0];
								Soap.Visible = true;
                success = ResultSound.GenericSuccess;
                ShowBalloon("I have some soap now", 2);
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
				if (l.price == 100 && l.amount > 0) { // Harvest
					int got = 0;
					for (int i = 0; i < l.amount; i++) {
						if (!Pickup(PickableItem.Carrot)) break;
						var c = l.GetChild(0) as GrowingCarrot;
						l.carrots.Remove(c);
						c.Free();
						l.amount--;
						got++;
					}
					if (l.amount == 1) ShowBalloon($"I got {got} fresh carrots.\nThere is still 1 carrot to be picked.", 2);
					else if (l.amount > 0) ShowBalloon($"I got {got} fresh carrots.\nThere are still {l.amount} carrots to be picked.", 2);
					else ShowBalloon($"I got {got} fresh carrots.", 2);
					StopEnemies();
					if (l.amount == 0) l.price = 0;
				}
				else if (l.price < 100 && l.amount > 0) { // Tell to wait
					ShowBalloon($"Carrots are not yet grown. They are at {l.price}%", 2);
					success = ResultSound.Failure;
				}
				else if (numCarrots + numRotCarrots == 0) {
					ShowBalloon("I do not have any veggy to plant", 2);
					success = ResultSound.Failure;
				}
				else {
					// Plant carrots: Each carrot will be transformed in 2 planted carrots, each rot carrot in one planted. Max 10 carrots can be planted
					l.amount = numRotCarrots + 2 * numCarrots;
					if (l.amount > 10) l.amount = 10;
					waitUntil = .25 / 24.0;
					if (waitUntil < dayTime) {
						waitUntilNextDay = waitUntil;
						waitUntil = -1;
					}
					InventoryRemoveItem(PickableItem.Carrot);
					numCarrots = 0;
					InventoryRemoveItem(PickableItem.RotCarrot);
					numRotCarrots = 0;
					if (!gardens.Contains(l)) gardens.Add(l);

					var rect = l.GetRect();
					for (int i = 0; i < l.amount; i++) {
						var gc = GrowingCarrotPrefab.Instantiate() as GrowingCarrot;
						l.carrots.Add(gc);
						l.AddChild(gc);
						float x = rect.Position.X + (rect.Size.X - 20) * (i + .5f) * 1f / l.amount + rnd.RandfRange(-8f, 8f);
						float y = rect.Position.Y + rect.Size.Y * .25f + rnd.RandfRange(-5f, 5f);
						gc.Position = new(x, y);
						gc.Scale = new(1f / l.Scale.X, 1f / l.Scale.Y);
					}
					ShowBalloon("Let's wait for them to grow!", 2);
					StopEnemies();
				}
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
				if (isDogPacified) {
					if (!Pickup(PickableItem.Bottle)) {
            GlobalMessage.Text = "I am already full!";
            globalMessageTimeout = 2;
            denial = false;
            doActionDelta = 0;
          }
          else {
						success = ResultSound.GenericSuccess;
						StopEnemies();
					}
				}
				else {
					if (numBones > 0) {
						numBones--;
						InventoryRemoveItem(PickableItem.Bone, true);
						isDogPacified = true;
						throwBone = 1;
						BoneForDog.Visible = true;
						BoneForDog.GlobalPosition = Player.GlobalPosition;
						DumpDog.Frame = 2;
						DumpDog.Visible = true;
						success = ResultSound.GenericSuccess;
						StopEnemies();
					}
					else {
						success = ResultSound.DogBark;
						dogBark = 2.4;
					}
				}
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
							numRotCarrots = 0;
							numBones = 0;
							InventoryRemoveItem(PickableItem.Bottle);
							InventoryRemoveItem(PickableItem.Can);
							InventoryRemoveItem(PickableItem.Paper);
							InventoryRemoveItem(PickableItem.Carrot);
							InventoryRemoveItem(PickableItem.RotCarrot);
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
						Hat.Frame = 2;
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
				if (bodySmell > 10 && hasSoap > 0) {
					hasSoap--;
					switch (hasSoap) {
						case 4: Soap.Texture = Soaps[0]; break;
						case 3: Soap.Texture = Soaps[1]; break;
						case 2: Soap.Texture = Soaps[2]; break;
						case 1: Soap.Texture = Soaps[3]; break;
						default: Soap.Visible = false; break;
					}
          waitUntil = dayTime + .3 / 24.0;
          if (waitUntil >= 1) waitUntil -= 1;
          if (waitUntil < dayTime) {
            waitUntilNextDay = waitUntil;
            waitUntil = -1;
          }
          success = ResultSound.Wash;
					resetPlayer = false;
          bodySmell = 0;
          StopEnemies();
					foundLocation = l;
        }
        else {
					drink += l.amount;
					if (drink > 100) drink = 100;
					ShowBalloon("Refreshing!", 1);
					RemoveMoney(l.price);
					success = ResultSound.Drink;
					StopEnemies();
				}
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

	public void JailTime(string msg) {
		SetRoad(SlumStreet, 2200);
		Player.Position = new(960, 90);
		restingOnBench = false;
		sleepingOnBench = false;
		pickup = false;
		doActionDelta = 0;
		Body.Frame = 0 + GetFitnessLevel();
		Face.Frame = 0;
		Hat.Frame = 0;
    Legs.Frame = 0 + 9 * (int)clothes;
		Player.ZIndex = 30;
		Player.Scale = Vector2.One;
		Player.Rotation = 0;
		Eye.Visible = false;
		Legs.Visible = false;
		jail = true;
		GlobalMessage.Text = msg;
		globalMessageTimeout = 5;
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
		else if (foundLocation.type == LocationType.Hotel || foundLocation.type == LocationType.Apartment) {
			rest = 100;
			drink = 100;
			bodySmell = 0;

			while (numCarrots > 0 && food < 76) {
				numCarrots--;
        InventoryRemoveItem(PickableItem.Carrot, true);
        food += foundLocation.type == LocationType.Hotel ? 25 : 50; // In apartment you gain more
        if (food > 100) food = 100;
      }
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
			ShowBalloon($"I got {FormatMoney(foundLocation.price)}", 2);
		}
		Player.Visible = true;
		ResetPlayer();
		waitUntil = -1;
		foundLocation = null;
		SoundPlayer.Stop();
	}

	public bool IsPlayerReacheable() {
    if (jail || sleeping || hospital || doingGym || restingOnBench || sleepingOnBench || fading) return false;
    if (status != Status.Playing) return false;

    if (foundLocation == null ||
      (foundLocation.type != LocationType.Bench && foundLocation.type != LocationType.Garden && waitUntil != -1 && waitUntilNextDay != -1 && doActionDelta <= 0)) return true;
    return false;
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
		if (wasInsideCrossroad) {
			Player.Position = new(960, 520);
		}
		else {
			Player.Position = new(960, 490);
		}
    Player.ZIndex = 75;
		Player.Scale = Vector2.One;
		Player.Rotation = 0;
		Eye.Visible = false;
		Legs.Visible = true;
		Beard.Frame = Beardlevel(BeardLevels.Walk);
    Beard.Visible = true;
    Player.Visible = true;
		BroomPlayer.Visible = false;
		HandBrooming.Visible = false;
	}

	// Beard:  Walk = 0, Pickup = 1, DenialL = 3, DenialR = 2, Sit = 4

	int Beardlevel(BeardLevels level) {
		if (beard < 20 || goingUp) return 0;
		else if (beard < 40) {
			if (doActionDelta == 0 || sleepingOnBench) return 1;
			if (denial) return (int)level * 4;
			if (status == Status.GameOver || status == Status.Dying) return 1;
			if (pickup) return 4;
			if (restingOnBench) return 16;
		}
		else if (beard < 60) {
			if (doActionDelta == 0 || sleepingOnBench) return 2;
			if (denial) return 1 + (int)level * 4;
			if (status == Status.GameOver || status == Status.Dying) return 2;
			if (pickup) return 5;
			if (restingOnBench) return 17;
		}
		// Default case big beard
		if (denial) return 2 + (int)level * 4;
		if (status == Status.GameOver || status == Status.Dying) return 3;
		if (pickup) return 6;
		if (restingOnBench) return 18;
		if (doActionDelta <= 0 || sleepingOnBench || fading) return 3;

		GlobalMessage.Text = $"Beard level unrecognized";
		globalMessageTimeout = 5;
		return 0;
	}

	void SetClothes(Clothes c) {
		clothes = c;
		switch (clothes) {
			case Clothes.Rags:
				Body.Texture = Body1;
				Hat.Texture = Hat1;
				break;
			case Clothes.Cheap:
        Body.Texture = Body2;
        Hat.Texture = Hat2;
        break;
			case Clothes.Classy:
				Body.Texture = Body3;
        Hat.Texture = Hat3;
        break;
		}
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

	void ProcessBaloons(double delta) {
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

	void ShowMetroMenu() {
		PanelMetro.Visible = true;
		if (currentRoad.Name == RoadName.Main_Street) {
			MetroMain.Visible = false;
		}
		else {
			MetroMain.Visible = true;
			panelMetroSelectedButton = 0;
		}
		if (currentRoad.Name == RoadName.Slum_Street) {
			MetroSlum.Visible = false;
		}
		else {
			MetroSlum.Visible = true;
			panelMetroSelectedButton = 1;
		}
		MetroSide.Visible = currentRoad.Name != RoadName.Side_Road;
		MetroNorth.Visible = currentRoad.Name != RoadName.North_Road;
		MetroTop.Visible = currentRoad.Name != RoadName.Top_Boulevard;
		MetroSuburb.Visible = currentRoad.Name != RoadName.Suburb_Avenue;
	}

	static string FormatTime(int time) {
		if (time == 0) return "Midnight";
		if (time == 12) return "Noon";
		if (time < 12) return $"{time}AM";
		else return $"{time - 12}PM";
	}

	int panelMetroSelectedButton = 0;
	private void HandlePanelMetro(bool left, bool right, bool up, bool down, bool use) {
		if (left || right) {
			PanelMetro.Visible = false;
		}
		else if (up) {
			panelMetroSelectedButton--;
			if ((int)currentRoad.Name == panelMetroSelectedButton) panelMetroSelectedButton--;
			if (panelMetroSelectedButton < 0) panelMetroSelectedButton = 6;
			MetroMain.Theme = panelMetroSelectedButton == 0 ? SelectedButtonTheme : null;
			MetroSlum.Theme = panelMetroSelectedButton == 1 ? SelectedButtonTheme : null;
			MetroSide.Theme = panelMetroSelectedButton == 2 ? SelectedButtonTheme : null;
			MetroNorth.Theme = panelMetroSelectedButton == 3 ? SelectedButtonTheme : null;
			MetroTop.Theme = panelMetroSelectedButton == 4 ? SelectedButtonTheme : null;
			MetroSuburb.Theme = panelMetroSelectedButton == 5 ? SelectedButtonTheme : null;
			MetroClose.Theme = panelMetroSelectedButton == 6 ? SelectedButtonTheme : null;
		}
		else if (down) {
			panelMetroSelectedButton++;
			if ((int)currentRoad.Name == panelMetroSelectedButton) panelMetroSelectedButton++;
			if (panelMetroSelectedButton > 6) panelMetroSelectedButton = 0;
			MetroMain.Theme = panelMetroSelectedButton == 0 ? SelectedButtonTheme : null;
			MetroSlum.Theme = panelMetroSelectedButton == 1 ? SelectedButtonTheme : null;
			MetroSide.Theme = panelMetroSelectedButton == 2 ? SelectedButtonTheme : null;
			MetroNorth.Theme = panelMetroSelectedButton == 3 ? SelectedButtonTheme : null;
			MetroTop.Theme = panelMetroSelectedButton == 4 ? SelectedButtonTheme : null;
			MetroSuburb.Theme = panelMetroSelectedButton == 5 ? SelectedButtonTheme : null;
			MetroClose.Theme = panelMetroSelectedButton == 6 ? SelectedButtonTheme : null;
		}
		else if (use) {
			if (panelMetroSelectedButton == 0) JumpToRoad(0, RoadName.Main_Street, true);
			else if (panelMetroSelectedButton == 1) JumpToRoad(0, RoadName.Slum_Street, true);
			else if (panelMetroSelectedButton == 2) JumpToRoad(0, RoadName.Side_Road, true);
			else if (panelMetroSelectedButton == 3) JumpToRoad(0, RoadName.North_Road, true);
			else if (panelMetroSelectedButton == 4) JumpToRoad(0, RoadName.Top_Boulevard, true);
			else if (panelMetroSelectedButton == 5) JumpToRoad(0, RoadName.Suburb_Avenue, true);
			else if (panelMetroSelectedButton == 6) PanelMetro.Visible = false;
		}
	}

	void JumpToRoad(float x, RoadName r, bool useTicket) {
		fading = true;
		FadePanel.Visible = true;
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
		int num = currentRoad.Name switch {
			RoadName.Slum_Street => 0,
			RoadName.Main_Street => 2,
			RoadName.Side_Road => 2,
			RoadName.Top_Boulevard => 3,
			RoadName.North_Road => 1,
			RoadName.Suburb_Avenue => 1,
			_ => 2
		};
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
		if (!handlingCrossroads) {
			foreach (var npc in toProcess) npc.ProcessNpc(d, Player.GlobalPosition.X, bodySmell + dirtyClothes, Crossroads, currentRoad.Position.X);
			enemy?.ProcessEnemy(d, Crossroads, currentRoad.Position.X);
		}
		if (enemy != null && removeEnemy) {
			removeEnemy = false;
			enemy.Free();
			enemy = null;
		}
	}

	void RemoveAllNPCs() {
		foreach (var npc in NPCs) npc.Delete();
    for (int i = 0; i < npcOrders.Length; i++) npcOrders[i] = false;
    NPCs.Clear();
    numNPCs = 0;
    if (enemy != null) {
			enemy.Free();
			enemy = null;
		}
		removeEnemy = false;
	}

	void SpawnNPC(double delta) {
		if (spawnDelay > 0) {
			spawnDelay -= delta;
			return;
		}

		int expected = NPCsPerDayTime();
		if (numNPCs < expected) {
			int pos = 0;
			for (int i = 0; i < npcOrders.Length; i++) {
				if (!npcOrders[i]) {
					pos = i;
					break;
				}
			}
			if (pos == -1) return; // No free space
			npcOrders[pos] = true;

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
			npc.Spawn(this, currentRoad.PeopleLayer, pos, female);
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
		if (npcForBalloon1 == npc) { NPCBalloon1.Visible = false; npcForBalloon1 = null; }
		if (npcForBalloon2 == npc) { NPCBalloon2.Visible = false; npcForBalloon2 = null; }
	}

	public void GiveToPlayer(int happiness) {
		if (happiness < 4) Pickup(PickableItem.Coin);
		else Pickup(PickableItem.Banknote);
	}

	public void AddDogPoop(float xPos) {
		NPCPlayer.Stream = FartSound;
		NPCPlayer.Play();
		var item = ItemPrefabs[PrefabPoop].Instantiate() as Pickable;
		currentRoad.FrontItems.AddChild(item);
		item.GlobalPosition = new(xPos, rnd.RandfRange(728, 738));
	}


	double enemyDelay = 30;
	EnemyType enemyType = EnemyType.None;
	Enemy enemy = null;
	bool removeEnemy = false;
  Location drunkGuyBench = null;

  void HandleDrunkGuy(double delta) {
    if (drunkGuyBench != null || DrunkGuy.Visible) {
      DrunkGuy.ProcessEnemy(delta, null, 0);
      return;
    }

    // Pick a bench that is far away from the camera
    bool good = false;
    while (!good) {
      drunkGuyBench = Benches[rnd.RandiRange(0, Benches.Length - 1)];
      if (Player.GlobalPosition.DistanceTo(drunkGuyBench.GlobalPosition) > 1000) {
        good = true;
      }
    }
    DrunkGuy.Visible = true;
    DrunkGuy.GetParent()?.RemoveChild(DrunkGuy);
    drunkGuyBench.AddChild(DrunkGuy);
    DrunkGuy.Position = new Vector2(0, -65);
  }


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
			currentRoad.PeopleLayer.AddChild(enemy);
			enemy.Spawn(this, enemyType);
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

  public void SpawnBottlesForDrunkGuy() {
    var frontItems = drunkGuyBench.GetParent().GetParent().FindChild("*FrontItems", false, true);
    float posX = drunkGuyBench.Position.X;

    for (int i = 0; i < 3; i++) {
      Pickable item;
      if (rnd.RandfRange(0, 1f) < .3) item = ItemPrefabs[PrefabCan].Instantiate() as Pickable;
      else item = ItemPrefabs[PrefabBottle].Instantiate() as Pickable;
      frontItems.AddChild(item);
      item.Position = new(posX + rnd.RandfRange(-25f, 25) + i * 75 - 150, rnd.RandfRange(728, 738));
    }
    drunkGuyBench = null;
    DrunkGuy.Visible = false;
    DrunkGuy.GetParent().RemoveChild(DrunkGuy);
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
				ShowBalloon("This guy stole everything I had!\nHe should be kicked out of civilized countries!", 2);
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

	#region Options ******************************************************************************************************************************      [Options]
	public void QuitGame() {
		SaveOptions();
		GetTree().Quit();
	}
	public void PlayGame(int fromWhere) {
		Balloon.Visible = false;
		NPCBalloon1.Visible = false;
		NPCBalloon2.Visible = false;
		SaveOptions();
		if (fromWhere == 0) { // From intro
			ResetAllValues();
			ResetPlayer();
			OptionsPlayButton.Text = "Restart";
			OptionsContinueButton.Disabled = false;
			SetStatus(Status.Starting);
		}
		else if (fromWhere == 1) { // Continue
			if (status == Status.Playing) PanelOptions.Visible = false;
			else {
				ResetAllValues();
				ResetPlayer();
				SetStatus(Status.Starting);
			}
		}
		else if (fromWhere == 2) { // From gameover
			ResetAllValues();
			ResetPlayer();
			OptionsPlayButton.Text = "Restart";
			OptionsContinueButton.Disabled = false;
			SetStatus(Status.Intro);
		}
		PanelOptions.Visible = false;
	}


	void SetMaxFPS(int index) {
		Engine.MaxFps = index switch {
			0 => 0,
			1 => 144,
			2 => 60,
			3 => 50,
			4 => 30,
			_ => 0
		};
	}

	public void ShowOptions() {
		PanelOptions.Visible = true;
	}

	void MusicSliderChange(float _) {
		MusicVolValue.Text = $"{MusicVolSlider.Value:N0}%";
		MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
	}
	void EffectsSliderChange(float _) {
		EffetsVolValue.Text = $"{EffetsVolSlider.Value:N0}%";
		SoundPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
		PlayerPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
		NPCPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
	}
	void GameSpeedSliderChange(float _) {
		GameSpeedValue.Text = (int)GameSpeedSlider.Value switch {
			0 => "Frozen",
			1 => "Very slow",
			2 => "Slow",
			3 => "Chilly",
			4 => "Normal",
			5 => "Rush",
			6 => "Speedy",
			7 => "Fast",
			8 => "Flash",
			9 => "Insane",
			_ => "Normal",
		};
		gameSpeed = (int)GameSpeedSlider.Value switch {
			0 => .1,
			1 => .25,
			2 => .5,
			3 => .8,
			4 => 1,
			5 => 1.1,
			6 => 1.2,
			7 => 1.5,
			8 => 2,
			9 => 5,
			_ => 1,
		};
	}
	void GameDifficultySliderChange(float _) {
		gameDifficulty = (int)GameDifficultySlider.Value;
		GameDifficultyValue.Text = gameDifficulty switch {
			0 => "Shevchenko",
			1 => "Shmyhal",
			2 => "Galushchenko",
			3 => "Kuleba",
			4 => "Umerov",
			5 => "Kamyshin",
			6 => "Budanov",
			7 => "Syrskyi",
			8 => "Zaluzhny",
			9 => "Zelenskyy",
			_ => "Kamyshin",
		};
	}


	int winWidth = 0, winHeight;
	void ChangeFullscreen(int index) {
		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed) {
			var wsize = DisplayServer.WindowGetSize(0);
			winWidth = wsize.X; winHeight = wsize.Y;
		}

		DisplayServer.WindowSetMode(index == 0 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed && winWidth != 0) {
			DisplayServer.WindowSetSize(new(winWidth, winHeight), 0);
		}
	}
	void SwitchFullscreen() {
		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed) {
			var wsize = DisplayServer.WindowGetSize(0);
			winWidth = wsize.X; winHeight = wsize.Y;
		}

		DisplayServer.WindowSetMode(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);

		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed && winWidth != 0) {
			DisplayServer.WindowSetSize(new(winWidth, winHeight), 0);
		}

		DropDownFullscreen.Selected = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed ? 1 : 0;
	}

	public void UseHatForPlayer(bool pressed) {
		Hat.Visible = pressed;
  }


	void SaveOptions() {
		config.SetValue("Sounds", "MusicVolume", (int)MusicVolSlider.Value);
		config.SetValue("Sounds", "SoundsVolume", (int)EffetsVolSlider.Value);
		config.SetValue("Graphics", "Fullscreen", (int)(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ? 1 : 0));
		if (winWidth != 0) {
			config.SetValue("Graphics", "WinWidth", winWidth);
			config.SetValue("Graphics", "WinHeight", winHeight);
		}
		config.SetValue("Graphics", "MaxFPS", Engine.MaxFps switch {
			0 => 0,
			144 => 1,
			60 => 2,
			50 => 3,
			30 => 4,
			_ => 0
		});
    config.SetValue("Game", "Speed", gameSpeed);
		config.SetValue("Game", "Difficulty", gameDifficulty);
		config.SetValue("Game", "UseHatForPlayer", Hat.Visible);
		config.SetValue("Game", "UseCustomMusic", UseCustomMusicCB.ButtonPressed);
		config.Save("user://R2R.cfg");
	}

	void GameSpeed(int speed) {
		userGameSpeed = speed switch {
			0 => 0,
			1 => 1,
			2 => 2,
			3 => 5,
			_ => 1
		};

		switch (userGameSpeed) {
			case 0:
				SpeedButton0.ButtonPressed = true;
				SpeedButton1.ButtonPressed = false;
				SpeedButton2.ButtonPressed = false;
				SpeedButton3.ButtonPressed = false;
				break;
			case 1:
				SpeedButton0.ButtonPressed = false;
				SpeedButton1.ButtonPressed = true;
				SpeedButton2.ButtonPressed = false;
				SpeedButton3.ButtonPressed = false;
				break;
			case 2:
				SpeedButton0.ButtonPressed = false;
				SpeedButton1.ButtonPressed = false;
				SpeedButton2.ButtonPressed = true;
				SpeedButton3.ButtonPressed = false;
				break;
			case 5:
				SpeedButton0.ButtonPressed = false;
				SpeedButton1.ButtonPressed = false;
				SpeedButton2.ButtonPressed = false;
				SpeedButton3.ButtonPressed = true;
				break;
		}
	}


	double optionsClickDelta = 0;
	void HandleMenuActions(bool joyStart, double delta) {
		if (Input.IsActionJustPressed("Esc") || (joyStart && !joyJustPressed)) {
			joyJustPressed = true;
			PanelOptions.Visible = false;
			return;
		}

		// Depending on the current selected node act accordingly
		int sel = -1;
		for (int i = 0; i < HighlightOpts.Length; i++) {
			if (HighlightOpts[i].Visible) {
				sel = i;
				break;
			}
		}

		float jx = Input.GetJoyAxis(0, JoyAxis.LeftX);
		float jy = Input.GetJoyAxis(0, JoyAxis.LeftY);
		bool up = (Input.IsKeyPressed(Key.Up) || Input.IsPhysicalKeyPressed(Key.W) || Input.IsJoyButtonPressed(0, JoyButton.DpadUp)) || jy < -.5f;
		bool down = (Input.IsKeyPressed(Key.Down) || Input.IsPhysicalKeyPressed(Key.S) || Input.IsJoyButtonPressed(0, JoyButton.DpadDown)) || jy > .5f;
		bool left = (Input.IsKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.A) || Input.IsJoyButtonPressed(0, JoyButton.DpadLeft)) || jx < -.5f;
		bool right = (Input.IsKeyPressed(Key.Right) || Input.IsPhysicalKeyPressed(Key.D) || Input.IsJoyButtonPressed(0, JoyButton.DpadRight)) || jx > .5f;
		bool use = (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A));

		optionsClickDelta += delta;
		double delay;
		if (use) delay = .5;
		else if (up || down) delay = .25;
		else if (sel == 3 || sel == 4 || sel == 7) delay = .4;
		else delay = .05;
		if (optionsClickDelta < delay) return;

		if (!up && !down && !left && !right && !use) return;
		optionsClickDelta = 0;

		if (down) {
			if (sel == -1) sel = HighlightOpts.Length - 1;
			HighlightOpts[sel].Visible = false;
			sel++;
			if (sel >= HighlightOpts.Length) sel = 0;
			HighlightOpts[sel].Visible = true;
		}
		if (up) {
			if (sel == -1) sel = 0;
			HighlightOpts[sel].Visible = false;
			sel--;
			if (sel < 0) sel = HighlightOpts.Length - 1;
			HighlightOpts[sel].Visible = true;
		}
		if (left && (sel == 0 || sel == 1 || sel == 3 || sel == 4 || sel == 7)) {
			if (sel == 0) {
				MusicVolSlider.SetValueNoSignal(MusicVolSlider.Value - 1);
				MusicVolValue.Text = $"{MusicVolSlider.Value:N0}%";
				MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
			}
			else if (sel == 1) {
				EffetsVolSlider.SetValueNoSignal(EffetsVolSlider.Value - 1);
				EffetsVolValue.Text = $"{EffetsVolSlider.Value:N0}%";
				SoundPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
				PlayerPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
				NPCPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
			}
			else if (sel == 3) {
				GameSpeedSlider.SetValueNoSignal(GameSpeedSlider.Value - 1);
				gameSpeed = (int)GameSpeedSlider.Value switch {
					0 => .1,
					1 => .25,
					2 => .5,
					3 => .8,
					4 => 1,
					5 => 1.1,
					6 => 1.2,
					7 => 1.5,
					8 => 2,
					9 => 5,
					_ => 1,
				};
				GameSpeedValue.Text = (int)GameSpeedSlider.Value switch {
					0 => "Frozen",
					1 => "Very slow",
					2 => "Slow",
					3 => "Chilly",
					4 => "Normal",
					5 => "Rush",
					6 => "Speedy",
					7 => "Fast",
					8 => "Flash",
					9 => "Insane",
					_ => "Normal",
				};
			}
			else if (sel == 4) {
				GameDifficultySlider.SetValueNoSignal(GameDifficultySlider.Value - 1);
				gameDifficulty = (int)GameDifficultySlider.Value;
				GameDifficultyValue.Text = gameDifficulty switch {
					0 => "Shevchenko",
					1 => "Shmyhal",
					2 => "Galushchenko",
					3 => "Kuleba",
					4 => "Umerov",
					5 => "Kamyshin",
					6 => "Budanov",
					7 => "Syrskyi",
					8 => "Zaluzhny",
					9 => "Zelenskyy",
					_ => "Kamyshin",
				};
			}
			else if (sel == 7) {
				var index = DropDownFPS.Selected;
				index--;
				if (index<0) index = 4;
				DropDownFPS.Selected = index;
        SetMaxFPS(index);
			}
			SaveOptions();
		}
		if (right && (sel == 0 || sel == 1 || sel == 3 || sel == 4 || sel == 7)) {
			if (sel == 0) {
				MusicVolSlider.SetValueNoSignal(MusicVolSlider.Value + 1);
				MusicVolValue.Text = $"{MusicVolSlider.Value:N0}%";
				MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
			}
			else if (sel == 1) {
				EffetsVolSlider.SetValueNoSignal(EffetsVolSlider.Value + 1);
				EffetsVolValue.Text = $"{EffetsVolSlider.Value:N0}%";
				SoundPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
				PlayerPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
				NPCPlayer.VolumeDb = (float)(.5 * EffetsVolSlider.Value - 40);
			}
			else if (sel == 3) {
				GameSpeedSlider.SetValueNoSignal(GameSpeedSlider.Value + 1);
				gameSpeed = (int)GameSpeedSlider.Value switch {
					0 => .1,
					1 => .25,
					2 => .5,
					3 => .8,
					4 => 1,
					5 => 1.1,
					6 => 1.2,
					7 => 1.5,
					8 => 2,
					9 => 5,
					_ => 1,
				};
				GameSpeedValue.Text = (int)GameSpeedSlider.Value switch {
					0 => "Frozen",
					1 => "Very slow",
					2 => "Slow",
					3 => "Chilly",
					4 => "Normal",
					5 => "Rush",
					6 => "Speedy",
					7 => "Fast",
					8 => "Flash",
					9 => "Insane",
					_ => "Normal",
				};
			}
			else if (sel == 4) {
				GameDifficultySlider.SetValueNoSignal(GameDifficultySlider.Value + 1);
				gameDifficulty = (int)GameDifficultySlider.Value;
				GameDifficultyValue.Text = gameDifficulty switch {
					0 => "Shevchenko",
					1 => "Shmyhal",
					2 => "Galushchenko",
					3 => "Kuleba",
					4 => "Umerov",
					5 => "Kamyshin",
					6 => "Budanov",
					7 => "Syrskyi",
					8 => "Zaluzhny",
					9 => "Zelenskyy",
					_ => "Kamyshin",
				};
			}
      else if (sel == 7) {
        var index = DropDownFPS.Selected;
				index++;
        if (index > 4) index = 0;
        DropDownFPS.Selected = index;
        SetMaxFPS(index);
      }
      SaveOptions();
		}

		if (use && sel == 2) {
			SwitchFullscreen();
			SaveOptions();
		}

		if (use && sel == 5) {
			UsePlayerHatCB.SetPressedNoSignal(!UsePlayerHatCB.ButtonPressed);
			UseHatForPlayer(UsePlayerHatCB.ButtonPressed);
		}
		if (use && sel == 6) {
			UseCustomMusicCB.ButtonPressed = false;
		}

    if (use && sel == 7) {
			var index = DropDownFPS.Selected;
			index++;
			if (index > 4) index = 0;
			DropDownFPS.Selected = index;
      SetMaxFPS(index);
      SaveOptions();
    }

    if (use && sel == 8) {
			PlayGame(0);
		}
		if (use && sel == 9) {
			PlayGame(1);
		}

		if (use && sel == 10) {
			QuitGame();
		}
	}





	#endregion Options ********************************************************************************************************************** ^Options^

	#region Intro ********************************************************************************************************************************      [Intro]

	[ExportGroup("Intro")]
	[Export] CanvasLayer IntroPanel;
	[Export] CanvasLayer GameOverPanel;
	[Export] CanvasLayer WinPanel;
	[Export] AnimationPlayer GameOverAnim;
	[Export] AnimationPlayer WinAnim;
	[Export] RichTextLabel IntroText;

	double introTextScrollPos = 400;

	void ShowIntro(double delta) {
		introTextScrollPos -= delta * 40;
		if (introTextScrollPos < -3100) introTextScrollPos = 400;
		IntroText.Position = new(0, (float)introTextScrollPos);
		if (doActionDelta > 0) {
			doActionDelta -= delta;
			if (doActionDelta <= 0) {
        MusicPlayer.VolumeDb = (float)(.5 * MusicVolSlider.Value - 40);
        MusicPlayer.Stream = MusicC64;
				MusicPlayer.Play();
			}
		}
		else if (!HighlightIntro[0].Visible && !HighlightIntro[1].Visible && !HighlightIntro[2].Visible && !HighlightIntro[3].Visible &&
			(Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A))) {
			SetStatus(Status.Starting);
		}
	}

	void HandleIntroActions(double delta) {
		// Depending on the current selected node act accordingly
		int sel = -1;
		for (int i = 0; i < HighlightIntro.Length; i++) {
			if (HighlightIntro[i].Visible) {
				sel = i;
				break;
			}
		}

		float jx = Input.GetJoyAxis(0, JoyAxis.LeftX);
		float jy = Input.GetJoyAxis(0, JoyAxis.LeftY);
		bool up = (Input.IsKeyPressed(Key.Up) || Input.IsPhysicalKeyPressed(Key.W) || Input.IsJoyButtonPressed(0, JoyButton.DpadUp)) || jy < -.5f;
		bool down = (Input.IsKeyPressed(Key.Down) || Input.IsPhysicalKeyPressed(Key.S) || Input.IsJoyButtonPressed(0, JoyButton.DpadDown)) || jy > .5f;
		bool use = (Input.IsKeyPressed(Key.Enter) || Input.IsPhysicalKeyPressed(Key.Ctrl) || Input.IsJoyButtonPressed(0, JoyButton.A));

		optionsClickDelta += delta;
		double delay;
		if (up || down) delay = .25;
		else if (sel == 3 || sel == 4) delay = .4f;
		else delay = .05;
		if (optionsClickDelta < delay) return;

		if (!up && !down && !use) return;
		optionsClickDelta = 0;

		if (down) {
			if (sel == -1) sel = HighlightIntro.Length - 1;
			HighlightIntro[sel].Visible = false;
			sel++;
			if (sel >= HighlightIntro.Length) sel = 0;
			HighlightIntro[sel].Visible = true;
		}
		if (up) {
			if (sel == -1) sel = 0;
			HighlightIntro[sel].Visible = false;
			sel--;
			if (sel < 0) sel = HighlightIntro.Length - 1;
			HighlightIntro[sel].Visible = true;
		}

		if (!use) return;
		switch (sel) {
			case 0: PlayGame(0); break;
			case 1: ShowOptions(); break;
			case 2: StartHelp(); break;
			case 3: QuitGame(); break;
		}
	}

  public void StartHelp() {
    GetTree().ChangeSceneToFile("res://Prefabs/Help.tscn");
  }

  #endregion Intro ********************************************************************************************************************** ^Intro^


}

public enum Clothes {
	Rags = 0, Cheap = 1, Classy = 2
}

public enum ResultSound {
	None, GenericSuccess, Failure, Eat, Drink, EatAndDrink, Toilet, Laundry, Broom, ATM, Sleep, Work1, Work2, Work3, Study,
	DogBark,
	Workout, Wash
}

/*
 Inventory

Have known size for all object types
Have a list, somewhere, of the items picked up
Before adding a new item, check if it will fit

 
 
 */

