using Godot;
using Godot.Composition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorTester
{
    [Component(typeof(CharacterBody2D))]
    public partial class TestComponent : Node
    {
        public override void _Ready()
        {
            base._Ready();
            InitializeComponent();
        }
    }
}
