call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\VsDevCmd.bat"
taskkill /im ""
msbuild Autodash.sln /target:rebuild /p:Configuration=Debug
src\Autodash.ConsoleHost\bin\Debug\Autodash.ConsoleHost.exe