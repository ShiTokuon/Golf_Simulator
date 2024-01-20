#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;

namespace Kamgam.Terrain25DLib
{
    public class CheckPackages
    {
        /// <summary>
        /// The boolean value will be TRUE if the package has been imported.
        /// It will be FALSE otherwise.
        /// </summary>
        static Action<bool> OnComplete;

        /// <summary>
        /// The value will be of Type System.Version and will contains
        /// the version of the package IF it has been imported. It will
        /// be NULL otherwise (error or not found).
        /// </summary>
        static Action<Version> OnCompleteVersion;

        static ListRequest Request;
        static string PackageId;

        /// <summary>
        /// Async check whether or not the package has been added to the project.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="onComplete"></param>
        public static void CheckForPackage(string packageId, Action<bool> onComplete)
        {
            PackageId = packageId;
            Request = Client.List(offlineMode: true);
            OnComplete = onComplete;
            OnCompleteVersion = null;

            EditorApplication.update += progress;
        }

        /// <summary>
        /// Async fetch of the package info. If the package has NOT been added
        /// to the project then NULL is returned to 'onCompleteVersion'.
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="onCompleteVersion">System.Version of the package or NULL if not found (or error)</param>
        public static void CheckForPackageVersion(string packageId, Action<Version> onCompleteVersion)
        {
            PackageId = packageId;
            Request = Client.List(offlineMode: true);
            OnComplete = null;
            OnCompleteVersion = onCompleteVersion;

            EditorApplication.update += progress;
        }

        static void progress()
        {
            if (Request.IsCompleted)
            {
                EditorApplication.update -= progress;

                if (Request.Status == StatusCode.Success)
                {
                    if (OnComplete != null)
                    {
                        bool containsPackage = CheckPackages.containsPackage(Request.Result, PackageId);
                        OnComplete.Invoke(containsPackage);
                    }

                    if (OnCompleteVersion != null)
                    {
                        Version version = getPackageVersion(Request.Result, PackageId);
                        OnCompleteVersion.Invoke(version);
                    }
                }
                else if (Request.Status >= StatusCode.Failure)
                {
                    // Debug.Log("Could not check for packages: " + Request.Error.message);
                    OnComplete?.Invoke(false);
                    OnCompleteVersion?.Invoke(null);
                }
            }
        }

        static bool containsPackage(PackageCollection packages, string packageId)
        {
            foreach (var package in packages)
            {
                if (string.Compare(package.name, packageId) == 0)
                    return true;

                foreach (var dependencyInfo in package.dependencies)
                    if (string.Compare(dependencyInfo.name, packageId) == 0)
                        return true;
            }

            return false;
        }

        static Version getPackageVersion(PackageCollection packages, string packageId)
        {
            foreach (var package in packages)
            {
                if (string.Compare(package.name, packageId) == 0)
                    return new Version(package.version);

                foreach (var dependencyInfo in package.dependencies)
                    if (string.Compare(dependencyInfo.name, packageId) == 0)
                        return new Version(package.version);
            }

            return null;
        }
    }
}
#endif