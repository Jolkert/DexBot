using System.Threading.Tasks;

namespace DexBot
{
	public interface IModuleWithHelp
	{
		public abstract string ModuleName { get; }

		public abstract Task HelpCommand();
	}
}
