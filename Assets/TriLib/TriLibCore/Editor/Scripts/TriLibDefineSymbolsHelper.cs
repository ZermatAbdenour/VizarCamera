﻿using UnityEditor;

namespace TriLibCore.Editor
{
    public static class TriLibDefineSymbolsHelper
    {
        public static bool IsSymbolDefined(string targetDefineSymbol)
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineSymbolsArray = defineSymbols.Split(';');
            for (var i = 0; i < defineSymbolsArray.Length; i++)
            {
                var defineSymbol = defineSymbolsArray[i];
                var trimmedDefineSymbol = defineSymbol.Trim();
                if (trimmedDefineSymbol == targetDefineSymbol)
                {
                    return true;
                }
            }

            return false;
        }

        public static void UpdateSymbol(string targetDefineSymbol, bool value)
        {
            var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineSymbolsArray = defineSymbols.Split(';');
            var newDefineSymbols = string.Empty;
            var isDefined = false;
            for (var i = 0; i < defineSymbolsArray.Length; i++)
            {
                var defineSymbol = defineSymbolsArray[i];
                var trimmedDefineSymbol = defineSymbol.Trim();
                if (trimmedDefineSymbol == targetDefineSymbol)
                {
                    if (!value)
                    {
                        continue;
                    }

                    isDefined = true;
                }

                newDefineSymbols += string.Format("{0};", trimmedDefineSymbol);
            }

            if (value && !isDefined)
            {
                newDefineSymbols += string.Format("{0};", targetDefineSymbol);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefineSymbols);
        }
    }
}