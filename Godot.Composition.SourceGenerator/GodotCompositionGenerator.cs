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
                    var componentClassNodes = declaredClass
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .FirstOrDefault(a => a.DescendantTokens().Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) && semanticModel.GetTypeInfo(dt.Parent).Type.Name == "ComponentAttribute"))
                        ?.DescendantTokens()
                        ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                        ?.ToList();

                    var entityClassNodes = declaredClass
                        .DescendantNodes()
                        .OfType<AttributeSyntax>()
                        .FirstOrDefault(a => a.DescendantTokens().Any(dt => dt.IsKind(SyntaxKind.IdentifierToken) && semanticModel.GetTypeInfo(dt.Parent).Type.Name == "EntityAttribute"))
                        ?.DescendantTokens()
                        ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                        ?.ToList();

                    if (componentClassNodes != null)
                    {
                        var srcBuilder = new StringBuilder();
                        var attributeParentClass = semanticModel.GetTypeInfo(componentClassNodes.Last().Parent);
                        var classTypeSymbol = semanticModel.GetDeclaredSymbol(declaredClass);

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
            var namespaceSymbol = classTypeSymbol.ContainingNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");
            srcBuilder.AppendLine();

            if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
            {
                var classNs = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                srcBuilder.AppendLine("namespace " + classNs);
            }

            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine("    public partial class " + classTypeSymbol.Name + " : Godot.Composition.IComponent");
            srcBuilder.AppendLine("    {");
            srcBuilder.AppendLine("        protected " + attributeParentClass.Type.Name + " parent;");
            srcBuilder.AppendLine();

            WriteInitializeComponentMethod(ref srcBuilder, attributeParentClass.Type);

            srcBuilder.AppendLine("    }");
            srcBuilder.AppendLine("}");
        }

        private void WriteInitializeComponentMethod(ref StringBuilder srcBuilder, ITypeSymbol type)
        {
            srcBuilder.AppendLine("        protected void InitializeComponent()");
            srcBuilder.AppendLine("        {");
            srcBuilder.AppendLine("            parent = GetParent<" + type.Name + ">();");
            srcBuilder.AppendLine("        }");
        }

        private void WriteEntityClass(ref StringBuilder srcBuilder, INamedTypeSymbol classTypeSymbol)
        {
            var namespaceSymbol = classTypeSymbol.ContainingNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");
            srcBuilder.AppendLine();

            if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
            {
                var classNs = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                srcBuilder.AppendLine("namespace " + classNs);
            }

            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine("    public partial class " + classTypeSymbol.Name + " : Godot.Composition.IEntity");
            srcBuilder.AppendLine("    {");
            srcBuilder.AppendLine("        protected Godot.Composition.ComponentContainer container = new Godot.Composition.ComponentContainer();");
            srcBuilder.AppendLine();

            WriteInitializeEntityMethod(ref srcBuilder);
            srcBuilder.AppendLine();

            WriteHasComponentMethod(ref srcBuilder);
            srcBuilder.AppendLine();

            WriteGetComponentMethod(ref srcBuilder);
            srcBuilder.AppendLine();

            WriteComponentsMethod(ref srcBuilder);
            srcBuilder.AppendLine();

            srcBuilder.AppendLine("    }");
            srcBuilder.AppendLine("}");
        }

        private void WriteInitializeEntityMethod(ref StringBuilder srcBuilder)
        {
            srcBuilder.AppendLine("        protected void InitializeEntity()");
            srcBuilder.AppendLine("        {");
            srcBuilder.AppendLine("            container.AddEntityComponents(this);");
            srcBuilder.AppendLine("        }");
        }

        private void WriteHasComponentMethod(ref StringBuilder srcBuilder)
        {
            srcBuilder.AppendLine("        public bool HasComponent<T>() where T : Godot.Node");
            srcBuilder.AppendLine("        {");
            srcBuilder.AppendLine("            return container.HasComponent<T>();");
            srcBuilder.AppendLine("        }");
        }

        private void WriteGetComponentMethod(ref StringBuilder srcBuilder)
        {
            srcBuilder.AppendLine("        public T GetComponent<T>() where T : Godot.Node");
            srcBuilder.AppendLine("        {");
            srcBuilder.AppendLine("            return container.GetComponent<T>();");
            srcBuilder.AppendLine("        }");
        }

        private void WriteComponentsMethod(ref StringBuilder srcBuilder)
        {
            srcBuilder.AppendLine("        public System.Collections.Generic.IEnumerable<Godot.Composition.IComponent> Components()");
            srcBuilder.AppendLine("        {");
            srcBuilder.AppendLine("            return container.AsEnumerable();");
            srcBuilder.AppendLine("        }");
        }
    }
}
