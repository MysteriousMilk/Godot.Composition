# Godot.Composition
[![NuGet version (Godot.Composition)](https://img.shields.io/badge/nuget-v1.3.1-blue?style=flat-square)](https://www.nuget.org/packages/Godot.Composition/1.3.1)

This library provides a solution for building game entities in Godot through composition over inheritance. Godot's node-based architecture requires some level of inheritance. However, minimizing inheritance and refactoring much of your game's logic into components may go a long way towards cleaner, more reusable code.

There are several major approaches to managing entities and data in game development such as inheritance, component-based entity (CBE) model, and entity component systems (ECS). Each of theses models have their pros and cons. While an ECS model is essentially impossible to do in (vanilla) Godot, we can achieve pseudo-CBE/composition approach. So, if you are trying to do composition in Godot, this library will provide some tools and boiler-plate code to help.

## Entities in Godot.Composition
Godot.Composition requires indicating which objects are entities. In Godot.Composition, entities are simply Godot nodes that can have attached components. Entities are indicated through the use of a class attribute, and must be initialized in the **_Ready** method. The class attribute is paired with a source-code generator which generates a lot of the *boiler-plate* code. Because of this, the class must be marked *partial*, which will  likely be the case anyway since Godot C# uses source-code generators as well.

```C#
using Godot;
using Godot.Composition;

[Entity]
public partial class Player : CharacterBody2D
{
    public override void _Ready()
    {
        InitializeEntity();
    }
}
```
Depending on implementation and composition level desired, you may end up with several entity classes or just one. For example, you may want all kinematic entities your game to by one entity class where the all the logic is separated into components. Or, you may desired to use a little inheritance and make multiple entity classes, such as, *Player* and *NPC*, etc.

## Components in Godot.Composition
The ultimate goal of composition is building reusable components. So, if all your entities need to move around the world, it makes sense to abstract this into a *VelocityComponent*. Likewise, if all your entities have health and can be damaged, that code might be abstracted into a *HealthComponent* and a *HitboxComponent*.

Lets take a look at how to specify a component in Godot.Composition. It works similar to entities. Since this is Godot, the components must be nodes as well (this can help with things like node communication via signal, etc.) Mark components with a class attribute and specify the type of entity that the component can be associated with. The component must be initialized in the **_Ready** method.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
public partial class VelocityComponent : Node
{
    public override void _Ready()
    {
        InitializeComponent();
    }
}
```

Components do not have to just be the base Node type. They can be whichever node type makes the most sense for the component. Take a *HitboxComponent* for example. This would be best implemented as an *Area* or *Area2D*.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
public partial class HitboxComponent : Area2D
{
    public override void _Ready()
    {
        InitializeComponent();
    }
}
```

All components will have a protected reference to the entity that they belong to. This can be useful for encapsulating the logic with a component. For example, below is a partial implementation of a *VelocityComponent*.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
public partial class VelocityComponent : Node
{
    [Export]
    public float Acceleration { get; set; }

    public Vector2 Velocity { get; private set; }

    public void AccelerateToVelocity(Vector2 vel)
    {
        Velocity = vel + Acceleration;
    }

    public override void _Ready()
    {
        InitializeComponent();
    }

    public void Move()
    {
        // The parent reference here will be a reference to the CharacterBody2D entity.
        parent.Velocity = Velocity;
        parent.MoveAndSlide();
        Velocity = parent.Velocity;
    }
}
```
All component nodes for an entity should be placed as a direct child of the entity node with the scene in Godot. An example of a Godot scene tree with components can be scene below.

![component-tree](https://github.com/MysteriousMilk/Godot.Composition/assets/6441268/1c0f10e0-9fe3-4439-b385-412ed979a45f)

## Accessing Components from an Entity
Even with most logic abstracted into components, there will likely be some glue code required to bring everything together. If access to a specific component is needed in the Entity script, components can be retrieved as seen below.

```C#
using Godot;
using Godot.Composition;

[Entity]
public partial class Player : CharacterBody2D
{
    ...

    public override void _PhysicsProcess(double delta)
    {
        var velocityComponent = GetComponent<VelocityComponent>();
        velocityComponent.AccelerateToVelocity(direction * maxSpeed);
        velocityComponent.Move();
    }
}
```

Components can also be retrieved using their interface type.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
public partial class NetworkClientComponent : Node, INetworkComponent
{
    public long NetworkId { get; set; }

    public override void _Ready()
    {
        InitializeComponent();
    }
}

[Entity]
public partial class Player : CharacterBody2D
{
    public override void _Ready()
    {
        InitializeEntity();

        var networkComponent = GetComponent<INetworkComponent>();
        networkComponent.NetworkId = Multiplayer.GetUniqueId();
    }
}
```

## Adding Component Dependencies
Adding references to other components is easy with the *ComponentDependency* attribute. Adding a *ComponentDependency* attribute to a component will automatically insert a protected reference to the dependent component. See the example below.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
[ComponentDependency(typeof(HealthComponent))]
public partial class HitboxComponent : Area2D
{
    ...

    public void OnAreaEntered(Area2D area)
    {
        // Area entered, calculated damage
        double damage;

        healthComponent.ApplyDamage(damage);
    }
}
```

## Entity Ready Callback
Sometimes, its necessary to run code on a component immediately after an Entity node is ready. Godot.Composition allows the developer to setup a special function that will be called immediately after the parent Entity's *_Ready* function is called.

Simply define a function called *void OnEntityReady()* on your component and it will be called automatically.

```C#
using Godot;
using Godot.Composition;

[Component(typeof(CharacterBody2D))]
public partial class HealthComponent : Node
{
    ...

    /// Called directly after the parent entity's
    /// _Ready function.
    public void OnEntityReady()
    {
        ...
    }
}
```

## Setting Component Values Programmatically
Occationally, Entity nodes will be created or spawned using a C#. When instantiating Entities in this manner, the Entity's Components do not become available until the Entity's *_Ready* function is called when added to the Scene Tree. Instead of deferring a frame (or two) to set values on a component, you can queue up values to set during the *_Ready* function. Consider the following scenario. We are spawning fast moving projectile. We desire to associate some data with a *DamageComponent* on the projectile, but we don't want to wait several frames to set the value since the projectile's lifetime will be so short. Here we can use the OnReadySet method. See below.

```C#
using Godot;
using Godot.Composition;

[Entity]
public partial class Gun : Node2D
{
    ...

    public void Fire()
    {
        // instantiate projectile
        var projectileScn = ResourceLoader.Load<PackedScene>("res://Projectile.tscn");
        var projectile = projectileScn.Instantiate<Projectile>();

        projectile.OnReadySet<DamageComponent>(DamageComponent.PropertyName.Damage, 20.0);
    }
}
```
