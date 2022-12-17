using System;
using System.Runtime.InteropServices;
using Rainmeter;
using Microsoft.Win32;
using MaterialColorUtilities.Schemes;
using MaterialColorUtilities.Palettes;
using System.Collections.Generic;

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

        String token = "Primary";

        internal string GetString()
        {
            if (!scheme.ContainsKey(token))
            {
                return "0, 0, 0";
            }
            return scheme[token];
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

                token = api.ReadString("Token", "Primary");
                scheme = generateScheme(GetAccentColor(), getLightMode());
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
                token = api.ReadString("Token", "Primary");
                scheme = generateScheme(GetAccentColor(), getLightMode());
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

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (StringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(StringBuffer);
                StringBuffer = IntPtr.Zero;
            }

            string stringValue = measure.GetString();
            if (stringValue != null)
            {
                StringBuffer = Marshal.StringToHGlobalUni(stringValue);
            }

            return StringBuffer;
        }
    }
}