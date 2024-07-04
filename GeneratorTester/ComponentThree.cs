using Godot.Composition;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DifferentNamespace;

[Component(typeof(CharacterBody2D))]
public partial class ComponentThree : Node
{
    public override void _Ready()
    {
        base._Ready();
        InitializeComponent();
    }
}
