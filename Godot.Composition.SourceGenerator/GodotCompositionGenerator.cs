using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Godot.Composition.SourceGenerator
{
    [Generator]
    public class GodotCompositionGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var classWithAttributes = context.Compilation.SyntaxTrees.Where(st => st.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Any(p => p.DescendantNodes().OfType<AttributeSyntax>().Any()));

            foreach (SyntaxTree tree in classWithAttributes)
            {
                var semanticModel = context.Compilation.GetSemanticModel(tree);

                foreach (var declaredClass in tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(cd => cd.DescendantNodes().OfType<AttributeSyntax>().Any()))
                {
                    if (declaredClass == null)
                        continue;

                    var componentClassNodes = declaredClass
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .FirstOrDefault(a => a.DescendantTokens().Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) && dt.Parent != null && semanticModel.GetTypeInfo(dt.Parent).Type?.Name == "ComponentAttribute"))
                        ?.DescendantTokens()
                        ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                        ?.ToList();

                    var componentRefAttributeNodes = declaredClass
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .Where(a => a.DescendantTokens().Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) && dt.Parent != null && semanticModel.GetTypeInfo(dt.Parent).Type?.Name == "ComponentDependencyAttribute")).ToList();

                    var entityClassNodes = declaredClass
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .FirstOrDefault(a => a.DescendantTokens().Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) && dt.Parent != null && semanticModel.GetTypeInfo(dt.Parent).Type?.Name == "EntityAttribute"))
                        ?.DescendantTokens()
                        ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                        ?.ToList();

                    if (componentClassNodes != null)
                    {
                        var srcBuilder = new StringBuilder();
                        var attributeParentClass = semanticModel.GetTypeInfo(componentClassNodes.Last().Parent);
                        var classTypeSymbol = semanticModel.GetDeclaredSymbol(declaredClass);

                        if (classTypeSymbol == null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(id: "GDCOMP0001",
                                    title: "blah blah",
                                    messageFormat: "blah blah",
                                    category: "Usage",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true,
                                    "blah blah"),
                                declaredClass.GetLocation(),
                                declaredClass.SyntaxTree.FilePath));
                        }

                        List<Tuple<string, string>> componentRefNames = new List<Tuple<string, string>>();

                        foreach (var attribute in componentRefAttributeNodes)
                        {
                            var componentRefClassNodes = attribute
                                .DescendantTokens()
                                ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                                ?.ToList();
                            if (componentRefClassNodes != null && componentRefClassNodes.Count > 0)
                            {
                                string typeName = semanticModel.GetTypeInfo(componentRefClassNodes.Last().Parent).Type.Name;
                                componentRefNames.Add(new Tuple<string, string>(typeName, typeName.Substring(0, 1).ToLower() + typeName.Substring(1)));
                            }
                        }

                        WriteComponentClass(ref srcBuilder, ref attributeParentClass, classTypeSymbol, componentRefNames);

                        context.AddSource($"{declaredClass.Identifier}_IComponent.g", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
                    }
                    else if (entityClassNodes != null)
                    {
                        var srcBuilder = new StringBuilder();
                        var classTypeSymbol = semanticModel.GetDeclaredSymbol(declaredClass);

                        WriteEntityClass(ref srcBuilder, classTypeSymbol);

                        context.AddSource($"{declaredClass.Identifier}_IEntity.g", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
                    }
                }
            }
        }

        private void WriteComponentClass(ref StringBuilder srcBuilder, ref TypeInfo attributeParentClass, INamedTypeSymbol classTypeSymbol, List<Tuple<string, string>> componentRefNames)
        {
            var classNamespaceSymbol = classTypeSymbol.ContainingNamespace;
            var attributeNamespaceSymbol = attributeParentClass.Type.ContainingNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");

            if (attributeNamespaceSymbol != null && !attributeNamespaceSymbol.IsGlobalNamespace)
            {
                var attributeTypeNs = attributeNamespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                if (!string.IsNullOrEmpty(attributeTypeNs) && attributeTypeNs != "Godot")
                    srcBuilder.AppendLine("using " + attributeTypeNs + ";");
            }

            srcBuilder.AppendLine();

            if (classNamespaceSymbol != null && !classNamespaceSymbol.IsGlobalNamespace)
            {
                var classNs = classNamespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                if (!string.IsNullOrEmpty(classNs))
                {
                    srcBuilder.AppendLine("namespace " + classNs + ";");
                    srcBuilder.AppendLine();
                }
            }

            srcBuilder.AppendLine("public partial class " + classTypeSymbol.Name + " : Godot.Composition.IComponent");
            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine("    protected " + attributeParentClass.Type.Name + " parent;");

            foreach (var compRefName in componentRefNames)
                srcBuilder.AppendLine("    protected " + compRefName.Item1 + " " + compRefName.Item2 + ";");

            srcBuilder.AppendLine();

            WriteInitializeComponentMethod(ref srcBuilder, attributeParentClass.Type, "    ");
            srcBuilder.AppendLine();
            WriteComponentResolveDependenciesMethod(ref srcBuilder, componentRefNames, "    ");

            srcBuilder.AppendLine("}");
        }

        private void WriteInitializeComponentMethod(ref StringBuilder srcBuilder, ITypeSymbol type, string indent)
        {
            srcBuilder.AppendLine(indent + "protected void InitializeComponent()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    parent = GetParent<" + type.Name + ">();");
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    if (parent is IEntity e)");
            srcBuilder.AppendLine(indent + "    {");
            srcBuilder.AppendLine(indent + "        if (e.IsEntityInitialized())");
            srcBuilder.AppendLine(indent + "            e.ResolveDependencies();");
            srcBuilder.AppendLine(indent + "    }");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteComponentResolveDependenciesMethod(ref StringBuilder srcBuilder, List<Tuple<string, string>> componentRefNames, string indent)
        {
            srcBuilder.AppendLine(indent + "public void ResolveDependencies()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    if (parent != null && parent is IEntity ent)");
            srcBuilder.AppendLine(indent + "    {");

            foreach (var compRefName in componentRefNames)
            {
                srcBuilder.AppendLine(indent + "        " + compRefName.Item2 + " = ent.GetComponent<" + compRefName.Item1 + ">();");
            }
            
            srcBuilder.AppendLine(indent + "    }");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteEntityClass(ref StringBuilder srcBuilder, INamedTypeSymbol classTypeSymbol)
        {
            var namespaceSymbol = classTypeSymbol.ContainingNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");
            srcBuilder.AppendLine("using System.Linq;");
            srcBuilder.AppendLine();

            if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
            {
                var classNs = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                if (!string.IsNullOrEmpty(classNs))
                {
                    srcBuilder.AppendLine("namespace " + classNs + ";");
                    srcBuilder.AppendLine();
                }
            }

            srcBuilder.AppendLine("public partial class " + classTypeSymbol.Name + " : Godot.Composition.IEntity");
            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine("    protected Godot.Composition.ComponentContainer container = new Godot.Composition.ComponentContainer();");
            srcBuilder.AppendLine("    private bool isEntityInitialized = false;");
            srcBuilder.AppendLine();

            WriteIsEntityInitializedMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteInitializeEntityMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteEntityResolveDependenciesMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteHasComponentMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteGetComponentMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteGetComponentByNameMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            WriteComponentsMethod(ref srcBuilder, "    ");
            srcBuilder.AppendLine();

            srcBuilder.AppendLine("}");
        }

        private void WriteIsEntityInitializedMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public bool IsEntityInitialized()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return isEntityInitialized;");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteInitializeEntityMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public void InitializeEntity()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    if (isEntityInitialized)");
            srcBuilder.AppendLine(indent + "        return;");
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    container.AddEntityComponents(this);");
            srcBuilder.AppendLine(indent + "    ResolveDependencies();");
            srcBuilder.AppendLine(indent + "    isEntityInitialized = true;");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteEntityResolveDependenciesMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public void ResolveDependencies()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    foreach (var component in container)");
            srcBuilder.AppendLine(indent + "        component.ResolveDependencies();");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteHasComponentMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public bool HasComponent<T>() where T : IComponent");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.HasComponent<T>();");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteGetComponentMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public T GetComponent<T>() where T : IComponent");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.GetComponent<T>();");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteGetComponentByNameMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public T GetComponentByName<T>(string name) where T : IComponent");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.GetComponentByName<T>(name);");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteComponentsMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public System.Collections.Generic.IEnumerable<Godot.Composition.IComponent> Components()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.AsEnumerable();");
            srcBuilder.AppendLine(indent + "}");
        }
    }
}
