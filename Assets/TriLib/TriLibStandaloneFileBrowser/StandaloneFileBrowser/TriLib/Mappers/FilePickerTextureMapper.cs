#pragma warning disable 672

using System;
using System.Collections.Generic;
using TriLibCore.SFB;
using TriLibCore.Interfaces;
using TriLibCore.Utils;

namespace TriLibCore.Mappers
{
    /// <summary>Represents a class used to load Textures from a list of selected files.</summary>
    public class FilePickerTextureMapper : TextureMapper
    {
        /// <inheritdoc />
        public override TextureLoadingContext Map(AssetLoaderContext assetLoaderContext, ITexture texture)
        {
            if (string.IsNullOrEmpty(texture.Filename))
            {
                return null;
            }
            if (assetLoaderContext.CustomData is IEnumerable<ItemWithStream> itemsWithStream)
            {
                var shortFileName = FileUtils.GetShortFilename(texture.Filename).ToLowerInvariant();
                foreach (var itemWithStream in itemsWithStream)
                {
                    if (!itemWithStream.HasData)
                    {
                        continue;
                    }
                    var checkingFileShortName = FileUtils.GetShortFilename(itemWithStream.Name).ToLowerInvariant();
                    if (shortFileName == checkingFileShortName)
                    {
                        var textureLoadingContext = new TextureLoadingContext
                        {
                            Context = assetLoaderContext,
                            Stream = itemWithStream.OpenStream(),
                            Texture = texture
                        };
                        return textureLoadingContext;
                    }
                }
            }
            else
            {
                throw new Exception("Missing custom context data.");
            }
            return null;
        }
    }
}