using GorillaShirts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GorillaShirts.Interfaces
{
    public interface IShirtLoader
    {
        Task<List<Pack>> LoadPacks(string rootDirectory);

        Task<List<Shirt>> LoadShirts(string rootDirectory);
    }
}
