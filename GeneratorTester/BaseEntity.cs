using Godot;
using Godot.Composition;

namespace GeneratorTester
{
    public interface ITestInterface
    {

    }

    [Entity]
    public partial class BaseEntity : CharacterBody2D, ITestInterface
    {
        public override void _Ready()
        {
            base._Ready();
            InitializeEntity();
        }
    }
}
