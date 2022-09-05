@SET EXCEL_FOLDER=xls
@SET JSON_FOLDER=..\..\TResources\Config
@SET EXE= Tools\excel2json\excel2json.exe
@SET CsharpPath=..\..\ConfigStruct

@ECHO Del old Config...
del %JSON_FOLDER% /S /Q
del %CsharpPath% /S /Q 

@ECHO Converting excel files in folder %EXCEL_FOLDER% ...
for /f "delims=" %%i in ('dir /b /a-d /s %EXCEL_FOLDER%\*.xlsx') do (
    @echo   processing %%~nxi 
    @CALL %EXE% --excel %EXCEL_FOLDER%\%%~nxi --json %JSON_FOLDER%\%%~ni.json --p %CsharpPath%\%%~ni.cs --header 3 --cell_json true --a --exclude_prefix #
)
pause