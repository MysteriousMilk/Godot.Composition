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
    public class CompositionComponentRef
    {
        public string TypeName { get; set; }
        public string TypeNamespace { get; set; }
        public string VariableName { get; set; }

        public CompositionComponentRef(string type, string ns, string varName)
        {
            TypeName = type;
            TypeNamespace = ns;
            VariableName = varName;
        }
    }

    public class EntityClassData
    {
        public ClassDeclarationSyntax ClassSyntax {  get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public bool HasEntityBaseClass { get; set; }
        public List<string> BaseClassTypes { get; set; }

        public EntityClassData(ClassDeclarationSyntax syntax, string className, string ns)
        {
            ClassSyntax = syntax;
            ClassName = className;
            Namespace = ns;
            HasEntityBaseClass = false;
            BaseClassTypes = new List<string>();
        }
    }

    public class ComponentClassData
    {
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public string ParentTypeName { get; set; }
        public string ParentTypeNamespace { get; set; }
        public List<CompositionComponentRef> ComponentRefs { get; set; }

        public ComponentClassData(INamedTypeSymbol classSymbol, TypeInfo parentType)
        {
            ClassName = classSymbol.Name;
            ParentTypeName = parentType.Type.Name;

            if (classSymbol.ContainingNamespace != null && !classSymbol.ContainingNamespace.IsGlobalNamespace)
                Namespace = classSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            if (parentType.Type.ContainingNamespace != null && !parentType.Type.ContainingNamespace.IsGlobalNamespace)
                ParentTypeNamespace = parentType.Type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));

            ComponentRefs = new List<CompositionComponentRef>();
        }
    }

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

            List<EntityClassData> entityClassList = new List<EntityClassData>();
            List<ComponentClassData> componentClassList = new List<ComponentClassData>();

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

                        var componentClassData = new ComponentClassData(classTypeSymbol, attributeParentClass);

                        foreach (var attribute in componentRefAttributeNodes)
                        {
                            var componentRefClassNodes = attribute
                                .DescendantTokens()
                                ?.Where(dt => dt.IsKind(SyntaxKind.IdentifierToken))
                                ?.ToList();
                            if (componentRefClassNodes != null && componentRefClassNodes.Count > 0)
                            {
                                var typeInfo = semanticModel.GetTypeInfo(componentRefClassNodes.Last().Parent);
                                string typeName = typeInfo.Type.Name;
                                string typeNamespace = typeInfo.Type.ContainingNamespace?.Name ?? null;
                                string varName = typeName.Substring(0, 1).ToLower() + typeName.Substring(1);

                                componentClassData.ComponentRefs.Add(new CompositionComponentRef(typeName, typeNamespace, varName));
                            }
                        }
                        componentClassList.Add(componentClassData);
                        
                    }
                    else if (entityClassNodes != null)
                    {
                        var entityClassData = new EntityClassData(declaredClass, declaredClass.Identifier.ToString(), GetNamespace(declaredClass, semanticModel));

                        foreach (var type in declaredClass.BaseList.Types)
                        {
                            var typeInfo = semanticModel.GetTypeInfo(type.Type);
                            entityClassData.BaseClassTypes.Add(typeInfo.Type?.Name ?? string.Empty);
                        }

                        entityClassList.Add(entityClassData);
                    }
                }
            }

            foreach (var classData in entityClassList)
            {
                // Determine if the base class for this Entity class is another Entity
                foreach (string baseType in classData.BaseClassTypes)
                {
                    if (entityClassList.Any(e => e.ClassName == baseType))
                    {
                        classData.HasEntityBaseClass = true;
                        break;
                    }
                }

                if (!classData.HasEntityBaseClass)
                {
                    var srcBuilder = new StringBuilder();
                    WriteEntityClass(ref srcBuilder, classData);
                    context.AddSource($"{classData.ClassName}_IEntity.g", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
                }
            }

            foreach (var classData in componentClassList)
            {
                var srcBuilder = new StringBuilder();
                WriteComponentClass(ref srcBuilder, classData);
                context.AddSource($"{classData.ClassName}_IComponent.g", SourceText.From(srcBuilder.ToString(), Encoding.UTF8));
            }
        }

        private string GetNamespace(ClassDeclarationSyntax classSyntax, SemanticModel semanticModel)
        {
            var classTypeSymbol = semanticModel.GetDeclaredSymbol(classSyntax);
            if (classTypeSymbol == null)
                return string.Empty;

            string ns = string.Empty;

            var namespaceSymbol = classTypeSymbol.ContainingNamespace;
            if (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
            {
                ns = namespaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
                if (ns == null)
                    ns = string.Empty;
            }

            return ns;
        }

        private void WriteComponentClass(ref StringBuilder srcBuilder, ComponentClassData classData)
        {
            string classNs = classData.Namespace;
            string attributeTypeNs = classData.ParentTypeNamespace;

            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");

            if (!string.IsNullOrEmpty(attributeTypeNs) && attributeTypeNs != "Godot")
                srcBuilder.AppendLine("using " + attributeTypeNs + ";");

            var componentNamespaces = classData.ComponentRefs.Select(x => x.TypeNamespace).Distinct();

            // ensure namespaces for component dependencies get added to the generated source
            foreach (var ns in componentNamespaces)
            {
                if (!string.IsNullOrEmpty(ns) && ns != classNs)
                    srcBuilder.AppendLine("using " +  ns + ";");
            }

            srcBuilder.AppendLine();

            if (!string.IsNullOrEmpty(classNs))
            {
                srcBuilder.AppendLine("namespace " + classNs + ";");
                srcBuilder.AppendLine();
            }

            srcBuilder.AppendLine($"public partial class {classData.ClassName} : Godot.Composition.IComponent");
            srcBuilder.AppendLine("{");
            srcBuilder.AppendLine($"    protected {classData.ParentTypeName} parent;");

            foreach (var compRefName in classData.ComponentRefs)
                srcBuilder.AppendLine("    protected " + compRefName.TypeName + " " + compRefName.VariableName + ";");

            srcBuilder.AppendLine();

            WriteInitializeComponentMethod(ref srcBuilder, classData.ParentTypeName, "    ");
            srcBuilder.AppendLine();
            WriteComponentResolveDependenciesMethod(ref srcBuilder, classData.ComponentRefs, "    ");

            srcBuilder.AppendLine("}");
        }

        private void WriteInitializeComponentMethod(ref StringBuilder srcBuilder, string parentTypeName, string indent)
        {
            srcBuilder.AppendLine(indent + "protected void InitializeComponent()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    parent = GetParent<" + parentTypeName + ">();");
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    if (parent is IEntity e)");
            srcBuilder.AppendLine(indent + "    {");
            srcBuilder.AppendLine(indent + "        if (e.IsEntityInitialized())");
            srcBuilder.AppendLine(indent + "            e.ResolveDependencies();");
            srcBuilder.AppendLine(indent + "    }");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteComponentResolveDependenciesMethod(ref StringBuilder srcBuilder, List<CompositionComponentRef> componentRefNames, string indent)
        {
            srcBuilder.AppendLine(indent + "public void ResolveDependencies()");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    if (parent != null && parent is IEntity ent)");
            srcBuilder.AppendLine(indent + "    {");

            foreach (var compRefName in componentRefNames)
            {
                srcBuilder.AppendLine(indent + "        " + compRefName.VariableName + " = ent.GetComponent<" + compRefName.TypeName + ">();");
            }
            
            srcBuilder.AppendLine(indent + "    }");
            srcBuilder.AppendLine(indent + "}");
        }

        private void WriteEntityClass(ref StringBuilder srcBuilder, EntityClassData entityClassData)
        {
            srcBuilder.AppendLine("using Godot;");
            srcBuilder.AppendLine("using Godot.Composition;");
            srcBuilder.AppendLine("using System;");
            srcBuilder.AppendLine("using System.Collections.Generic;");
            srcBuilder.AppendLine("using System.Linq;");
            srcBuilder.AppendLine();

            if (!string.IsNullOrEmpty(entityClassData.Namespace))
            {
                srcBuilder.AppendLine($"namespace {entityClassData.Namespace};");
                srcBuilder.AppendLine();
            }

            srcBuilder.AppendLine($"public partial class {entityClassData.ClassName} : Godot.Composition.IEntity");
            srcBuilder.AppendLine("{");

            srcBuilder.AppendLine("    protected System.Collections.Generic.List<System.Tuple<System.Type, Godot.StringName, Godot.Variant>> onReadySetList = new System.Collections.Generic.List<System.Tuple<System.Type, Godot.StringName, Godot.Variant>>();");
            srcBuilder.AppendLine("    protected Godot.Composition.ComponentContainer container = new Godot.Composition.ComponentContainer();");
            srcBuilder.AppendLine("    protected bool isEntityInitialized = false;");
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

            WriteOnReadySetMethod(ref srcBuilder, "    ");
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
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    foreach (var item in onReadySetList)");
            srcBuilder.AppendLine(indent + "    {");
            srcBuilder.AppendLine(indent + "        var component = container.GetComponentByType(item.Item1);");
            srcBuilder.AppendLine(indent + "        if (component != null)");
            srcBuilder.AppendLine(indent + "            component.Set(item.Item2, item.Item3);");
            srcBuilder.AppendLine(indent + "    }");
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    onReadySetList.Clear();");
            srcBuilder.AppendLine(indent + "    isEntityInitialized = true;");
            srcBuilder.AppendLine(indent + "    ");
            srcBuilder.AppendLine(indent + "    foreach (var component in container)");
            srcBuilder.AppendLine(indent + "    {");
            srcBuilder.AppendLine(indent + "        if (component is Godot.GodotObject gdObj && gdObj.HasMethod(\"OnEntityReady\"))");
            srcBuilder.AppendLine(indent + "            Connect(SignalName.Ready, new Callable(gdObj, \"OnEntityReady\"));");
            srcBuilder.AppendLine(indent + "    }");
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

        private void WriteOnReadySetMethod(ref StringBuilder srcBuilder, string indent)
        {
            srcBuilder.AppendLine(indent + "public void OnReadySet<T>(Godot.StringName propertyName, Godot.Variant val) where T : Godot.Composition.IComponent");
            srcBuilder.AppendLine(indent + "{");
            srcBuilder.AppendLine(indent + "    var type = typeof(T);");
            srcBuilder.AppendLine(indent + "    onReadySetList.Add(new System.Tuple<System.Type, Godot.StringName, Godot.Variant>(type, propertyName, val));");
            srcBuilder.AppendLine(indent + "}");
        }
    }
}
