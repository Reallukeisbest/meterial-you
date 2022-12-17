---
layout: ../layouts/UsageLayout.astro
---
# Installation
To download the latest version of Meterial You, you can use the download button below [(or click here)](/download):  
<button style="margin-top: 45px;">Download</button>

# Tokens
Color tokens are defined dynamic colors in a skin. This means that, when the accent color changes, these colors change too. In order to get these token names, you can use the [Colors](/color-picker) skin.  

# Usage
Meterial You is very easy to use. All that you have to do is to import the plugin and set a token:
```ini
[Primary]
Measure=Plugin
Plugin=Empty
Token=Primary
```
To add background opacity, just add a comma, then the opacity value:
```ini
[MeterText]
Meter=String
FontColor=[Primary], 100
FontSize=50
Text=This is an example of a primary color with opacity
DynamicVariables=1
```
Something important to remember is to add `DynamicVariables=1` to any meter that will be using a Meterial You color.  
Here is an example of a full skin:
```ini
[Rainmeter]
Author=Realluke

[Primary]
Measure=Plugin
Plugin=Empty
Token=Primary

[Secondary]
Measure=Plugin
Plugin=Empty
Token=Secondary

[MeterText]
Meter=String
FontColor=[Primary]
FontSize=50
Text=This is your primary color: [Primary]
DynamicVariables=1

[MeterText2]
Meter=String
Y=100
FontColor=[Secondary]
FontSize=50
Text=This is your secondary color: [Secondary]
DynamicVariables=1
```
Its as easy as that!