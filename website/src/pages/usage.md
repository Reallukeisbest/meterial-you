---
layout: ../layouts/UsageLayout.astro
---
# Installation
To download the latest version of Meterial You, you can use the download button below [(or click here)](https://github.com/Reallukeisbest/meterial-you/releases):  
[<button style="margin-top: 45px;">Download</button>](https://github.com/Reallukeisbest/meterial-you/releases)

# Tokens
Color tokens are defined dynamic colors in a skin. This means that, when the accent color changes, these colors change too. In order to get these token names, you can use the [Colors](https://github.com/Reallukeisbest/meterial-you/blob/main/example-skins/Colors.ini) skin.  

# Usage
Meterial You is very easy to use. All that you have to do is to import the plugin and call the function:
```ini
[Rainmeter]
Author=Realluke
Update=5000

[MeterialYou]
Measure=Plugin
Plugin=MeterialYou

[MeterText1]
Meter=String
FontColor=[&MeterialYou:GetToken("Primary")]
FontSize=50
Text=This is an example of a primary color
DynamicVariables=1
```
Something important to remember is to add `DynamicVariables=1` to any meter that will be using a Meterial You color. 
When the accent color changes, the skin will have to be reloaded.
To add background opacity, just add a comma, then the opacity value:
```ini
[MeterText]
Meter=String
FontColor=[&MeterialYou:GetToken("Primary")], 100
FontSize=50
Text=This is an example of a primary color with opacity
DynamicVariables=1
```
Its as easy as that!
