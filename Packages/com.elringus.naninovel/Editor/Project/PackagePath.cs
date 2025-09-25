using System.IO;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Provides paths to various package-related directories and resources; all paths are relative to project root.
    /// </summary>
    public static class PackagePath
    {
        public static string PackageRootPath => GetPackageRootPath();
        public static string PackageBeaconPath => Path.Combine(cachedPackagePath, "Editor", packageBeacon);
        public static string EditorResourcesPath => Path.Combine(PackageRootPath, "Editor/Resources/Naninovel");
        public static string RuntimeResourcesPath => Path.Combine(PackageRootPath, "Resources/Naninovel");
        public static string PrefabsPath => Path.Combine(PackageRootPath, "Prefabs");
        public static string GeneratedDataPath => GetGeneratedDataPath();

        private const string dataBeacon = ".naninovel.unity.data";
        private const string packageBeacon = "Elringus.Naninovel.Editor.asmdef";
        private static string cachedDataPath;
        private static string cachedPackagePath;

        private static string GetPackageRootPath ()
        {
            if (string.IsNullOrEmpty(cachedPackagePath) || !File.Exists(PackageBeaconPath))
                cachedPackagePath = FindInPackages() ?? FindInAssets();
            return cachedPackagePath ?? throw new Error("Failed to locate Naninovel package directory.");

            static string FindInPackages ()
            {
                // Even when package is installed as immutable (eg, local or git) and only physically
                // exists under Library/PackageCache/…, Unity will still symlink it to Packages/….
                const string packageDir = "Packages/com.elringus.naninovel";
                return Directory.Exists(packageDir) ? packageDir : null;
            }

            static string FindInAssets ()
            {
                foreach (var path in Directory.EnumerateFiles(Application.dataPath, packageBeacon, SearchOption.AllDirectories))
                    return PathUtils.AbsoluteToAssetPath(Directory.GetParent(path)?.Parent?.FullName ?? "");
                return null;
            }
        }

        private static string GetGeneratedDataPath ()
        {
            if (string.IsNullOrEmpty(cachedDataPath) || !File.Exists(cachedDataPath))
                cachedDataPath = FindInAssets();
            if (!string.IsNullOrEmpty(cachedDataPath)) return cachedDataPath;
            var defaultDir = "Assets/NaninovelData";
            var defaultFile = $"{defaultDir}/{dataBeacon}";
            Directory.CreateDirectory(defaultDir);
            File.WriteAllText(defaultFile, "");
            return cachedDataPath = defaultDir;

            static string FindInAssets ()
            {
                foreach (var path in Directory.EnumerateFiles(Application.dataPath, dataBeacon, SearchOption.AllDirectories))
                    return PathUtils.AbsoluteToAssetPath(Directory.GetParent(path)?.FullName ?? "");
                return null;
            }
        }
    }
}
