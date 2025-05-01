using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaShirts.Behaviours;
using UnityEngine;

namespace GorillaShirts.Models
{
    public interface IShirtAsset
    {
        /// <summary>
        /// The file path at which the shirt is located
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The descriptor of the shirt
        /// </summary>
        ShirtDescriptor Descriptor { get; }

        /// <summary>
        /// A usable template for the shirt
        /// </summary>
        GameObject Template { get; }

        Task<IShirtAsset> Construct(string filePath);

        List<bool> TemplateData { get; }
    }
}
