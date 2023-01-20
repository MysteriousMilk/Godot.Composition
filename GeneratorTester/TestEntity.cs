using Godot;
using Godot.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorTester
{
    [Entity]
    public partial class TestEntity : CharacterBody2D
    {
        public override void _Ready()
        {
            base._Ready();
            InitializeEntity();
        }

        public void TestFunction()
        {
            var component = GetComponent<TestComponent>();
        }
    }
}
