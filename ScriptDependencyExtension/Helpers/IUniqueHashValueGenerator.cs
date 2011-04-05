using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Helpers
{
	public interface IUniqueHashValueGenerator
	{
		string ComputeHash(string input);
	}
}
