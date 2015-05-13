using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

using Roslyn.Services;

using Workspace = Roslyn.Services.Workspace;

namespace roslyncompiler
{
    public static class ExtensionMethods
    {
        public static void test()
        {
        }

        public static IEnumerable<IDocument> GetProjectBy(string projectName, string pathToSolution)
        {
            IWorkspace workspace = Workspace.LoadSolution(pathToSolution);
            foreach (IProject project in workspace.CurrentSolution.Projects)
            {
                if (projectName.Contains(project.DisplayName))
                {
                    return project.Documents.Select(d =>d);
                }
            }

            return null;
        }

        public static IEnumerable<IDocument> GetProjectBy(this IWorkspace workspace, string projectName)
        {
            return (from project in workspace.CurrentSolution.Projects
                    where projectName.Contains(project.DisplayName)
                    select project.Documents.Select(d => d)).FirstOrDefault();
        }

        public static string GetFullNamespace(this ISymbol symbol)
        {
            if ((symbol.ContainingNamespace == null) ||
                 (string.IsNullOrEmpty(symbol.ContainingNamespace.Name)))
            {
                return null;
            }

            // get the rest of the full namespace string
            string restOfResult = symbol.ContainingNamespace.GetFullNamespace();

            string result = symbol.ContainingNamespace.Name;

            if (restOfResult != null)
                // if restOfResult is not null, append it after a period
                result = restOfResult + '.' + result;

            return result;
        }

        public static string Join(this SyntaxTokenList source, string delimiter)
        {
            
            return "";
        }
    }
}
