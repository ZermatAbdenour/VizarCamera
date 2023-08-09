using System;
using TriLibCore.General;
using TriLibCore.Mappers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace TriLibCore.HDRP.Mappers
{
    /// <summary>Represents a Material Mapper that converts TriLib Materials into Unity HDRP Materials.</summary>
    [Serializable]
    [CreateAssetMenu(menuName = "TriLib/Mappers/Material/HDRP Material Mapper", fileName = "HDRPMaterialMapper")]
    public class HDRPMaterialMapper : MaterialMapper
    {
        private bool _isCompatible;

        #region Standard
        public override Material MaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRP");

        public override Material CutoutMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlphaCutout");

        public override Material TransparentMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlpha");

        public override Material TransparentComposeMaterialPreset => Resources.Load<Material>("Materials/HDRP/Standard/TriLibHDRPAlpha");
        #endregion

        public override Material LoadingMaterial => Resources.Load<Material>("Materials/HDRP/TriLibHDRPLoading");

        public override bool IsCompatible(MaterialMapperContext materialMapperContext)
        {
            return _isCompatible;
        }

        private void Awake()
        {
            _isCompatible = TriLibSettings.GetBool("HDRPMaterialMapper");
        }

        public override void Map(MaterialMapperContext materialMapperContext)
        {
            materialMapperContext.VirtualMaterial = new HDRPVirtualMaterial();

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
            materialMapperContext.AddPostProcessingAction(BuildHDRPMask, materialMapperContext);
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
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_BaseColorMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.DiffuseMap);
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
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_EmissiveColorMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.EmissionMap);
            if (textureLoadingContext.UnityTexture)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_EMISSIVE_COLOR_MAP");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_EmissiveIntensity", 1f);
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_EMISSIVE_COLOR_MAP");
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
            textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_NormalMap", textureLoadingContext.UnityTexture, GenericMaterialProperty.NormalMap);
            if (textureLoadingContext.UnityTexture != null)
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.SetProperty("_NormalScale", 1f);
            }
            else
            {
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP");
                textureLoadingContext.MaterialMapperContext.VirtualMaterial.DisableKeyword("_NORMALMAP_TANGENT_SPACE");
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
            ((HDRPVirtualMaterial)textureLoadingContext.MaterialMapperContext.VirtualMaterial).OcclusionTexture = textureLoadingContext.UnityTexture;
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
            ((HDRPVirtualMaterial)textureLoadingContext.MaterialMapperContext.VirtualMaterial).MetallicTexture = textureLoadingContext.UnityTexture;
        }

        private void CheckEmissionColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValueMultiplied(GenericMaterialProperty.EmissionColor, materialMapperContext);
            materialMapperContext.VirtualMaterial.SetProperty("_EmissiveColor", value);
            materialMapperContext.VirtualMaterial.SetProperty("_EmissiveColorLDR", value);
            if (value != Color.black)
            {
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                materialMapperContext.VirtualMaterial.SetProperty("_EmissiveIntensity", 1f);
            }
            else
            {
                materialMapperContext.VirtualMaterial.GlobalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }

        private void CheckDiffuseColor(MaterialMapperContext materialMapperContext)
        {
            var value = materialMapperContext.Material.GetGenericColorValueMultiplied(GenericMaterialProperty.DiffuseColor, materialMapperContext);
            value.a *= materialMapperContext.Material.GetGenericFloatValueMultiplied(GenericMaterialProperty.AlphaValue);
            materialMapperContext.VirtualMaterial.HasAlpha |= value.a < 1f;
            materialMapperContext.VirtualMaterial.SetProperty("_BaseColor", value);
            materialMapperContext.VirtualMaterial.SetProperty("_Color", value);
        }

        private void BuildHDRPMask(MaterialMapperContext materialMapperContext)
        {
            if (materialMapperContext.UnityMaterial == null)
            {
                return;
            }
            var hdrpVirtualMaterial = (HDRPVirtualMaterial)materialMapperContext.VirtualMaterial;
            var maskBaseTexture = hdrpVirtualMaterial.MetallicTexture ?? hdrpVirtualMaterial.OcclusionTexture ?? hdrpVirtualMaterial.DetailMaskTexture;
            if (maskBaseTexture == null)
            {
                if (materialMapperContext.Context.Options.UseMaterialKeywords)
                {
                    materialMapperContext.UnityMaterial.DisableKeyword("_MASKMAP");
                }
                return;
            }
            var graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            var renderTexture = new RenderTexture(maskBaseTexture.width, maskBaseTexture.height, 0, graphicsFormat);
            renderTexture.name = $"{(string.IsNullOrWhiteSpace(maskBaseTexture.name) ? "Unnamed" : maskBaseTexture.name)}_Mask";
            renderTexture.useMipMap = false;
            renderTexture.autoGenerateMips = false;
            var material = new Material(Shader.Find("Hidden/TriLib/BuildHDRPMask"));
            if (hdrpVirtualMaterial.MetallicTexture != null)
            {
                material.SetTexture("_MetallicTex", hdrpVirtualMaterial.MetallicTexture);
            }
            if (hdrpVirtualMaterial.OcclusionTexture != null)
            {
                material.SetTexture("_OcclusionTex", hdrpVirtualMaterial.OcclusionTexture);
            }
            if (hdrpVirtualMaterial.DetailMaskTexture != null)
            {
                material.SetTexture("_DetailMaskTex", hdrpVirtualMaterial.DetailMaskTexture);
            }
            Graphics.Blit(null, renderTexture, material);
            if (renderTexture.useMipMap)
            {
                renderTexture.GenerateMips();
            }
            if (materialMapperContext.Context.Options.UseMaterialKeywords)
            {
                materialMapperContext.UnityMaterial.EnableKeyword("_MASKMAP");
            }
            materialMapperContext.UnityMaterial.SetTexture("_MaskMap", renderTexture);
            materialMapperContext.VirtualMaterial.TextureProperties.Add("_MaskMap", renderTexture);
            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }

        public override string GetDiffuseTextureName(MaterialMapperContext materialMapperContext)
        {
            return "_BaseColorMap";
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
