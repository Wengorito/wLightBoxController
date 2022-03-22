# Project name
>wLightBoxController - the wirelesss LED controller library

## Table of Contents
* [General information](#general-information)
* [Technologies used](#technologies-used)
* [Setup](#setup)
* [Author](#author)

## General information
This project has been created in order to get a basic grip on the API and JSON usage as well as general good coding craft. It is based on officialy avaialble API providing communication with BleBox devices with API type: wLightBox (in this particular case the wireless LED controller wLightBox v3 hardware).
The project consists of reusable class library LightBoxController performing basic HTTP requests on the host device and exemplary user interface LightBoxGUI presenting the proper usage of the library.
https://technical.blebox.eu/openapi_wlightbox/openAPI_wLightBox_20190808.html

## Technologies used
The whole project has been created using the .NET 5 target framework and Windows Presentation Foundation.
NuGet packages reference:

### LightBoxController:
- ObjectDumper.NET v3.3.13
- System.Text.Json v6.0.2

### LightBoxGUI
- Extended.Wpf.Toolkit v4.2.0

### LightBoxController.Tests
- Moq v4.17.2
- xunit v2.4.1
- xunit.runner.visualstudio v2.4.3
- coverlet.collector v3.0.2


## Setup
To run this project clone it locally and open with Visual Studio (preferably 2019 or newer). LightBoxGUI should be set as the startup project. Compile and run. 
For now, you shall use the device's internal access point (something like 192.168.4.1) 
to which you have to connect your PC or use the local WiFi provided that it has been preconfigured in the hardware.

## Author
Grzegorz Wengorz Inc.