using System;
using System.Collections.Generic;
using System.IO;
using TriLibCore.SFB;
using TriLibCore.Utils;

namespace TriLibCore.Mappers
{    /// <summary>Represents a class used to load external data from a series of selected files.</summary>
    public class FilePickerExternalDataMapper : ExternalDataMapper
    {
        /// <inheritdoc />
        public override Stream Map(AssetLoaderContext assetLoaderContext, string originalFilename, out string finalPath)
        {
            if (!string.IsNullOrEmpty(originalFilename))
            {
                if (assetLoaderContext.CustomData is IEnumerable<ItemWithStream> itemsWithStream)
                {
                    var shortFileName = FileUtils.GetShortFilename(originalFilename).ToLowerInvariant();
                    foreach (var itemWithStream in itemsWithStream)
                    {
                        if (!itemWithStream.HasData)
                        {
                            continue;
                        }

                        var checkingFileShortName = FileUtils.GetShortFilename(itemWithStream.Name).ToLowerInvariant();
                        if (shortFileName == checkingFileShortName)
                        {
                            finalPath = itemWithStream.Name;
                            return itemWithStream.OpenStream();
                        }
                    }
                }
                else
                {
                    throw new Exception("Missing custom context data.");
                }
            }
            finalPath = null;
            return null;
        }
    }
}