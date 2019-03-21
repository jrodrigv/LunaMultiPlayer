﻿using LmpCommon.ModFile.Structure;
using LmpCommon.Xml;
using Server.Context;
using Server.Log;
using System;

namespace Server.System
{
    public class ModFileSystem
    {
        public static ModControlStructure ModControl { get; private set; }

        public static void GenerateNewModFile()
        {
            var defaultModFile = new ModControlStructure();
            defaultModFile.SetDefaultAllowedParts();

            FileHandler.WriteToFile(ServerContext.ModFilePath, LunaXmlSerializer.SerializeToXml(defaultModFile));
        }

        public static void LoadModFile()
        {
            try
            {
                ModControl = LunaXmlSerializer.ReadXmlFromPath<ModControlStructure>(ServerContext.ModFilePath);
            }
            catch (Exception)
            {
                LunaLog.Error("Cannot read LMPModControl file. Will load the default one. Please regenerate it");
                ModControl = new ModControlStructure();
            }
        }
    }
}