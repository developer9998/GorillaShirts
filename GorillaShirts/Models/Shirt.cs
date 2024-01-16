using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class Shirt
    {
        public ShirtPair Pair;

        public string Name, DisplayName, FileName, Author, Description;
        public bool CustomColor, IsVerified, HasAudio, HasLight, HasParticles, Invisibility, Wobble, Billboard;
        public AudioClip Wear, Remove;

        public GameObject RawAsset;
        public List<Sector> SectorList = new();
        public List<Renderer> FurMatchList = new();
        public List<bool> SlotData;

        public Shirt(string name, string displayName, string fileName)
        {
            Name = name;
            DisplayName = displayName;
            FileName = fileName;
        }

        public List<bool> GetSlotData()
        {
            if (RawAsset == null || Pair.myDataJSON == null) return null;

            SlotData ??= new List<bool>()
            {
                RawAsset.GetComponentInChildren<AudioSource>(),
                Billboard,
                Pair.myDataJSON.infoConfig.customColors,
                Pair.myDataJSON.infoConfig.invisibility,
                RawAsset.GetComponentInChildren<Light>(),
                RawAsset.GetComponentInChildren<ParticleSystem>(),
                Wobble
            };

            return SlotData;
        }
    }
}
