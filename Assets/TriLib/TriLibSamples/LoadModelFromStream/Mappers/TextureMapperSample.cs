#pragma warning disable 672

using System;
using System.IO;
using TriLibCore.Interfaces;
using TriLibCore.Mappers;
using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Samples
{
    /// <summary>
    /// Represents a class that finds textures at the given model base path.
    /// </summary>
    public class TextureMapperSample : TextureMapper
    {
        /// <summary>Tries to retrieve a Stream to the Texture native data based on the given context.</summary>
        /// <param name="assetLoaderContext">The Asset Loader Context reference. Asset Loader Context contains the Model loading data.</param>
        /// <param name="texture">The source Texture to load the Stream from.</param>
        /// <returns>Return the context containing the texture data.</returns>
        public override TextureLoadingContext Map(AssetLoaderContext assetLoaderContext, ITexture texture)
        {
            var finalPath = $"{assetLoaderContext.BasePath}/{FileUtils.GetFilename(texture.Filename)}";
            if (File.Exists(finalPath))
            {
                var textureLoadingContext = new TextureLoadingContext
                {
                    Context = assetLoaderContext,
                    Stream = File.OpenRead(finalPath),
                    Texture = texture
                };
                Debug.Log($"Found texture at: {finalPath}");
                return textureLoadingContext;
            }
            throw new Exception($"Texture [{texture.Filename}] not found.");
        }
    }
}