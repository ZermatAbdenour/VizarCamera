using System;
using TriLibCore.General;
using TriLibCore.Mappers;
using UnityEngine;

namespace TriLibCore.URP.Mappers
{
    /// <summary>Represents a Material Mapper that converts TriLib Materials into Unity UniversalRP Materials.</summary>
    [Serializable]
    [CreateAssetMenu(menuName = "TriLib/Mappers/Material/Universal RP Material Mapper", fileName = "UniversalRPMaterialMapper")]
    public class UniversalRPMaterialMapper : MaterialMapper
    {
        private bool _isCompatible;

        #region Standard
        public override Material MaterialPreset => Resources.Load<Material>("Materials/UniversalRP/Standard/TriLibUniversalRP");

        public override Material CutoutMaterialPreset => Resources.Load<Material>("Materials/UniversalRP/Standard/TriLibUniversalRPAlphaCutout");

        public override Material TransparentMaterialPreset => Resources.Load<Material>("Materials/UniversalRP/Standard/TriLibUniversalRPAlpha");

        public override Material TransparentComposeMaterialPreset => Resources.Load<Material>("Materials/UniversalRP/Standard/TriLibUniversalRPAlpha");
        #endregion

        public override Material LoadingMaterial => Resources.Load<Material>("Materials/UniversalRP/TriLibUniversalRPLoading");

        public override bool IsCompatible(MaterialMapperContext materialMapperContext)
        {
            return _isCompatible;
        }

        private void Awake()
        {
            _isCompatible = TriLibSettings.GetBool("UniversalRPMaterialMapper");
        }

        public override void Map(MaterialMapperContext materialMapperContext)
        {
            materialMapperContext.VirtualMaterial = new VirtualMaterial();

            CheckTransparencyMapTexture(materialMapperContext);
            CheckSpecularMapTexture(materialMapperContext);

            CheckDiffuseMapTexture(materialMapperContext);
            CheckDiffuseColor(materialMapperContext);

            CheckNormalMapTexture(materialMapperContext);

            CheckEmissionMapTexture(materialMapperContext);
            CheckEmissionColor(materialMapperContext);

            CheckOcclusionMapTexture(materialMapperContext);

            CheckGlossinessMapTexture(materialMapperContext);
            CheckGlossinessValue(materialMapperContext);

            CheckMetallicGlossMapTexture(materialMapperContext);
            CheckMetallicValue(materialMapperContext);

            materialMapperContext.AddPostProcessingAction(BuildMaterial, materialMapperContext);
        }

        private void CheckDiffuseMapTexture(MaterialMapperContext materialMapperContext)
        {
            var diffuseTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.DiffuseMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(diffuseTexturePropertyName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Diffuse, textureValue, CheckTextureOffsetAndScaling, ApplyDiffuseMapTexture);
        }

        private void ApplyDiffuseMapTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.Context.AddUsedTexture(textureLoadingContext.UnityTexture);
            }
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_BaseMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.DiffuseMap);
        }

        private void CheckGlossinessValue(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericFloatValueMultiplied(GenericMaterialProperty.Glossiness, materialMapperContext);
            materialMapperContext.VirtualMaterial.SetProperty("_Smoothness", value);
        }

        private void CheckMetallicValue(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericFloatValueMultiplied(GenericMaterialProperty.Metallic, materialMapperContext);
            materialMapperContext.VirtualMaterial.SetProperty("_Metallic", value);
        }

        private void CheckEmissionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var emissionTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.EmissionMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(emissionTexturePropertyName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Emission, textureValue, CheckTextureOffsetAndScaling, ApplyEmissionMapTexture);
        }

        private void ApplyEmissionMapTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.Context.AddUsedTexture(textureLoadingContext.UnityTexture);
            }
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_EmissionMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.EmissionMap);
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_EMISSION");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_EMISSION");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckNormalMapTexture(MaterialMapperContext materialMapperContext)
        {
            var normalMapTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.NormalMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(normalMapTexturePropertyName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.NormalMap, textureValue, CheckTextureOffsetAndScaling, ApplyNormalMapTexture);
        }

        private void ApplyNormalMapTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.Context.AddUsedTexture(textureLoadingContext.UnityTexture);
            }
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_BumpMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.NormalMap);
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_NormalScale", 1f);
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP");
            }
        }

        private void CheckTransparencyMapTexture(MaterialMapperContext materialMapperContext)
        {
            materialMapperContext.VirtualMaterial.HasAlpha |= materialMapperContext.Material.UsesAlpha;
            var transparencyTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.TransparencyMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(transparencyTexturePropertyName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Transparency, textureValue, CheckTextureOffsetAndScaling);
        }

        private void CheckSpecularMapTexture(MaterialMapperContext materialMapperContext)
        {
            var specularTexturePropertyName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.SpecularMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(specularTexturePropertyName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Specular, textureValue, CheckTextureOffsetAndScaling);
        }

        private void CheckOcclusionMapTexture(MaterialMapperContext materialMapperContext)
        {
            var occlusionMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.OcclusionMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(occlusionMapTextureName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Occlusion, textureValue, CheckTextureOffsetAndScaling, ApplyOcclusionMapTexture);
        }

        private void ApplyOcclusionMapTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.Texture != null)
            {
                textureLoadingContext.Context.AddUsedTexture(textureLoadingContext.UnityTexture);
            }
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_OcclusionMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.OcclusionMap);
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_OCCLUSIONMAP");
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_OCCLUSIONMAP");
            }
        }

        private void CheckGlossinessMapTexture(MaterialMapperContext materialMapperContext)
        {
            var auxiliaryMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.GlossinessOrRoughnessMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(auxiliaryMapTextureName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.GlossinessOrRoughness, textureValue, CheckTextureOffsetAndScaling);
        }

        private void CheckMetallicGlossMapTexture(MaterialMapperContext materialMapperContext)
        {
            var metallicGlossMapTextureName = materialMapperContext.Material.GetGenericPropertyName(GenericMaterialProperty.MetallicMap);
            var textureValue = materialMapperContext.Material.GetTextureValue(metallicGlossMapTextureName);
            LoadTextureWithCallbacks(materialMapperContext, TextureType.Metalness, textureValue, CheckTextureOffsetAndScaling, ApplyMetallicGlossMapTexture);
        }

        private void ApplyMetallicGlossMapTexture(TextureLoadingContext textureLoadingContext)
        {
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.Context.AddUsedTexture(textureLoadingContext.UnityTexture);
            }
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_MetallicGlossMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.MetallicMap);
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_METALLICGLOSSMAP");
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_METALLICGLOSSMAP");
            }
        }

        private void CheckEmissionColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValueMultiplied(GenericMaterialProperty.EmissionColor, materialMapperContext);
            materialMapperContext.VirtualMaterial.SetProperty("_EmissionColor", value);
            if (value != Color.black)
            {
                materialMapperContext.VirtualMaterial.EnableKeyword("_EMISSION");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                materialMapperContext.VirtualMaterial.SetProperty("_EmissiveIntensity", 1f);
            }
            else
            {
                materialMapperContext.VirtualMaterial.DisableKeyword("_EMISSION");
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckDiffuseColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValueMultiplied(GenericMaterialProperty.DiffuseColor, materialMapperContext);
            value.a *= materialMapperContext.Material.GetGenericFloatValueMultiplied(GenericMaterialProperty.AlphaValue);
            materialMapperContext.VirtualMaterial.HasAlpha |= value.a < 1f;
            materialMapperContext.VirtualMaterial.SetProperty("_BaseColor", value);
        }
        
        public override string GetDiffuseTextureName(MaterialMapperContext materialMapperContext)
        {
            return "_BaseMap";
        }

        public override string GetGlossinessOrRoughnessTextureName(MaterialMapperContext materialMapperContext)
        {
            return "_MetallicGlossMap";
        }

        public override string GetDiffuseColorName(MaterialMapperContext materialMapperContext)
        {
            return "_BaseColor";
        }

        public override string GetEmissionColorName(MaterialMapperContext materialMapperContext)
        {
            return "_EmissionColor";
        }

        public override string GetGlossinessOrRoughnessName(MaterialMapperContext materialMapperContext)
        {
            return "_Smoothness";
        }

        public override string GetMetallicName(MaterialMapperContext materialMapperContext)
        {
            return "_Metallic";
        }

        public override string GetMetallicTextureName(MaterialMapperContext materialMapperContext)
        {
            return null;
        }
    }
}