using DifferentNamespace;
using Godot;
using Godot.Composition;
using System.ComponentModel;

namespace GeneratorTester
{
    [Component(typeof(CharacterBody2D))]
    [ComponentDependency(typeof(ComponentOne))]
    [ComponentDependency(typeof(ComponentTwo))]
    [ComponentDependency(typeof(ComponentThree))]
    public partial class TestComponent : Node
    {
        public override void _Ready()
        {
            base._Ready();
            InitializeComponent();
        }

        public void OnEntityReady()
        {
        }
    }
}
