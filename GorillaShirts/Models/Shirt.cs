using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class Shirt(string name, string displayName, string fileName)
    {
        public string Name = name, DisplayName = displayName, FileName = fileName, Author, Description;
        public bool CustomColor, IsVerified, HasAudio, HasLight, HasParticles, Invisibility, Wobble, Billboard;
        public AudioClip Wear, Remove;

        public GameObject ImportedAsset;
        public List<Sector> SectorList = new();

        public List<bool> SlotData => 
        [
            ImportedAsset.GetComponentInChildren<AudioSource>(),
            Billboard,
            CustomColor,
            Invisibility,
            ImportedAsset.GetComponentInChildren<Light>(),
            ImportedAsset.GetComponentInChildren<ParticleSystem>(),
            Wobble
        ];
    }
}
