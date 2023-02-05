using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Project_Phoenix_Launcher_MAUI_Port;

class Program : MauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	static void Main(string[] args)
	{
		var app = new Program();
		app.Run(args);
	}
}
