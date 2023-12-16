using System;
using System.Collections.Generic;
using System.IO;
using CoDLuaDecompiler.AssetExporter.Util;

namespace CoDLuaDecompiler.AssetExporter.Games;

public class BlackOps4 : IGame
{
    public override string ExportFolder => "BO4/";
    
    public static long[] GameOffsets =
    {
        0x88788D0,
    };
    
    struct AssetPool
    {
        public long PoolPointer { get; set; }
        public int AssetSize { get; set; }
        public int PoolSize { get; set; }
        public int Padding { get; set; }
        public int AssetCount { get; set; }
        public long NextSlot { get; set; }
    }

    private struct LuaFile
    {
        public long Hash { get; set; }
        public ulong NullPointer { get; set; }
        public Int32 DataSize { get; set; }
        public Int32 UnknownHash1 { get; set; }
        public long StartLocation { get; set; }
    }
    public override unsafe List<LuaFileData> LoadLuaFiles(bool isMP = true)
    {
        // Get Base Address for ASLR and Scans
        long baseAddress = AssetExport.Reader.GetBaseAddress();

        foreach (var gameOffset in GameOffsets)
        {
            var xmodelPoolData = AssetExport.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset + sizeof(AssetPool) * 0x4);
            
            // Check XModel Hash
            if (AssetExport.Reader.ReadUInt64(xmodelPoolData.PoolPointer) == 0x04647533e968c910)
            {
                var luaPoolData = AssetExport.Reader.ReadStruct<AssetPool>(baseAddress + gameOffset + sizeof(AssetPool) * 0x67);

                return FetchFiles(luaPoolData);
            }
        }
        
        var dbAssetsScan = AssetExport.Reader.FindBytes(new byte?[] { 0x48, 0x89, 0x5C, 0x24, null, 0x57, 0x48, 0x83, 0xEC, null, 0x0F, 0xB6, 0xF9, 0x48, 0x8D, 0x05 }, baseAddress, baseAddress + AssetExport.Reader.GetModuleMemorySize(), true);

        // Check that we had hits
        if (dbAssetsScan.Length > 0)
        {
            var assetPoolAddress = AssetExport.Reader.ReadUInt32(dbAssetsScan[0] + 0x10) + dbAssetsScan[0] + 0x14;
            var xmodelPoolData      = AssetExport.Reader.ReadStruct<AssetPool>(assetPoolAddress + sizeof(AssetPool) * 0x4);
            
            // Check XModel Hash
            if (AssetExport.Reader.ReadUInt64(xmodelPoolData.PoolPointer) == 0x04647533e968c910)
            {
                var luaPoolData = AssetExport.Reader.ReadStruct<AssetPool>(assetPoolAddress + sizeof(AssetPool) * 0x67);

                return FetchFiles(luaPoolData);
            }
        }

        return null;
    }

    private List<LuaFileData> FetchFiles(AssetPool luaPoolData)
    {
        var filesList = new List<LuaFileData>();
        for (int i = 0; i < luaPoolData.PoolSize; i++)
        {
            var luaFile = AssetExport.Reader.ReadStruct<LuaFile>(luaPoolData.PoolPointer + (i * luaPoolData.AssetSize));

            if (luaFile.DataSize == 0 || luaFile.StartLocation == 0)
                continue;
            
            var luaFileData = AssetExport.Reader.ReadBytes(luaFile.StartLocation, luaFile.DataSize);
                    
            filesList.Add(new LuaFileData()
            {
                Reader = new BinaryReader(new MemoryStream(luaFileData)),
                Hash = luaFile.Hash & 0x7FFFFFFFFFFFFFFF,
            });
        }

        return filesList;
    }
}
