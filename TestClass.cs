using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Roslyn.Services;

using Workspace = Roslyn.Services.Workspace;

namespace roslyncompiler
{
    public class TestClass
    {
        public static IList<FileVisited> FilesVisited = new List<FileVisited>();

        public static IList<EndPoint> EndPointsListed = new List<EndPoint>();

        public IList<Method> methodsUsedInGivenMethod = new List<Method>();

        public IList<Parameter> membersUsedInGivenMethod = new List<Parameter>();

        public IList<ConstructorParameter> ConstructorParamenters;

        public static IWorkspace workspace;
        public void Test(string pathToSolution, string projectName, string className, string methodName)
        {
            workspace = Workspace.LoadSolution(pathToSolution);
            var documents = workspace.GetProjectBy(projectName);
            
            var documentToAnalyze = documents.FirstOrDefault(d => d.DisplayName.ToLower() == className.ToLower());
            if(documentToAnalyze  == null)
            {
                Logger.Log("The document :"+ className + " does not found in project "+ projectName);
                return;
            }
            SyntaxTree tree = CSharpSyntaxTree.ParseText(documentToAnalyze.GetText().ToString());

            var root = (CompilationUnitSyntax)tree.GetRoot();
            var firstMember = root.Members[0];
            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;
            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            var fields = programDeclaration.Members.Where(type => type.IsKind(SyntaxKind.FieldDeclaration)).Select(type => (FieldDeclarationSyntax)type);
            var constructors = programDeclaration.Members.Where(type => type.IsKind(SyntaxKind.ConstructorDeclaration)).Select(type => (ConstructorDeclarationSyntax)type);
            var methods = programDeclaration.Members.Where(type => type.IsKind(SyntaxKind.MethodDeclaration)).Select(type => (MethodDeclarationSyntax)type);

            ConstructorParamenters = null;
            foreach (var constructor in constructors)
            {
                Console.WriteLine(constructor.ParameterList);
                ConstructorParamenters = constructor.Body.Statements.Select(
                    t =>
                    {
                        switch(t.Kind())
                        {
                            case SyntaxKind.ExpressionStatement:
                                if (((ExpressionStatementSyntax)t).Expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
                                {
                                    var q = (AssignmentExpressionSyntax)((ExpressionStatementSyntax)t).Expression;
                                    var pm = new ConstructorParameter
                                    {
                                        Name = q.Left.ToString()

                                    };
                                    return pm;
                                }
                                break;
                        }

                        return null;
                    }).Where(p => p !=null).ToList();
            }

            if (ConstructorParamenters == null)
            {
                return;
            }

            // Constructor paramenters are listed with data type.
            ConstructorParamenters.ToList().ForEach(pm =>
            {
                var field = fields.FirstOrDefault(
                    f => pm.Name.Contains(f.Declaration.Variables[0].Identifier.ToString()));
                pm.Type = field.Declaration.Type.ToString();
            });

            ClassDetails classDetails;

            ConstructorParamenters.ToList().ForEach(p=>
                                                    {
                                                        classDetails = GetClassDetailsFor(p.Type);
                                                            if (classDetails != null)
                                                            {
                                                                p.ClassName = classDetails.ClassName;
                                                                p.NameSpace = classDetails.NameSpace;
                                                            }
                                                    });
            ParseMethod(methods, methodName);

            string mName;
            while (methodsUsedInGivenMethod.Any(method => !method.IsProcessed))
            {
                methodsUsedInGivenMethod.ToList().ForEach(method =>
                {
                    if (!method.IsProcessed)
                    {
                        if (method.Name.Contains("."))
                        {
                            mName = method.Name.Split('.')[1];
                            if (!method.Name.Contains("this") && methods.FirstOrDefault(m => m.Identifier.Text.ToLower() == mName.ToLower()) != null)
                            {
                                // do nothing.
                            }
                            else
                            {
                                ParseMethod(methods, mName);
                            }
                        }
                        else
                        {
                            ParseMethod(methods, method.Name);
                        }
                        method.IsProcessed = true;
                    }
                });
            }


            var matchingArguments = ConstructorParamenters.Where(cp => methodsUsedInGivenMethod.Any(m => m.Name.Contains(cp.Name)));

            matchingArguments.ToList().ForEach(p =>
                                                    {
                                                        var method = methodsUsedInGivenMethod.FirstOrDefault(m => m.Name.Contains(p.Name));
                                                        if(method != null)
                                                        {
                                                            if (method.ShortName != null)
                                                            {
                                                                p.MethodName = method.ShortName;
                                                            }
                                                            if(method.Name != null && method.ShortName == null)
                                                            {
                                                                p.MethodName = method.Name.Split('.').LastOrDefault().Trim();
                                                            }
                                                        }
                                                    });
           
            TestClass testClass;
            matchingArguments.ToList().ForEach(p =>
                                               {
                                                   if (p.ClassName != null && p.NameSpace != null)
                                                   {
                                                       testClass = new TestClass();
                                                       FilesVisited.Add(new FileVisited
                                                                        {
                                                                            ClassName = p.ClassName,
                                                                            MethodName = p.MethodName,
                                                                            NameSpace = p.NameSpace
                                                                        });
                                                       testClass.Test(
                                                           pathToSolution,
                                                           p.NameSpace,
                                                           p.ClassName,
                                                           p.MethodName);
                                                   }
                                               });
        }

        private ClassDetails GetClassDetailsFor(string classType)
        {
            SyntaxTree tree;
             foreach (var project in workspace.CurrentSolution.Projects)
                {
                    if (!project.DisplayName.Contains(""))
                    {
                        continue;
                    }

                    if (project.DisplayName.Contains(""))
                    {
                        continue;
                    }

                    foreach (var doc in project.Documents)
                    {
                        tree = CSharpSyntaxTree.ParseText(doc.GetText().ToString());
                        var root = (CompilationUnitSyntax) tree.GetRoot();
                        if (!root.Members.Any() ||
                            !root.Members[0].IsKind(SyntaxKind.NamespaceDeclaration))
                        {
                            continue;
                        }
                        var firstMember = root.Members[0];
                        var helloWorldDeclaration = (NamespaceDeclarationSyntax) firstMember;
                        if (helloWorldDeclaration.Members.Count > 0 &&
                            helloWorldDeclaration.Members[0].IsKind(SyntaxKind.ClassDeclaration))
                        {
                            var programDeclaration = (ClassDeclarationSyntax) helloWorldDeclaration.Members[0];
                            if (programDeclaration.Keyword.Value == "class")
                            {
                                if (programDeclaration.BaseList != null &&
                                    // change here : programDeclaration.BaseList.Types.Any(b => b.ToString() == classType))
                                    programDeclaration.BaseList.Types.Any(b => MatchBaseType(b.ToString(), classType)))
                                {
                                    return new ClassDetails
                                           {
                                               ClassName = programDeclaration.Identifier.Value + ".cs",
                                               NameSpace = helloWorldDeclaration.Name.ToString()
                                           };
                                }
                            }
                        }
                    }
                }

            return null;
        }

        private string ParseValue(string value)
        {
            if (value.Contains("<"))
            {
                var x = value.Split('<');
                var y = x[1].Remove(x[1].Length - 1, 1);
                if (y.Contains("."))
                {
                    var t111 = y.Split('.');
                    return (t111[t111.Length - 1].Trim());
                }
                return y;
            }

            if (value.Contains("."))
            {
                var t111 = value.Split('.');
                return (t111[t111.Length - 1].Trim());
            }

            return value;
        }
	
        private bool MatchBaseType(string type1, string type2)
        {
            if(type1.Contains("IQueryFor") && type2.Contains("IQueryFor"))
            {
                var t1 = type1.Split(',');
                var t2 = type2.Split(',');
                IList<string> t1List = new List<string>();
                IList<string> t2List = new List<string>();
                t1[0] = t1[0].Replace("IQueryFor<", "").Trim();
                t1[1] = t1[1].Remove(t1[1].Length - 1, 1).Trim();
                t1List.Add(ParseValue(t1[0]).Trim());
                t1List.Add(ParseValue(t1[1].Trim()));

                t2[0] = t2[0].Replace("IQueryFor<", "");
                t2[1] = t2[1].Remove(t2[1].Length - 1, 1);
                t2List.Add(ParseValue(t2[0].Trim()));
                t2List.Add(ParseValue(t2[1].Trim()));
                string r1 = t1List.Aggregate("",
                                                 (current,
                                                  t) => current + t);

                string r2 = t2List.Aggregate("",
                                                 (current,
                                                  t) => current + t);
                return r1 == r2;
            }

            return type1 == type2;
           
        }

        public void ParseMethod(IEnumerable<MethodDeclarationSyntax> methods, string method)
        {
            var testmethod = methods.FirstOrDefault(m => m.Identifier.Text.ToLower() == method.ToLower());

            if (testmethod == null)
            {
                return;
            }

            // Lists member variables used in given method.
            testmethod.Body.Statements.ToList().ForEach(ResolveStatement);
        }

        public void ResolveSwitchStatement(SwitchStatementSyntax switchStatementSyntax)
        {
            switchStatementSyntax.Sections.ToList()
                .ForEach(section => section.Statements.ToList()
                                        .ForEach(ResolveStatement));
        }

        public void ResolveStatement(StatementSyntax statement)
        {
            switch (statement.Kind())
            {
                case SyntaxKind.TryStatement:
                    ResolveTryStatement((TryStatementSyntax)statement);
                    break;
                case SyntaxKind.IfStatement:
                    ResolveIfStatement((IfStatementSyntax)statement);
                    break;
                case SyntaxKind.ExpressionStatement:
                    ResolveExpressionStatement((ExpressionStatementSyntax)statement);
                    break;
                case SyntaxKind.ReturnStatement:
                    ResolveReturnStatement((ReturnStatementSyntax)statement);
                    break;
                case SyntaxKind.LocalDeclarationStatement:
                    ResolveLocalDeclarationStatement((LocalDeclarationStatementSyntax)statement);
                    break;
                case SyntaxKind.SwitchStatement:
                    ResolveSwitchStatement((SwitchStatementSyntax)statement);
                    break;
            }
        }

        public void ResolveIfStatement(IfStatementSyntax ifStatementSyntax)
        {
            ResolveIfCondition(ifStatementSyntax);

            ResolveIfStatementBlock(ifStatementSyntax);

            ResolveElseCondition(ifStatementSyntax);

        }

        public void ResolveIfStatementBlock(IfStatementSyntax ifStatementSyntax)
        {
            ((BlockSyntax)ifStatementSyntax.Statement).Statements.ToList()
                .ForEach(ResolveStatement);
        }

        public void ResolveIfCondition(IfStatementSyntax ifStatementSyntax)
        {
            switch (ifStatementSyntax.Condition.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    ResolveInvocationExpressionSyntax((InvocationExpressionSyntax)ifStatementSyntax.Condition);
                    break;
            }
        }

        public void ResolveElseCondition(IfStatementSyntax ifStatementSyntax)
        {
            if (ifStatementSyntax.Else == null)
            {
                return;
            }
            switch (ifStatementSyntax.Else.Statement.Kind())
            {
                    case SyntaxKind.Block:
                    ((BlockSyntax)ifStatementSyntax.Else.Statement).Statements.ToList()
                    .ForEach(
                        ResolveStatement);
                    break;

                case SyntaxKind.IfStatement:
                    ResolveIfStatement((IfStatementSyntax)ifStatementSyntax.Else.Statement);
                    break;
            }
        }

        public void ResolveTryStatement(TryStatementSyntax tryStatement)
        {
            tryStatement.Block.Statements.ToList().ForEach(ResolveStatement);
        }

        public void ResolveReturnStatement(ReturnStatementSyntax returnStatement)
        {
            if (returnStatement.Expression == null)
            {
                return;
            }

            switch (returnStatement.Expression.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    ResolveInvocationExpressionSyntax((InvocationExpressionSyntax)returnStatement.Expression);
                    break;

            }
        }

        public void ResolveInvocationExpressionSyntax(InvocationExpressionSyntax invocationExpression)
        {
            ParseInvocationSyntax(invocationExpression);
            invocationExpression.ArgumentList.Arguments.ToList()
                .ForEach(a =>
                {
                    switch (a.Expression.Kind())
                    {
                        case SyntaxKind.SimpleMemberAccessExpression:
                            ParseSimpleMemberAccessExpressionSyntax((MemberAccessExpressionSyntax)a.Expression);
                            break;
                        case SyntaxKind.InvocationExpression:
                            ResolveInvocationExpressionSyntax((InvocationExpressionSyntax)a.Expression);
                            break;
                    }
                });
        }

        public void ResolveLocalDeclarationStatement(LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            if (localDeclarationStatement.Declaration.Variables[0].Initializer != null)
            {
                ParseExpressionSyntax(localDeclarationStatement.Declaration.Variables[0].Initializer.Value);
            }
        }

        public void ResolveExpressionStatement(ExpressionStatementSyntax expressionStatementSyntax)
        {
            switch (expressionStatementSyntax.Expression.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    var expression = (AssignmentExpressionSyntax)expressionStatementSyntax.Expression;
                    ParseExpressionSyntax(expression.Left);
                    ParseExpressionSyntax(expression.Right);
                    break;
                case SyntaxKind.InvocationExpression:
                    ParseExpressionSyntax(expressionStatementSyntax.Expression);
                    break;
            }
        }

        public void ResolveHandleExternalServiceCall(InvocationExpressionSyntax expressionSyntax)
        {
            string argumentType, operationName;
            EndPoint endPoint = new EndPoint();
            expressionSyntax.ArgumentList.Arguments.ToList()
                .ForEach(argument =>
                         {
                             switch(argument.Expression.Kind())
                             {
                                 case SyntaxKind.SimpleMemberAccessExpression:
                                     argumentType = argument.Expression.ToString();
                                     endPoint.EndPointUrl =
                                         MappingGeneration.GetEndPoint(ConstructorParamenters.FirstOrDefault(p => p.Name == argumentType).Type);
                                     break;
                                case SyntaxKind.SimpleLambdaExpression:
                                     operationName = ((InvocationExpressionSyntax)((SimpleLambdaExpressionSyntax)argument.Expression).Body).Expression.ToString();
                                     endPoint.Operation = operationName.Split('.').LastOrDefault();
                                     break;
                             }
                         });

            Logger.Log(endPoint);
            EndPointsListed.Add(new EndPoint
                                {
                                    EndPointUrl = endPoint.EndPointUrl,
                                    Operation = endPoint.Operation
                                });
        }

        public void ParseInvocationSyntax(InvocationExpressionSyntax invocationExpression)
        {
            if (invocationExpression.Expression.ToString().Contains(""))
            {
                ResolveHandleExternalServiceCall(invocationExpression);
                return;
            }

            methodsUsedInGivenMethod.Add(new Method
            {
                Name = (invocationExpression).Expression.ToString()
            }
                );
        }

        public void ParseSimpleMemberAccessExpressionSyntax(MemberAccessExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax.ToString().Contains("this"))
            {
                membersUsedInGivenMethod.Add(new Parameter
                {
                    Name = expressionSyntax.ToString()
                });
            }
        }

        public void ParseExpressionSyntax(ExpressionSyntax expressionSyntax)
        {
            switch (expressionSyntax.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    if (((InvocationExpressionSyntax)expressionSyntax).Expression.ToString().Contains(""))
                    {
                        ResolveHandleExternalServiceCall((InvocationExpressionSyntax)expressionSyntax);
                        return;
                    }
                    Method method = new Method();
                    method.Name = ((InvocationExpressionSyntax)expressionSyntax).Expression.ToString();
                    if (((InvocationExpressionSyntax)expressionSyntax).Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        method.ShortName = ((MemberAccessExpressionSyntax) ((InvocationExpressionSyntax) expressionSyntax).Expression).Name.Identifier.ValueText;
                    }
                    method.Kind = (((InvocationExpressionSyntax) expressionSyntax).Expression).Kind().ToString();
                    methodsUsedInGivenMethod.Add(method);
                    break;
                case SyntaxKind.SimpleMemberAccessExpression:
                    if (expressionSyntax.ToString().Contains("this"))
                    {
                        membersUsedInGivenMethod.Add(new Parameter
                        {
                            Name = expressionSyntax.ToString()
                        });
                    }
                    break;
                case SyntaxKind.ConditionalExpression:
                    ResolveConditionalExpression((ConditionalExpressionSyntax) expressionSyntax);
                    break;
            }
        }

        public void ResolveConditionalExpression(ConditionalExpressionSyntax conditionalExpression)
        {
            HandleConditionalExpression(conditionalExpression.WhenFalse);
            HandleConditionalExpression(conditionalExpression.WhenTrue);
        }

        private void HandleConditionalExpression(ExpressionSyntax conditionalExpression)
        {
            switch (conditionalExpression.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    ResolveInvocationExpressionSyntax((InvocationExpressionSyntax)(conditionalExpression));
                    break;
            }
        }
    }
}
