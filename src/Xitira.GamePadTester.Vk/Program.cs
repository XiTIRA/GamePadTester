using System.Reflection;
using Xitira.GamePadTester;

using var game = new Xitira.GamePadTester.GamePadTester();
LiteDbContent.LauncherAssembly = Assembly.GetExecutingAssembly();

game.Run();