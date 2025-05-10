using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaShirts.Behaviours;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class ShirtAsset : IShirtAsset
    {
        public string FilePath { get; private set; }
        public ShirtDescriptor Descriptor { get; private set; }
        public GameObject Template { get; private set; }
        public List<EShirtComponentType> ComponentTypes { get; private set; }

        public List<bool> TemplateData =>
        [
            Template.GetComponentInChildren<AudioSource>(),
            Descriptor.Billboard,
            Descriptor.CustomColours,
            Descriptor.Invisiblity,
            Template.GetComponentInChildren<Light>(),
            Template.GetComponentInChildren<ParticleSystem>(),
            Descriptor.Wobble
        ];

        public Task<IShirtAsset> Construct(string filePath)
        {
            return null;
        }

        public (string name, string author, string description, string type, string source, string note) GetNavigationInfo()
        {
            return
            (
                Descriptor.DisplayName,
                Descriptor.Author,
                Descriptor.Description,
                "Shirt",
                Descriptor.Pack,
                string.Empty
            );
        }

        public override string ToString()
        {
            return $"{Descriptor.Name} / {Descriptor.Version}";
        }
    }
}
