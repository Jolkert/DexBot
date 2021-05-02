using Discord;
using PokeAPI;
using System.Threading.Tasks;

namespace DexBot.Modules
{
	public interface IParsableModule
	{
		public Task<NamedApiObject> ParseAsync(string parse);
		public Embed GetData(NamedApiObject apiObject);
	}
}
