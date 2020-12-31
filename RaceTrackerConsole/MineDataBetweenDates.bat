cd ../
set /p startdate=Enter start date (yyyy-MM-dd): 
set /p enddate=Enter end date (yyyy-MM-dd):
RaceTrackerConsole.exe -minedata %startdate% %enddate%
RaceTrackerConsole.exe -processdata -all
RaceTrackerConsole.exe -processdata -move compiled_data.csv
@pause 