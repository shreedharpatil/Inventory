using System.Collections.Generic;

namespace roslyncompiler
{
    public class Run
    {
        public static void Main(string[] args)
        {
            string pathToSolution = @"";
            string projectName = "";
            string className = "" + "Controller.cs";
            string methodName = "";
            string webconfigPath = null;
            if(args.Length == 5)
            {
                pathToSolution = args[0];
                className = args[1];
                methodName = args[2];
                webconfigPath = args[3];
                projectName = args[4];
            }
            MappingGeneration.Generate(webconfigPath);
            Logger.Log("---------------------------------------" + "End points used in Controller :" + className + ", method:" + methodName+ "---------------------------------------");
            TestClass.FilesVisited = new List<FileVisited>();
            new TestClass().Test(pathToSolution, projectName, className, methodName);
            Logger.Log("------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }
    }
}
