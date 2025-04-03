@echo off

set INITIAL_DIR=%cd%
set RESULTS_DIR="%cd%\Results"
set FLNN_DIR="%cd%\FaultLocalizationNN\FaultLocalizationNN\FaultLocalizationNN\bin\Debug\net6.0"
set FLPBW_DIR="%cd%\FaultLocalizationPBW\bin"

echo Abbreviations:
echo FLNN-B -- fault localization with nearest neighbour queries (binary coverage spectra)
echo FLNN-P -- fault localization with nearest neighbour queries (permutation spectra)
echo.

echo Started FLNN-B with logging...
cd %FLNN_DIR%
FaultLocalizationNN.exe --binary > %RESULTS_DIR%\FaultLocalizationNN.BinaryCoverageSpectra.txt
echo Done FLNN-B
echo.

echo Started FLNN-P with logging...
FaultLocalizationNN.exe --permutation > %RESULTS_DIR%\FaultLocalizationNN.PermutationSpectra.txt
echo Done FLNN-P
echo.

echo Fault localization done, all results logged into %RESULTS_DIR%
echo.

cd %INITIAL_DIR%

pause