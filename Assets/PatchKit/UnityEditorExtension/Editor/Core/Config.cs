using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace PatchKit.UnityEditorExtension.Core
{
public class Config : ScriptableObject
{
    private const string DefaultPath =
        "Assets/PatchKit/UnityEditorExtension/Config.asset";

    private static Config FindInstance()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(Config).Name);

        Assert.IsNotNull(guids);

        Config[] instances = guids.Select(x => AssetDatabase.GUIDToAssetPath(x))
            .Select(x => AssetDatabase.LoadAssetAtPath<Config>(x))
            .Where(x => x != null)
            .ToArray();

        if (instances.Length == 0)
        {
            return null;
        }

        if (instances.Length > 1)
        {
            string[] paths = instances
                .Select(x => AssetDatabase.GetAssetPath(x))
                .ToArray();

            string warning =
                "There are multiple instances of PatchKit Unity Editor Extension config.\n\n" +
                "Please pick one and delete rest:\n" +
                string.Join("\n", paths.Select(x => "- " + x).ToArray()) +
                "\n\n" +
                "Currently using " +
                paths[0];

            EditorUtility.DisplayDialog(
                "Multiple configurations",
                warning,
                "OK");
        }

        return instances[0];
    }

    [NotNull]
    private static Config CreateInstance()
    {
        var instance = ScriptableObject.CreateInstance<Config>();
        Assert.IsNotNull(instance);

        AssetDatabase.CreateAsset(instance, DefaultPath);
        AssetDatabase.SaveAssets();

        return instance;
    }

    [NotNull]
    private static Config FindOrCreateInstance()
    {
        Config instance = FindInstance();

        if (instance == null)
        {
            instance = CreateInstance();
        }

        return instance;
    }

#if PATCHKIT_UNITY_EDITOR_EXTENSION_DEV
    [SerializeField]
    private bool _useOverrideApiConnectionSettings;

    [SerializeField]
    private PatchKit.Api.ApiConnectionSettings _overrideApiConnectionSettings =
        PatchKit.Api.MainApiConnection.GetDefaultSettings();

#endif

    public static PatchKit.Api.ApiConnectionSettings ApiConnectionSettings
    {
        get
        {
            Config instance = FindOrCreateInstance();

#if PATCHKIT_UNITY_EDITOR_EXTENSION_DEV
            if (instance._useOverrideApiConnectionSettings)
            {
                return instance._overrideApiConnectionSettings;
            }
#endif
            return PatchKit.Api.MainApiConnection.GetDefaultSettings();
        }
    }

    [SerializeField]
    private string _savedWindows32AppSecret;

    [SerializeField]
    private string _savedWindows64AppSecret;

    [SerializeField]
    private string _savedLinux32AppSecret;

    [SerializeField]
    private string _savedLinux64AppSecret;

    [SerializeField]
    private string _savedMac64AppSecret;

    [NotNull]
    public static Dictionary<AppPlatform, AppSecret?> GetSavedAppSecrets()
    {
        return new Dictionary<AppPlatform, AppSecret?>
        {
            {
                AppPlatform.Windows32, GetSavedAppSecret(AppPlatform.Windows32)
            },
            {
                AppPlatform.Windows64, GetSavedAppSecret(AppPlatform.Windows64)
            },
            {
                AppPlatform.Linux32, GetSavedAppSecret(AppPlatform.Linux32)
            },
            {
                AppPlatform.Linux64, GetSavedAppSecret(AppPlatform.Linux64)
            },
            {
                AppPlatform.Mac64, GetSavedAppSecret(AppPlatform.Mac64)
            }
        };
    }

    public static AppSecret? GetSavedAppSecret(AppPlatform platform)
    {
        Config instance = FindOrCreateInstance();

        string value;
        switch (platform)
        {
            case AppPlatform.Windows32:
                value = instance._savedWindows32AppSecret;
                break;
            case AppPlatform.Windows64:
                value = instance._savedWindows64AppSecret;
                break;
            case AppPlatform.Linux32:
                value = instance._savedLinux32AppSecret;
                break;
            case AppPlatform.Linux64:
                value = instance._savedLinux64AppSecret;
                break;
            case AppPlatform.Mac64:
                value = instance._savedMac64AppSecret;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "platform",
                    platform,
                    null);
        }

        try
        {
            return new AppSecret(value);
        }
        catch (ValidationException)
        {
            return null;
        }
    }

    public static void SetSavedAppSecret(
        AppSecret appSecret,
        AppPlatform platform)
    {
        if (!appSecret.IsValid)
        {
            throw new InvalidArgumentException("appSecret");
        }

        Config instance = FindOrCreateInstance();

        switch (platform)
        {
            case AppPlatform.Windows32:
                instance._savedWindows32AppSecret = appSecret.Value;
                break;
            case AppPlatform.Windows64:
                instance._savedWindows64AppSecret = appSecret.Value;
                break;
            case AppPlatform.Linux32:
                instance._savedLinux32AppSecret = appSecret.Value;
                break;
            case AppPlatform.Linux64:
                instance._savedLinux64AppSecret = appSecret.Value;
                break;
            case AppPlatform.Mac64:
                instance._savedMac64AppSecret = appSecret.Value;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "platform",
                    platform,
                    null);
        }

        EditorUtility.SetDirty(instance);
        AssetDatabase.SaveAssets();
    }

    public static void ClearSavedAppSecret(AppPlatform platform)
    {
        Config instance = FindOrCreateInstance();

        switch (platform)
        {
            case AppPlatform.Windows32:
                instance._savedWindows32AppSecret = string.Empty;
                break;
            case AppPlatform.Windows64:
                instance._savedWindows64AppSecret = string.Empty;
                break;
            case AppPlatform.Linux32:
                instance._savedLinux32AppSecret = string.Empty;
                break;
            case AppPlatform.Linux64:
                instance._savedLinux64AppSecret = string.Empty;
                break;
            case AppPlatform.Mac64:
                instance._savedMac64AppSecret = string.Empty;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "platform",
                    platform,
                    null);
        }

        EditorUtility.SetDirty(instance);
        AssetDatabase.SaveAssets();
    }

    private const string ApiKeyPlayerPrefsKey =
        "patchkit-tools-integration-api-key";

    private const string ApiKeyEncryptionKey = "42";

    public static void SetSavedApiKey(ApiKey apiKey)
    {
        if (!apiKey.IsValid)
        {
            throw new InvalidArgumentException("apiKey");
        }

        byte[] data = Encryption.EncryptString(
            apiKey.Value,
            ApiKeyEncryptionKey);

        string dataString = Convert.ToBase64String(data);

        PlayerPrefs.SetString(ApiKeyPlayerPrefsKey, dataString);
        PlayerPrefs.Save();
    }

    public static ApiKey? GetSavedApiKey()
    {
        if (PlayerPrefs.HasKey(ApiKeyPlayerPrefsKey))
        {
            string dataString = PlayerPrefs.GetString(ApiKeyPlayerPrefsKey);

            if (string.IsNullOrEmpty(dataString))
            {
                return null;
            }

            byte[] data = Convert.FromBase64String(dataString);

            string value = Encryption.DecryptString(data, ApiKeyEncryptionKey);

            try
            {
                return new ApiKey(value);
            }
            catch (ValidationException)
            {
                return null;
            }
        }

        return null;
    }

    public static void ClearSavedApiKey()
    {
        PlayerPrefs.DeleteKey(ApiKeyPlayerPrefsKey);
        PlayerPrefs.Save();
    }
}
}