﻿using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ScriptedImporter(k_SmartTextureVersion, k_SmartTextureExtesion)]
public class SmartTextureImporter : ScriptedImporter
{
    public const string k_SmartTextureExtesion = "smartex";
    public const int k_SmartTextureVersion = 1;

    // Input Texture Settings
    [SerializeField] Texture2D[] m_InputTextures = new Texture2D[4];
    [SerializeField] TexturePackingSettings[] m_InputTextureSettings = new TexturePackingSettings[4];
    
    // Output Texture Settings
    [SerializeField] bool m_IsReadable = false;
    [SerializeField] bool m_sRGBTexture = false;
    [SerializeField] bool m_EnableMipMap = true;

    [SerializeField] FilterMode m_FilterMode = FilterMode.Bilinear;
    [SerializeField] TextureWrapMode m_WrapMode = TextureWrapMode.Repeat;
    [SerializeField] int m_AnisotricLevel = 1;

    [MenuItem("Assets/Create/Smart Texture", priority = 310)]
    static void CreateTexture2DArrayMenuItem()
    {
        // https://forum.unity.com/threads/how-to-implement-create-new-asset.759662/
        string directoryPath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            directoryPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(directoryPath) && File.Exists(directoryPath))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
                break;
            }
        }

        directoryPath = directoryPath.Replace("\\", "/");
        if (directoryPath.Length > 0 && directoryPath[directoryPath.Length - 1] != '/')
            directoryPath += "/";
        if (string.IsNullOrEmpty(directoryPath))
            directoryPath = "Assets/";

        var fileName = string.Format("SmartTexture.{0}", k_SmartTextureExtesion);
        directoryPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + fileName);
        ProjectWindowUtil.CreateAssetWithContent(directoryPath,
            "SmartTexture.");
    }
    
    public override void OnImportAsset(AssetImportContext ctx)
    {
        int width = 512;
        int height = 512;
        Texture2D[] textures = m_InputTextures;
        TexturePackingSettings[] settings = m_InputTextureSettings;
        bool canGenerateTexture = GetOuputTextureSize(textures, out width, out height);

        Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32,  m_EnableMipMap);
        mask.filterMode = m_FilterMode;
        mask.wrapMode = m_WrapMode;
        mask.anisoLevel = m_AnisotricLevel;
        // TODO:
        // read/write
        // color space
        
        if (canGenerateTexture)
        {
            mask.PackChannels(textures, settings);
            
            // Mark all input textures as dependency to the texture array.
            // This causes the texture array to get re-generated when any input texture changes or when the build target changed.
            foreach (Texture2D t in textures)
            {
                if (t != null)
                {
                    var path = AssetDatabase.GetAssetPath(t);
                    ctx.DependsOnSourceAsset(path);
                }
            }
        }
        
        ctx.AddObjectToAsset("mask", mask);
        ctx.SetMainObject(mask);
    }
    
    bool GetOuputTextureSize(Texture2D[] textures, out int width, out int height)
    {
        Texture2D masterTexture = null;
        foreach (Texture2D t in textures)
        {
            if (t != null && t.isReadable)
            {
                masterTexture = t;
                break;
            }
        }

        if (masterTexture == null)
        {
            var defaultTexture = Texture2D.blackTexture;
            width = defaultTexture.width;
            height = defaultTexture.height;
            return false;
        }

        width = masterTexture.width;
        height = masterTexture.height;
        return true;
    }
}
