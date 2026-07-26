using System.Reflection;
using Xitira.GamePadTester;

using var game = new GamePadTester();
LiteDbContent.LauncherAssembly = Assembly.GetExecutingAssembly();

game.Run();