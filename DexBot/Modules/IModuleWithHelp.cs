using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;

namespace DexBot
{
	public interface IModuleWithHelp
	{
		public abstract string ModuleName { get; }

		public abstract Task HelpCommand();
	}
}
