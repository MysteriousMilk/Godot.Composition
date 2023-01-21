using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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

                        WriteComponentClass(ref srcBuilder, ref attributeParentClass, classTypeSymbol);

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

        private void WriteComponentClass(ref StringBuilder srcBuilder, ref TypeInfo attributeParentClass, INamedTypeSymbol classTypeSymbol)
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

            bool hasNamespace = false;
            if (classNamespaceSymbol != null && !classNamespaceSymbol.IsGlobalNamespace)
            {
                var classNs = classNamespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                if (!string.IsNullOrEmpty(classNs))
                {
                    hasNamespace = true;
                    srcBuilder.AppendLine("namespace " + classNs);
                    srcBuilder.AppendLine("{");
                }
            }

            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "public partial class " + classTypeSymbol.Name + " : Godot.Composition.IComponent");
            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "{");
            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "    protected " + attributeParentClass.Type.Name + " parent;");
            srcBuilder.AppendLine();

            WriteInitializeComponentMethod(ref srcBuilder, attributeParentClass.Type, hasNamespace ? "        " : "    ");

            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "}");

            if (hasNamespace)
                srcBuilder.AppendLine("}");
        }

        private void WriteInitializeComponentMethod(ref StringBuilder srcBuilder, ITypeSymbol type, string indent)
        {
            srcBuilder.AppendLine(indent + "protected void InitializeComponent()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    parent = GetParent<" + type.Name + ">();");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteEntityClass(ref StringBuilder srcBuilder, INamedTypeSymbol classTypeSymbol)
        {
            var namespaceSymbol = classTypeSymbol.ContainingNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");
            srcBuilder.AppendLine("using System.Linq;");
            srcBuilder.AppendLine();

            bool hasNamespace = false;
            if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
            {
                var classNs = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

                if (!string.IsNullOrEmpty(classNs))
                {
                    hasNamespace = true;
                    srcBuilder.AppendLine("namespace " + classNs);
                    srcBuilder.AppendLine("{");
                }
            }

            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "public partial class " + classTypeSymbol.Name + " : Godot.Composition.IEntity");
            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "{");
            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "    protected Godot.Composition.ComponentContainer container = new Godot.Composition.ComponentContainer();");
            srcBuilder.AppendLine();

            WriteInitializeEntityMethod(ref srcBuilder, hasNamespace ? "        " : "    ");
            srcBuilder.AppendLine();

            WriteHasComponentMethod(ref srcBuilder, hasNamespace ? "        " : "    ");
            srcBuilder.AppendLine();

            WriteGetComponentMethod(ref srcBuilder, hasNamespace ? "        " : "    ");
            srcBuilder.AppendLine();

            WriteComponentsMethod(ref srcBuilder, hasNamespace ? "        " : "    ");
            srcBuilder.AppendLine();

            srcBuilder.AppendLine(hasNamespace ? "    " : "" + "}");

            if (hasNamespace)
                srcBuilder.AppendLine("}");
        }

        private void WriteInitializeEntityMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "protected void InitializeEntity()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    container.AddEntityComponents(this);");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteHasComponentMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public bool HasComponent<T>() where T : Godot.Node");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.HasComponent<T>();");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteGetComponentMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public T GetComponent<T>() where T : Godot.Node");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    return container.GetComponent<T>();");
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
