using Godot;
using Godot.Composition;
using System.ComponentModel;

namespace GeneratorTester
{
    [Component(typeof(CharacterBody2D))]
    public partial class ComponentTwo : Node
    {
        public override void _Ready()
        {
            base._Ready();
            InitializeComponent();
        }
    }
}
