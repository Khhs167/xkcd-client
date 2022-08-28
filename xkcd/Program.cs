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
int currentAmount = 1;
Texture2D? texture = null;
XKCDInfo currentInfo;

if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/comics/"))
	Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/comics/");

void GetComicMax()
{
	if (wifi)
	{
		currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(wc.DownloadString("https://xkcd.com/info.0.json"));
		currentAmount = currentInfo.num;
	}
	else
	{
		string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "/comics/");
		if (dirs.Length <= 0)
			return;

		List<int> downloaded = new List<int>();
		foreach (var d in dirs) downloaded.Add(int.Parse(Path.GetFileNameWithoutExtension(d)));
		int max = -1;
		for (int i = 0; i < downloaded.Count; i++)
		{
			if (downloaded[i] > max)
				max = downloaded[i];
		}
		currentAmount = max;
	}
}

void LoadNewComic(int num)
{
	if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num))
	{
		Console.WriteLine("Downloading comic " + num);
		Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num);
		string infoString;
		if (num == 404)
		{
			infoString = wc.DownloadString("https://raw.githubusercontent.com/Khhs167/xkcd-client/master/comic404/info.0.json");
			File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/info.json", infoString);
			currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(infoString);
			string extention = Path.GetExtension(currentInfo.img);
			wc.DownloadFile(currentInfo.img, AppDomain.CurrentDomain.BaseDirectory + "/comics/" + currentInfo.num + "/comic" + extention);
			
		}
		else
		{
			infoString = wc.DownloadString("https://xkcd.com/" + num + "/info.0.json");
			File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/info.json", infoString);
			currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(infoString);
			string extention = Path.GetExtension(currentInfo.img);
			wc.DownloadFile(currentInfo.img, AppDomain.CurrentDomain.BaseDirectory + "/comics/" + currentInfo.num + "/comic" + extention);
		}
	}
	else
	{
		Console.WriteLine("Loading comic " + num);
		currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/info.json"));
	}
	if(texture != null)
		Raylib.UnloadTexture((Texture2D)texture);
	string loadImageEx = Path.GetExtension(currentInfo.img);

	texture = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + num + "/comic" + loadImageEx);

	if (texture.Value.id <= 0)
	{
		Console.Error.WriteLine("Could not load comic!");
		LoadNewComic(404);
	}
	
	Raylib.SetWindowSize(texture.Value.width, texture.Value.height);
	Raylib.SetWindowTitle($"xkcd - {currentInfo.safe_title}[{currentInfo.num}] {currentInfo.day}/{currentInfo.month}-{currentInfo.year}");
}

const bool SHOULD_DISPLAY_ALT = false;

Raylib.InitWindow(1280, 720, "xkcd client");

GetComicMax();
if (args.Length == 1)
{
	if (args[0] == "download")
	{
		Raylib.CloseWindow();
		if (!wifi)
		{
			Console.Error.WriteLine("Cannot download all comics: No wifi");
			return;
		}

		for (int i = 1; i < currentAmount; i++)
		{
			if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + i))
			{

				Console.WriteLine("Downloading comic " + i + " [" + MathF.Round((float)i / (float)currentAmount * 100, 2) + "%]");
				Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + i);
				string infoString;
				if (i == 404)
				{
					infoString = wc.DownloadString("https://raw.githubusercontent.com/Khhs167/xkcd-client/master/comic404/info.0.json");
					File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + i + "/info.json", infoString);
					currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(infoString);
					wc.DownloadFile(currentInfo.img,
						AppDomain.CurrentDomain.BaseDirectory + "/comics/" + currentInfo.num + "/comic.png");
					continue;
				}
				infoString = wc.DownloadString("https://xkcd.com/" + i + "/info.0.json");
				File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/comics/" + i + "/info.json", infoString);
				currentInfo = JsonConvert.DeserializeObject<XKCDInfo>(infoString);
				string extention = Path.GetExtension(currentInfo.img);
				wc.DownloadFile(currentInfo.img,
					AppDomain.CurrentDomain.BaseDirectory + "/comics/" + currentInfo.num + "/comic" + extention);
			}
		}
		return;

	}
	Console.WriteLine(args[0]);
	LoadNewComic(int.Parse(args[0]));
}
else
{
	LoadNewComic(currentAmount);
}

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
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.WHITE);
		Raylib.DrawText("Loading comic...", 5, 5, 20, Color.BLACK);
		Raylib.EndDrawing();
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