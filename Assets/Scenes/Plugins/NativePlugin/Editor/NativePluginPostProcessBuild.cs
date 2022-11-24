using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class NativePluginPostProcessBuild
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            // Get root
            PlistElementDict rootDict = plist.root;
            // Read health data
            rootDict.SetString(
                "NSHealthShareUsageDescription",
                "This app needs to access your health data to grow your own personalized tree."
            );
            rootDict.SetString(
                "NSMotionUsageDescription",
                "This app needs to access your motion data to detect resting/sleeping."
            );
            // Write health data
            // rootDict.SetString("NSHealthUpdateUsageDescription", "This app needs to access your health data to work properly.");
            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
            // Get project
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            string targetGuid = project.GetUnityMainTargetGuid();
            // project.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            // Add frameworks
            project.AddFrameworkToProject(targetGuid, "HealthKit.framework", false);
            File.WriteAllText(projectPath, project.WriteToString());
            // Add entitlements
            var manager = new ProjectCapabilityManager(
                projectPath,
                "Entitlements.entitlements",
                null,
                project.GetUnityMainTargetGuid()
            );
            manager.AddHealthKit();
            manager.WriteToFile();
        }
    }
}
