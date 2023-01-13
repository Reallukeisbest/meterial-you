using System;
using System.Runtime.InteropServices;
using Rainmeter;
using Microsoft.Win32;
using MaterialColorUtilities.Schemes;
using MaterialColorUtilities.Palettes;
using System.Collections.Generic;
using MaterialColorUtilities.Utils;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace MeterialYouPlugin
{
    internal class Measure
    {
        public static uint GetAccentColor()
        {
            const String DWM_KEY = @"Software\Microsoft\Windows\DWM";
            using (RegistryKey dwmKey = Registry.CurrentUser.OpenSubKey(DWM_KEY, RegistryKeyPermissionCheck.ReadSubTree))
            {
                const String KEY_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\" registry key does not exist.";
                if (dwmKey is null) throw new InvalidOperationException(KEY_EX_MSG);

                Object accentColorObj = dwmKey.GetValue("AccentColor");
                if (accentColorObj is Int32 accentColorDword)
                {
                    return ParseDWordColor(accentColorDword);
                }
                else
                {
                    const String VALUE_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\\AccentColor\" registry key value could not be parsed as an ABGR color.";
                    throw new InvalidOperationException(VALUE_EX_MSG);
                }
            }

        }

        static uint ParseDWordColor(Int32 color)
        {
            Byte
                a = (byte)((color >> 24) & 0xFF),
                b = (byte)((color >> 16) & 0xFF),
                g = (byte)((color >> 8) & 0xFF),
                r = (byte)((color >> 0) & 0xFF);


            return (uint)((a << 24) | (r << 16) |
                          (g << 8) | (b << 0));
        }

        public static bool getLightMode()
        {
            string RegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            int theme;
            theme = (int)Registry.GetValue(RegistryKey, "AppsUseLightTheme", 0);
            return theme == 1;
        }

        IDictionary<string, string> generateScheme(uint color, bool light)
        {
            CorePalette corePalette = CorePalette.Of(color);
            IDictionary<string, string> schemeTheme = new Dictionary<string, string>();
            Scheme<uint> scheme;
            if (light)
            {
                scheme = new LightSchemeMapper().Map(corePalette);
            }
            else
            {
                scheme = new DarkSchemeMapper().Map(corePalette);
            }
            return scheme.getAsJson();
        }

        IDictionary<string, string> scheme;

        internal string GetString(string token)
        {   
            if (!scheme.ContainsKey(token))
            {
                return "0, 0, 0";
            }
            return scheme[token];
        }

        uint ToUint(Color c)
        {
            return (uint)(((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B) & 0xffffffffL);
        }

        uint getColorFromImage(string path, Rainmeter.API api)
        {
            try
            {
                Bitmap img = new(path);

                api.Log(API.LogType.Notice, "1");

                List<uint> pixels = new();

                api.Log(API.LogType.Notice, "2");

                for (int i = 0; i < Math.Floor((double)img.Width / 11); i++) //We do not need all of the pixels
                {
                    for (int j = 0; j < Math.Floor((double)img.Height / 11); j++) //We only grab a few (for performance)
                    {
                        Color pixel = img.GetPixel(i * 10, j * 10);
                        pixels.Add(ToUint(pixel));
                    }
                }

                api.Log(API.LogType.Notice, "3");

                List<uint> bestColors = ImageUtils.ColorsFromImage(pixels.ToArray());

                api.Log(API.LogType.Notice, "4");

                api.Log(API.LogType.Notice, bestColors[0].ToString());

                return bestColors[0];
            } catch (System.Exception e)
            {
                api.Log(API.LogType.Error, "Error generating Material You color scheme from image :(");
                api.Log(API.LogType.Error, "Error:" + e.ToString());

                return GetAccentColor();
            }
        }

        void genSchemeOnRequest(Rainmeter.API api)
        {
            string source = api.ReadString("ColorSource", "");

            if (source.Length > 1)
            {
                try
                {
                    if (source.ToLower() == "wallpaper")
                    {
                        scheme = generateScheme(getColorFromImage(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Themes\\TranscodedWallpaper", api), getLightMode());
                    }
                    else
                    {
                        scheme = generateScheme(getColorFromImage(source, api), getLightMode());
                    }
                }
                catch (System.Exception e)
                {
                    api.Log(API.LogType.Error, "Error starting Material You plugin :(");
                    api.Log(API.LogType.Error, "Error:" + e.ToString());

                    scheme = generateScheme(GetAccentColor(), getLightMode());
                }
            }
            else
            {
                scheme = generateScheme(GetAccentColor(), getLightMode());
            }
        }

        private static string rainmeterFileSettingsLocation = "";

        internal Measure(Rainmeter.API api)
        {
            try
            {
                if (rainmeterFileSettingsLocation != API.GetSettingsFile())
                {
                    rainmeterFileSettingsLocation = API.GetSettingsFile();
                }

                genSchemeOnRequest(api);
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Error, "Error starting Material You plugin :(");
                api.Log(API.LogType.Error, "Error:" + e.ToString());
            }
        }

        internal virtual void Reload(Rainmeter.API api, ref double maxValue)
        {
            try
            {
                genSchemeOnRequest(api);
            }
            catch (Exception e)
            {
                api.Log(API.LogType.Error, "Error reloading Material You plugin :(");
                api.Log(API.LogType.Error, "Error:" + e.ToString());
            }
        }

        internal virtual void Dispose()
        {

        }

        internal virtual double Update()
        {
            return 0.0;
        }

    }
    public class Plugin
    {
        static IntPtr StringBuffer = IntPtr.Zero;

        private static string TrimQuotes(string rawString)
        {
            string outString = rawString;
            if ((rawString.StartsWith("\"") && rawString.EndsWith("\"")) || (rawString.StartsWith("'") && rawString.EndsWith("'")))
            {
                outString = outString.Remove(0, 1);
                outString = outString.Remove(outString.Length - 1, 1);
                return outString;
            }
            return rawString;
        }

        [DllExport]
        public static IntPtr GetToken(IntPtr data, int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 1)] string[] argv)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;

            if (argc <= 0)
            {
                return Marshal.StringToHGlobalUni(measure.GetString("Primary"));
            }

            return Marshal.StringToHGlobalUni(measure.GetString(TrimQuotes(argv[0])));
        }

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            Rainmeter.API theapithing = new Rainmeter.API(rm);
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure(theapithing)));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Dispose();

            GCHandle.FromIntPtr(data).Free();

            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Rainmeter.API theapithing = new Rainmeter.API(rm);
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new Rainmeter.API(rm), ref maxValue);
        }


        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }
    }
}