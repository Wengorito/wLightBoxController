# Project name
>wLightBoxController - the wirelesss LED controller library

## Table of Contents
* [General Info](#general-information)
* [Technologies(#technologies)
* [Setup](#setup)
* [Author](#author)

## General info

This project has been created in order to get a basic grip on the API and JSON usage as well as general good coding craft.
It is based on officialy avaialble API providing communication with BleBox devices with API type: wLightBox 
- in this particular case the wireless LED controller wLightBox (v2/v3) hardware.
The project consists of reusable class library LightBoxController performing basic HTTP requests on the host device 
and exemplary user interface LightBoxGUI presenting the proper usage of the library.
https://technical.blebox.eu/openapi_wlightbox/openAPI_wLightBox_20190808.html

## Technologies

The whole project has been created using the .NET 5 target framework and Windows Presentation Foundation

NuGet packages reference:
LightBoxController:
    "ObjectDumper.NET" Version="3.3.13"
    "System.Text.Json" Version="6.0.2"

LightBoxGUI
    "Extended.Wpf.Toolkit" Version="4.2.0"

LightBoxController.Tests
    "Microsoft.NET.Test.Sdk" Version="16.9.4"
    "Moq" Version="4.17.2"
    "xunit" Version="2.4.1"
    "xunit.runner.visualstudio" Version="2.4.3"
    "coverlet.collector" Version="3.0.2"


## Setup

To run this project clone it locally and open with Visual Studio (preferably 2019 or newer). LightBoxGUI should be set as the startup project. Compile and run. 
For now, you shall use the device's internal access point (something like 192.168.4.1) 
to which you have to connect your PC or use the local WiFi provided that it has been preconfigured in the hardware

Author:
Grzegorz Wengorz Inc.