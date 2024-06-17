using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace LB
{
    public static class SaveDataUtil
    {
        public static string SavePath()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save level layout",
                "",
                "lb_level.lblayout",
                "lblayout"
            );
            return path;
        }

        public static string OpenPath()
        {
            string path = EditorUtility.OpenFilePanel(
                "Open Saved Level Layout",
                "",
                "lblayout"
            );
            return path;
        }

        public static string [] OpenFileAtPath(string path)
        {
            List<string> fileData = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                string newLine;
                while ((newLine = sr.ReadLine()) != null)
                {
                    fileData.Add(newLine);
                }
            }
            return fileData.ToArray();
        }

        public static void SaveArrayToPath<T>(T[,] arr, string path)
        {
            string [] rows = new string [arr.GetLength(0)];
            for (int i=0; i<arr.GetLength(0); i++)
            {
                string [] newRow = new string [arr.GetLength(1)];
                for (int j=0; j<arr.GetLength(1); j++)
                {
                    newRow [j] = arr[i,j].ToString();
                }
                rows[i] = string.Join(",", newRow);
            }
            WriteStringArrayToFile(rows, path);
        }

        private static void WriteStringArrayToFile(string [] arr, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i=0; i<arr.GetLength(0); i++)
                {
                    sw.WriteLine(arr[i]);
                }
            }
        }
    }
}