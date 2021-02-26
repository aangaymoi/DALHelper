mode 800

for /f "tokens=1 delims=" %%a in ('dir /b /s *.sln') do (
	call "d:\06. Tools\RandomDateTime.exe"
	"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe" /m "%%a" /t:rebuild /p:Configuration=Release
	call "%%a.clean.bat"
)

call "d:\06. Tools\SyncTime.exe"

@PAUSE