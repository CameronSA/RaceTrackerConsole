cd ../
set /p hours=Enter number of hours to mine data: 
RaceTrackerConsole.exe -minedatafor %hours%
RaceTrackerConsole.exe -processdata -all
RaceTrackerConsole.exe -processdata -move compiled_data.csv
@pause