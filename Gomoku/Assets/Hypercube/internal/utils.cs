using UnityEngine;
using System.Collections;
using System.IO;

namespace hypercube
{
    public class utils
    {
        //this method is used to figure out which drive is the usb flash drive attached to Volume, and then returns that path so that our settings can load normally from there.
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        public static bool getConfigPath(string relativePathToConfig, out string fullPath)
        {
            relativePathToConfig = relativePathToConfig.Replace('\\', Path.DirectorySeparatorChar);
            relativePathToConfig = relativePathToConfig.Replace('/', Path.DirectorySeparatorChar);

            string[] drives = System.Environment.GetLogicalDrives();
            foreach (string drive in drives)
            {
                if (File.Exists(drive + relativePathToConfig))
                {
                    fullPath = drive + relativePathToConfig;
                    return true;
                }
            }
            Debug.LogWarning("Could not locate Volume calibration file!\nIs Volume connected via USB?");
            fullPath = Path.GetFileName(relativePathToConfig); //return the base name of the file only.
            return false;
        }
#else  //osx,  TODO: linux untested in standalone
        public static bool getConfigPath(string relativePathToConfig, out string fullPath)        
        {                        
            string[] directories = Directory.GetDirectories("/Volumes/");
            foreach (string d in directories)
            {
                string fixedPath = d + "/" + relativePathToConfig;
                fixedPath = fixedPath.Replace('\\', Path.DirectorySeparatorChar);
                fixedPath = fixedPath.Replace('/', Path.DirectorySeparatorChar);

                FileInfo f = new FileInfo (fixedPath);
                if (f.Exists)                
                {                    
                    fullPath = f.FullName;      
                    return true;  
                }            
            }            
            fullPath = Path.GetFileName(relativePathToConfig); //return the base name of the file only.        
            return false;
        }
#endif

        // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
        public static string colorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }
        public static Color hexToColor(string hex)
        {
            hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
            hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
            byte a = 255;//assume fully visible unless specified in hex
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            //Only use alpha if the string has enough characters
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }
    }

}
