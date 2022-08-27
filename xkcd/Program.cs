// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Raylib_cs;
using System.Net;
using System.Numerics;
using System.Net.NetworkInformation;

bool wifi = true;
try {
	wifi = new Ping().Send("www.google.com.mx").Status == IPStatus.Success;
}  catch(Exception e){
	wifi = false;
}

var wc = new WebClient();
int currentAmount;
Image? image = null;
Texture2D? texture = null;
XKCDInfo currentInfo;

if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/comics/"))
	Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/comics/");

void LoadNewComic(int num)
{
	
	if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num))
	{
		Console.WriteLine("Downloading comic " + num);
		Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num);
		string infoString = wc.DownloadString("https://xkcd.com/" + num + "/info.0.json");
		File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/info.json", infoString);
		currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(infoString);
		wc.DownloadFile(currentInfo.img, AppDomain.CurrentDomain.BaseDirectory + "/comics/" + currentInfo.num + "/comic.png");
	}
	else
	{
		Console.WriteLine("Loading comic " + num);
		currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/info.json"));
	}

	if(image != null)
		Raylib.UnloadImage((Image)image);
	if(texture != null)
		Raylib.UnloadTexture((Texture2D)texture);

	image = Raylib.LoadImage(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/comic.png");

	Raylib.SetWindowSize(((Image)image).width, ((Image)image).height);
	Raylib.SetWindowTitle($"xkcd - {currentInfo.safe_title}[{currentInfo.num}] {currentInfo.day}/{currentInfo.month}-{currentInfo.year}");

	texture = Raylib.LoadTextureFromImage((Image)image);
}

const bool SHOULD_DISPLAY_ALT = false;

if(wifi){
	currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(wc.DownloadString("https://xkcd.com/info.0.json"));
	currentAmount = currentInfo.num;
} else{
	string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "/comics/");
	if(dirs.Length <= 0)
		return;
	
	List<int> downloaded = new List<int>();
	foreach(var d in dirs) downloaded.Add(int.Parse(Path.GetFileNameWithoutExtension(d)));
	int max = -1;
	for(int i = 0; i < downloaded.Count; i++){
		if(downloaded[i] > max)
			max = downloaded[i];
	}
	currentAmount = max;
	currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + max + "/info.json"));
}

Raylib.InitWindow(10, 10, "xkcd");
LoadNewComic(currentAmount);
Raylib.SetTargetFPS(30);

float timeSinceLastMouseMove = 0f;
int lastMouseX = Raylib.GetMouseX();
int lastMouseY = Raylib.GetMouseY();

while (!Raylib.WindowShouldClose())
{
	if (SHOULD_DISPLAY_ALT)
	{
		if (lastMouseX == Raylib.GetMouseX() && lastMouseY == Raylib.GetMouseY())
			timeSinceLastMouseMove += Raylib.GetFrameTime();
		else if (timeSinceLastMouseMove > 0.5)
		{
			timeSinceLastMouseMove = 0;
			Raylib.SetWindowTitle(
				$"xkcd - {currentInfo.safe_title}[{currentInfo.num}] {currentInfo.day}/{currentInfo.month}-{currentInfo.year}");
		}
		else
			timeSinceLastMouseMove = 0;
		lastMouseX = Raylib.GetMouseX();
		lastMouseY = Raylib.GetMouseY();
	}


	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.RED);
	
	if(texture != null)
		Raylib.DrawTexture((Texture2D)texture, 0, 0, Color.WHITE);
	if (SHOULD_DISPLAY_ALT && currentInfo != null && timeSinceLastMouseMove > 0.5f)
	{
		Raylib.DrawRectangle(lastMouseX, lastMouseY - 10, Raylib.MeasureText(currentInfo.alt, 5), 10, Color.WHITE);
		Raylib.DrawText(currentInfo.alt, lastMouseX, lastMouseY - 10, 5, Color.BLACK);
		Raylib.SetWindowTitle(currentInfo.alt);
	}

	Raylib.EndDrawing();

	if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
	{
		if(wifi)
			LoadNewComic(Random.Shared.Next(1, currentAmount));
		else{
			string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "/comics/");
			if(dirs.Length <= 0){
				Raylib.CloseWindow();
			} else
				LoadNewComic(int.Parse(Path.GetFileNameWithoutExtension(dirs[Random.Shared.Next(0, dirs.Length)])));
		}
			
		//Random.Shared.Next(1, amt)
	}
}

Raylib.CloseWindow();


class XKCDInfo
{
	public string month = "";
	public int num = 0;
	public string link = "";
	public string year = "";
	public string news = "";
	public string safe_title = "";
	public string transcript = "";
	public string alt = "";
	public string img = "";
	public string title = "";
	public string day = "";
}