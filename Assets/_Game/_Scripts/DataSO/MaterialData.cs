using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Data
{
    [CreateAssetMenu(fileName = "MaterialData", menuName = "ScriptableObjects/MaterialData", order = 1)]
    public class MaterialData : SerializedScriptableObject
    {
        [SerializeField] private Material transparentMaterial;
        
        public Material GetTransparentMaterial()
        {
            return transparentMaterial;
        }

        [Title("Model Texture")]
        [SerializeField] private Material generalModelMaterial;
        [SerializeField] private Dictionary<ThemeEnum, Texture> modelTextureDict = new();
        
        [Title("Surface Material")]
        [SerializeField]
        private readonly Dictionary<MaterialEnum, Material> _surfaceMaterialDict = new();
        [SerializeField] private readonly Dictionary<MaterialEnum, SurfaceMaterialData> _surfaceMaterialDataDict = new();
        
        
        [Title("Grass Material")]
        [SerializeField]
        private readonly Dictionary<MaterialEnum, Material> _grassMaterialDict = new();
        [SerializeField] private readonly Dictionary<MaterialEnum, GrassMaterialData> _grassMaterialDataDict = new();

        [Title("Water Material Color")]
        [SerializeField] private Material waterMaterial;
        [SerializeField] private readonly Dictionary<ThemeEnum, PairColor> waterColorDict = new();

        
        #region Functionality

        public int CountSurfaceMaterial => _surfaceMaterialDict.Count;
        
        public int CountGrassMaterial => _grassMaterialDict.Count;

        
        [Title("Change theme")]
        
        [SerializeField] private ThemeEnum currentTheme;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int ShallowColor = Shader.PropertyToID("_ShallowColor");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        public ThemeEnum CurrentTheme
        {
            get => currentTheme;
            set
            {
                currentTheme = value;
                OnChangeTheme(value);
            }
        }

        [Button]
        private void OnChangeTheme(ThemeEnum themeEnum = ThemeEnum.None)
        {
            if (themeEnum == ThemeEnum.None || themeEnum != currentTheme)
            {
                themeEnum = currentTheme;
            }
            
            foreach (KeyValuePair<MaterialEnum, SurfaceMaterialData> pair in _surfaceMaterialDataDict)
            {
                pair.Value.OnChangeTheme(themeEnum);
            }
            
            foreach (KeyValuePair<MaterialEnum, GrassMaterialData> pair in _grassMaterialDataDict)
            {
                pair.Value.OnChangeTheme(themeEnum);
            }
            
            OnChangeModelTexture(themeEnum);
            OnChangeWaterColor(themeEnum);
        }
        
        private void OnChangeModelTexture(ThemeEnum themeEnum)
        {
            Texture texture = modelTextureDict[themeEnum];
            generalModelMaterial.SetTexture(BaseMap, texture);
        }
        
        private void OnChangeWaterColor(ThemeEnum themeEnum)
        {
            PairColor pairColor = waterColorDict[themeEnum];
            waterMaterial.SetColor(BaseColor, pairColor.baseColor);
            waterMaterial.SetColor(ShallowColor, pairColor.tipColor);
        }
        
        public Material GetSurfaceMaterial(MaterialEnum materialEnum)
        {
            return _surfaceMaterialDict.GetValueOrDefault(materialEnum);
        }
        
        public Material GetGrassMaterial(MaterialEnum materialEnum)
        {
            return _grassMaterialDict.GetValueOrDefault(materialEnum);
        }

        #endregion
        
 
    }
    
    [Serializable]
    public struct SurfaceMaterialData
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public MaterialEnum materialEnum;
        public Material material;
        [Tooltip("Check the order in Theme Enum")]
        public List<Color> themeColorList;


        public void OnChangeTheme(ThemeEnum themeEnum)
        {
            Color color = themeColorList[(int) themeEnum];
            material.SetColor(BaseColor, color);
        }
    }
    
    [Serializable]
    public struct GrassMaterialData
    {
        public MaterialEnum materialEnum;
        public Material material;
        [Tooltip("Check the order in Theme Enum")]
        public List<PairColor> themeColorList;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int TipColor = Shader.PropertyToID("_TipColor");

        public void OnChangeTheme(ThemeEnum themeEnum)
        {
            PairColor pairColor = themeColorList[(int) themeEnum];
            material.SetColor(BaseColor, pairColor.baseColor);
            material.SetColor(TipColor, pairColor.tipColor);
        }
    }

    [Serializable]
    public struct PairColor
    {
        public Color baseColor;
        public Color tipColor;
    }
    
    // DO NOT CHANGE THE ORDER
    public enum MaterialEnum
    {
        None = -1,
        Light = 0,
        Medium = 1,
        Dark = 2,
    }

    // DO NOT CHANGE THE ORDER
    public enum ThemeEnum
    {
        None = -1,
        Default = 0,
        Spring = 1,
        Autumn = 2,
        Winter = 3,
    }
}
