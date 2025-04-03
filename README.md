# Content

* Authors and contributors
* Intorduction
* Project structure
* Project dependencies
* Fault localization startup

# Authors and contributors

The main contributor Egor O. Korovin., Student of SPbPU ICSC.
The advisor and contributor Vladimir A. Parkhomenko., Seniour Lecturer of SPbPU ICSC.

# Introduction

This is a project of SBFL (spectrum-based fault localization) in "tcas" program of Siemens Suite. Localization is being made with nearest neighbour query using by 2 kinds of spectra: binary coverage and permutation.

The project is completed during the preparation of Egor O. Korovin work under Testing of software at SPbPU Institute of Computer Science and Cybersecurity (SPbPU ICSC).

# Project structure

Contents:
* Siemens suite: original "tcas" program, its versions, and test suite;
* FaultLocalizationNN: a C# program that implements nearest neighbour query fault localzation;
* Results: folder for the results of the fault localiztion;
* runfl.bat: batch file for starting the fault localization program on "tcas" program and writing the results into Results folder.

# Project dependencies

For correct work and compilation of the program requirements are:
* Windows 10 (compatibility with other versions are not guaranteed);
* .NET 6.0 (https://dotnet.microsoft.com//) is required.

# Fault localization startup

Start runfl.bat batch file to start fault localization. After completion, the results of the fault localization using 2 different types of spectra will be located in Results folder.
