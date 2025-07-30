﻿using GorillaShirts.Behaviours.Cosmetic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Models.Cosmetic
{
    public interface IGorillaShirt
    {
        string ShirtId { get; }
        FileInfo FileInfo { get; }
        AssetBundle Bundle { get; }
        ShirtDescriptor Descriptor { get; }
        GameObject Template { get; }
        EShirtObject Objects { get; }
        EShirtFeature Features { get; }
        Task CreateShirt(FileInfo file);
    }
}