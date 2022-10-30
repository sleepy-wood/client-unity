using System.IO;

using UnityEditor;

namespace Broccoli.Utils
{
    /// <summary>
    /// File utilities.
    /// </summary>
    public class FileUtils {
        /// <summary>
        /// Gets a combined file name.
        /// </summary>
        /// <param name="folderPath">Path to the folder.</param>
        /// <param name="filename">Filename.</param>
        /// <param name="ext">Extension of the file, without the '.'.</param>
        /// <returns>A suffix to use on a file name.</returns>
        public static string GetFilePath (string folderPath, string filename, string ext, int take = -1) {
            return Path.Combine (folderPath, filename) + (take<0?"":GetFileTakeSuffix (take)) + "." + ext;
        }
        /// <summary>
        /// Gets a take suffix for a file.
        /// </summary>
        /// <param name="take">Number of take from 0 to 999.</param>
        /// <returns>File take suffix.</returns>
        public static string GetFileTakeSuffix (int take) {
            if (take < 0) take = 0;
            string suffix;
            if (take < 10) suffix = $"_00{take}";
            else if (take < 100) suffix = $"_0{take}";
            else suffix = $"_{take}";
            return suffix;
        }
        /// <summary>
        /// Checks if a path to a folder is valid.
        /// </summary>
        /// <param name="folderPath">Path to a folder.</param>
        /// <returns><c>True</c> if the path corresponds to a valid folder.</returns>
        public static bool IsValidFolder (string folderPath) {
            #if UNITY_EDITOR
            return AssetDatabase.IsValidFolder (folderPath);
            #else
            return false;
            #endif
        }
        /// <summary>
        /// Creates a subfolder given its name an a parent path.
        /// </summary>
        /// <param name="parentFolderFullPath">Path to the parend folder, should contain Assets/.</param>
        /// <param name="subfolderName">Name of the subfolder</param>
        /// <returns><c>True</c>if the subfolder was created or it exists.</returns>
        public static bool CreateSubfolder (string parentFolderFullPath, string subfolderName) {
            #if UNITY_EDITOR
            if (!IsValidFolder (parentFolderFullPath)) {
                UnityEngine.Debug.LogWarning ("Not a valid path to create a subfolder: " + parentFolderFullPath);
                return false;
            }
            // Check if already exists, then return true.
            string fullPath = Path.Combine (parentFolderFullPath, subfolderName);
            if (IsValidFolder (fullPath)) return true;
            string folderGUID = AssetDatabase.CreateFolder (parentFolderFullPath, subfolderName);
            if (string.IsNullOrEmpty (folderGUID)) {
                UnityEngine.Debug.LogWarning ("Could not create subfolder: " + fullPath + ". Check file permissions.");
                return false;
            } else {
                return true;
            }
            #else
            return false;
            #endif
        }
        /// <summary>
        /// Checks if the path to a file is a valid asset file.
        /// </summary>
        /// <param name="path">Path to the asset file.</param>
        /// <returns><c>True</c> if it is a valid asset file path.</returns>
        public static bool IsValidAssetFile (string path) {
            #if UNITY_EDITOR
            // Validate the full path.
			return !string.IsNullOrEmpty (AssetDatabase.AssetPathToGUID (path));
            #else
            return false;
            #endif
        }
        /// <summary>
        /// Search if files wih an existing '_xxx' suffix exists and return the next iterarion suffix.
        /// </summary>
        /// <param name="folderPath">Path to the folder.</param>
        /// <param name="filename">Filename.</param>
        /// <param name="ext">Extension of the file, without the '.'.</param>
        /// <returns>A suffix to use on a file name.</returns>
        public static string GetNumericSuffix (string folderPath, string filename, string ext) {
            string suffix = "_000";
            string fullPath; 
            int i = 0;
			do {
                if (i < 10) suffix = $"_00{i}";
                else if (i < 100) suffix = $"_0{i}";
                else suffix = $"_{i}";
				fullPath = Path.Combine (folderPath, filename) + suffix + "." + ext;
				i++;
			} while (IsValidAssetFile (fullPath));
            return suffix;
        }
    }
}