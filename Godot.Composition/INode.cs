using Godot.Collections;

namespace Godot.Composition;

public interface INode
{
    //
    // Summary:
    //     The name of the node. This name is unique among the siblings (other child nodes
    //     from the same parent). When set to an existing name, the node will be automatically
    //     renamed.
    //     Note: Auto-generated names might include the @ character, which is reserved for
    //     unique names when using Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode).
    //     When setting the name manually, any @ will be removed.
    StringName Name { get; set; }

    //
    // Summary:
    //     The node owner. A node can have any other node as owner (as long as it is a valid
    //     parent, grandparent, etc. ascending in the tree). When saving a node (using Godot.PackedScene),
    //     all the nodes it owns will be saved with it. This allows for the creation of
    //     complex Godot.SceneTrees, with instancing and subinstancing.
    //     Note: If you want a child to be persisted to a Godot.PackedScene, you must set
    //     Godot.Node.Owner in addition to calling Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode).
    //     This is typically relevant for tool scripts and editor plugins. If Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)
    //     is called without setting Godot.Node.Owner, the newly added Godot.Node will not
    //     be visible in the scene tree, though it will be visible in the 2D/3D view.
    Node Owner { get; set; }

    //
    // Summary:
    //     Returns the object's built-in class name, as a System.String. See also Godot.Object.IsClass(System.String).
    //     Note: This method ignores class_name declarations. If this object's script has
    //     defined a class_name, the base, built-in class name is returned instead.
    string GetClass();

    //
    // Summary:
    //     Returns true if the object inherits from the given [param class]. See also Godot.Object.GetClass.
    //     [codeblocks]
    //     [gdscript]
    //     var sprite2d = Sprite2D.new()
    //     sprite2d.is_class("Sprite2D") # Returns true
    //     sprite2d.is_class("Node") # Returns true
    //     sprite2d.is_class("Node3D") # Returns false
    //     [/gdscript]
    //     [csharp]
    //     var sprite2d = new Sprite2D();
    //     sprite2d.IsClass("Sprite2D"); // Returns true
    //     sprite2d.IsClass("Node"); // Returns true
    //     sprite2d.IsClass("Node3D"); // Returns false
    //     [/csharp]
    //     [/codeblocks]
    //     Note: This method ignores class_name declarations in the object's script.
    bool IsClass(string @class);

    //
    // Summary:
    //     Assigns [param value] to the given [param property]. If the property does not
    //     exist or the given [param value]'s type doesn't match, nothing happens.
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node2D.new()
    //     node.set("global_scale", Vector2(8, 2.5))
    //     print(node.global_scale) # Prints (8, 2.5)
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node2D();
    //     node.Set("global_scale", new Vector2(8, 2.5));
    //     GD.Print(node.GlobalScale); // Prints Vector2(8, 2.5)
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param property] must be in snake_case when referring to built-in
    //     Godot properties. Prefer using the names exposed in the PropertyName class to
    //     avoid allocating a new Godot.StringName on each call.
    void Set(StringName property, Variant value);

    //
    // Summary:
    //     Returns the Variant value of the given [param property]. If the [param property]
    //     does not exist, this method returns null.
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node2D.new()
    //     node.rotation = 1.5
    //     var a = node.get("rotation") # a is 1.5
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node2D();
    //     node.Rotation = 1.5f;
    //     var a = node.Get("rotation"); // a is 1.5
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param property] must be in snake_case when referring to built-in
    //     Godot properties. Prefer using the names exposed in the PropertyName class to
    //     avoid allocating a new Godot.StringName on each call.
    Variant Get(StringName property);

    //
    // Summary:
    //     Emits the given [param signal] by name. The signal must exist, so it should be
    //     a built-in signal of this class or one of its inherited classes, or a user-defined
    //     signal (see Godot.Object.AddUserSignal(System.String,Godot.Collections.Array)).
    //     This method supports a variable number of arguments, so parameters can be passed
    //     as a comma separated list.
    //     Returns Godot.Error.Unavailable if [param signal] does not exist or the parameters
    //     are invalid.
    //     [codeblocks]
    //     [gdscript]
    //     emit_signal("hit", "sword", 100)
    //     emit_signal("game_over")
    //     [/gdscript]
    //     [csharp]
    //     EmitSignal("Hit", "sword", 100);
    //     EmitSignal("GameOver");
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param signal] must be in snake_case when referring to built-in
    //     Godot signals. Prefer using the names exposed in the SignalName class to avoid
    //     allocating a new Godot.StringName on each call.
    Error EmitSignal(StringName signal, params Variant[] args);

    //
    // Summary:
    //     Calls the [param method] on the object and returns the result. This method supports
    //     a variable number of arguments, so parameters can be passed as a comma separated
    //     list.
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node3D.new()
    //     node.call("rotate", Vector3(1.0, 0.0, 0.0), 1.571)
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node3D();
    //     node.Call("rotate", new Vector3(1f, 0f, 0f), 1.571f);
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param method] must be in snake_case when referring to built-in
    //     Godot methods. Prefer using the names exposed in the MethodName class to avoid
    //     allocating a new Godot.StringName on each call.
    Variant Call(StringName method, params Variant[] args);

    //
    // Summary:
    //     Calls the [param method] on the object during idle time. This method supports
    //     a variable number of arguments, so parameters can be passed as a comma separated
    //     list.
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node3D.new()
    //     node.call_deferred("rotate", Vector3(1.0, 0.0, 0.0), 1.571)
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node3D();
    //     node.CallDeferred("rotate", new Vector3(1f, 0f, 0f), 1.571f);
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param method] must be in snake_case when referring to built-in
    //     Godot methods. Prefer using the names exposed in the MethodName class to avoid
    //     allocating a new Godot.StringName on each call.
    Variant CallDeferred(StringName method, params Variant[] args);

    //
    // Summary:
    //     Assigns [param value] to the given [param property], after the current frame's
    //     physics step. This is equivalent to calling Godot.Object.Set(Godot.StringName,Godot.Variant)
    //     through Godot.Object.CallDeferred(Godot.StringName,Godot.Variant[]).
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node2D.new()
    //     add_child(node)
    //     node.rotation = 45.0
    //     node.set_deferred("rotation", 90.0)
    //     print(node.rotation) # Prints 45.0
    //     await get_tree().process_frame
    //     print(node.rotation) # Prints 90.0
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node2D();
    //     node.Rotation = 45f;
    //     node.SetDeferred("rotation", 90f);
    //     GD.Print(node.Rotation); // Prints 45.0
    //     await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    //     GD.Print(node.Rotation); // Prints 90.0
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param property] must be in snake_case when referring to built-in
    //     Godot properties. Prefer using the names exposed in the PropertyName class to
    //     avoid allocating a new Godot.StringName on each call.
    void SetDeferred(StringName property, Variant value);

    //
    // Summary:
    //     Calls the [param method] on the object and returns the result. Unlike Godot.Object.Call(Godot.StringName,Godot.Variant[]),
    //     this method expects all parameters to be contained inside [param arg_array].
    //     [codeblocks]
    //     [gdscript]
    //     var node = Node3D.new()
    //     node.callv("rotate", [Vector3(1.0, 0.0, 0.0), 1.571])
    //     [/gdscript]
    //     [csharp]
    //     var node = new Node3D();
    //     node.Callv("rotate", new Godot.Collections.Array { new Vector3(1f, 0f, 0f), 1.571f
    //     });
    //     [/csharp]
    //     [/codeblocks]
    //     Note: In C#, [param method] must be in snake_case when referring to built-in
    //     Godot methods. Prefer using the names exposed in the MethodName class to avoid
    //     allocating a new Godot.StringName on each call
    Variant Callv(StringName method, Godot.Collections.Array argArray);

    //
    // Summary:
    //     Connects a [param signal] by name to a [param callable]. Optional [param flags]
    //     can be also added to configure the connection's behavior (see Godot.Object.ConnectFlags
    //     constants).
    //     A signal can only be connected once to the same Godot.Callable. If the signal
    //     is already connected, this method returns Godot.Error.InvalidParameter and pushes
    //     an error message, unless the signal is connected with Godot.Object.ConnectFlags.ReferenceCounted.
    //     To prevent this, use Godot.Object.IsConnected(Godot.StringName,Godot.Callable)
    //     first to check for existing connections.
    //     If the [param callable]'s object is freed, the connection will be lost.
    //     Examples with recommended syntax:
    //     Connecting signals is one of the most common operations in Godot and the API
    //     gives many options to do so, which are described further down. The code block
    //     below shows the recommended approach.
    //     [codeblocks]
    //     [gdscript]
    //     func _ready():
    //     var button = Button.new()
    //     # `button_down` here is a Signal variant type, and we thus call the Signal.connect()
    //     method, not Object.connect().
    //     # See discussion below for a more in-depth overview of the API.
    //     button.button_down.connect(_on_button_down)
    //     # This assumes that a `Player` class exists, which defines a `hit` signal.
    //     var player = Player.new()
    //     # We use Signal.connect() again, and we also use the Callable.bind() method,
    //     # which returns a new Callable with the parameter binds.
    //     player.hit.connect(_on_player_hit.bind("sword", 100))
    //     func _on_button_down():
    //     print("Button down!")
    //     func _on_player_hit(weapon_type, damage):
    //     print("Hit with weapon %s for %d damage." % [weapon_type, damage])
    //     [/gdscript]
    //     [csharp]
    //     public override void _Ready()
    //     {
    //     var button = new Button();
    //     // C# supports passing signals as events, so we can use this idiomatic construct:
    //     button.ButtonDown += OnButtonDown;
    //     // This assumes that a `Player` class exists, which defines a `Hit` signal.
    //     var player = new Player();
    //     // Signals as events (`player.Hit += OnPlayerHit;`) do not support argument binding.
    //     You have to use:
    //     player.Hit.Connect(OnPlayerHit, new Godot.Collections.Array {"sword", 100 });
    //     }
    //     private void OnButtonDown()
    //     {
    //     GD.Print("Button down!");
    //     }
    //     private void OnPlayerHit(string weaponType, int damage)
    //     {
    //     GD.Print(String.Format("Hit with weapon {0} for {1} damage.", weaponType, damage));
    //     }
    //     [/csharp]
    //     [/codeblocks]
    //     Object.connect() or Signal.connect()?
    //     As seen above, the recommended method to connect signals is not Godot.Object.Connect(Godot.StringName,Godot.Callable,System.UInt32).
    //     The code block below shows the four options for connecting signals, using either
    //     this legacy method or the recommended Signal.connect, and using either an implicit
    //     Godot.Callable or a manually defined one.
    //     [codeblocks]
    //     [gdscript]
    //     func _ready():
    //     var button = Button.new()
    //     # Option 1: Object.connect() with an implicit Callable for the defined function.
    //     button.connect("button_down", _on_button_down)
    //     # Option 2: Object.connect() with a constructed Callable using a target object
    //     and method name.
    //     button.connect("button_down", Callable(self, "_on_button_down"))
    //     # Option 3: Signal.connect() with an implicit Callable for the defined function.
    //     button.button_down.connect(_on_button_down)
    //     # Option 4: Signal.connect() with a constructed Callable using a target object
    //     and method name.
    //     button.button_down.connect(Callable(self, "_on_button_down"))
    //     func _on_button_down():
    //     print("Button down!")
    //     [/gdscript]
    //     [csharp]
    //     public override void _Ready()
    //     {
    //     var button = new Button();
    //     // Option 1: Object.Connect() with an implicit Callable for the defined function.
    //     button.Connect("button_down", OnButtonDown);
    //     // Option 2: Object.connect() with a constructed Callable using a target object
    //     and method name.
    //     button.Connect("button_down", new Callable(self, nameof(OnButtonDown)));
    //     // Option 3: Signal.connect() with an implicit Callable for the defined function.
    //     button.ButtonDown.Connect(OnButtonDown);
    //     // Option 3b: In C#, we can use signals as events and connect with this more
    //     idiomatic syntax:
    //     button.ButtonDown += OnButtonDown;
    //     // Option 4: Signal.connect() with a constructed Callable using a target object
    //     and method name.
    //     button.ButtonDown.Connect(new Callable(self, nameof(OnButtonDown)));
    //     }
    //     private void OnButtonDown()
    //     {
    //     GD.Print("Button down!");
    //     }
    //     [/csharp]
    //     [/codeblocks]
    //     While all options have the same outcome (button's Godot.BaseButton.ButtonDown
    //     signal will be connected to _on_button_down), option 3 offers the best validation:
    //     it will print a compile-time error if either the button_down Godot.SignalInfo
    //     or the _on_button_down Godot.Callable are not defined. On the other hand, option
    //     2 only relies on string names and will only be able to validate either names
    //     at runtime: it will print a runtime error if "button_down" doesn't correspond
    //     to a signal, or if "_on_button_down" is not a registered method in the object
    //     self. The main reason for using options 1, 2, or 4 would be if you actually need
    //     to use strings (e.g. to connect signals programmatically based on strings read
    //     from a configuration file). Otherwise, option 3 is the recommended (and fastest)
    //     method.
    //     Binding and passing parameters:
    //     The syntax to bind parameters is through Callable.bind, which returns a copy
    //     of the Godot.Callable with its parameters bound.
    //     When calling Godot.Object.EmitSignal(Godot.StringName,Godot.Variant[]), the signal
    //     parameters can be also passed. The examples below show the relationship between
    //     these signal parameters and bound parameters.
    //     [codeblocks]
    //     [gdscript]
    //     func _ready():
    //     # This assumes that a `Player` class exists, which defines a `hit` signal.
    //     var player = Player.new()
    //     player.hit.connect(_on_player_hit.bind("sword", 100))
    //     # Parameters added when emitting the signal are passed first.
    //     player.emit_signal("hit", "Dark lord", 5)
    //     # We pass two arguments when emitting (`hit_by`, `level`),
    //     # and bind two more arguments when connecting (`weapon_type`, `damage`).
    //     func _on_player_hit(hit_by, level, weapon_type, damage):
    //     print("Hit by %s (level %d) with weapon %s for %d damage." % [hit_by, level,
    //     weapon_type, damage])
    //     [/gdscript]
    //     [csharp]
    //     public override void _Ready()
    //     {
    //     // This assumes that a `Player` class exists, which defines a `Hit` signal.
    //     var player = new Player();
    //     // Option 1: Using Callable.Bind(). This way we can still use signals as events.
    //     player.Hit += OnPlayerHit.Bind("sword", 100);
    //     // Option 2: Using a `binds` Array in Signal.Connect().
    //     player.Hit.Connect(OnPlayerHit, new Godot.Collections.Array{ "sword", 100 });
    //     // Parameters added when emitting the signal are passed first.
    //     player.EmitSignal("hit", "Dark lord", 5);
    //     }
    //     // We pass two arguments when emitting (`hit_by`, `level`),
    //     // and bind two more arguments when connecting (`weapon_type`, `damage`).
    //     private void OnPlayerHit(string hitBy, int level, string weaponType, int damage)
    //     {
    //     GD.Print(String.Format("Hit by {0} (level {1}) with weapon {2} for {3} damage.",
    //     hitBy, level, weaponType, damage));
    //     }
    //     [/csharp]
    //     [/codeblocks]
    Error Connect(StringName signal, Callable callable, uint flags = 0u);

    //
    // Summary:
    //     Disconnects a [param signal] by name from a given [param callable]. If the connection
    //     does not exist, generates an error. Use Godot.Object.IsConnected(Godot.StringName,Godot.Callable)
    //     to make sure that the connection exists.
    void Disconnect(StringName signal, Callable callable);

    //
    // Summary:
    //     Returns true if a connection exists between the given [param signal] name and
    //     [param callable].
    //     Note: In C#, [param signal] must be in snake_case when referring to built-in
    //     Godot methods. Prefer using the names exposed in the SignalName class to avoid
    //     allocating a new Godot.StringName on each call.
    bool IsConnected(StringName signal, Callable callable);

    //
    // Summary:
    //     Returns true if the Godot.Node.QueueFree method was called for the object.
    bool IsQueuedForDeletion();

    //
    // Summary:
    //     Fetches a node. The Godot.NodePath can be either a relative path (from the current
    //     node) or an absolute path (in the scene tree) to a node. If the path does not
    //     exist, a null instance is returned and an error is logged. Attempts to access
    //     methods on the return value will result in an "Attempt to call <method> on a
    //     null instance." error. Note: Fetching absolute paths only works when the node
    //     is inside the scene tree (see Godot.Node.IsInsideTree).
    //
    // Parameters:
    //   path:
    //     The path to the node to fetch.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The Godot.Node at the given path.
    //
    // Exceptions:
    //   T:System.InvalidCastException:
    //     The fetched node can't be casted to the given type T.
    T GetNode<T>(NodePath path) where T : class;

    //
    // Summary:
    //     Fetches a node. The Godot.NodePath can be either a relative path (from the current
    //     node) or an absolute path (in the scene tree) to a node. If the path does not
    //     exist, a null instance is returned and an error is logged. Attempts to access
    //     methods on the return value will result in an "Attempt to call <method> on a
    //     null instance." error. Note: Fetching absolute paths only works when the node
    //     is inside the scene tree (see Godot.Node.IsInsideTree).
    //
    // Parameters:
    //   path:
    //     The path to the node to fetch.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The Godot.Node at the given path, or null if not found.
    T GetNodeOrNull<T>(NodePath path) where T : class;

    //
    // Summary:
    //     Returns a child node by its index (see Godot.Node.GetChildCount(System.Boolean)).
    //     This method is often used for iterating all children of a node. Negative indices
    //     access the children from the last one. To access a child node via its name, use
    //     Godot.Node.GetNode(Godot.NodePath).
    //
    // Parameters:
    //   idx:
    //     Child index.
    //
    //   includeInternal:
    //     If false, internal children are skipped (see internal parameter in Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)).
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The child Godot.Node at the given index idx.
    //
    // Exceptions:
    //   T:System.InvalidCastException:
    //     The fetched node can't be casted to the given type T.
    T GetChild<T>(int idx, bool includeInternal = false) where T : class;

    //
    // Summary:
    //     Returns a child node by its index (see Godot.Node.GetChildCount(System.Boolean)).
    //     This method is often used for iterating all children of a node. Negative indices
    //     access the children from the last one. To access a child node via its name, use
    //     Godot.Node.GetNode(Godot.NodePath).
    //
    // Parameters:
    //   idx:
    //     Child index.
    //
    //   includeInternal:
    //     If false, internal children are skipped (see internal parameter in Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)).
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The child Godot.Node at the given index idx, or null if not found.
    T GetChildOrNull<T>(int idx, bool includeInternal = false) where T : class;

    //
    // Summary:
    //     The node owner. A node can have any other node as owner (as long as it is a valid
    //     parent, grandparent, etc. ascending in the tree). When saving a node (using Godot.PackedScene),
    //     all the nodes it owns will be saved with it. This allows for the creation of
    //     complex Godot.SceneTrees, with instancing and subinstancing.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The owner Godot.Node.
    //
    // Exceptions:
    //   T:System.InvalidCastException:
    //     The fetched node can't be casted to the given type T.
    T GetOwner<T>() where T : class;

    //
    // Summary:
    //     The node owner. A node can have any other node as owner (as long as it is a valid
    //     parent, grandparent, etc. ascending in the tree). When saving a node (using Godot.PackedScene),
    //     all the nodes it owns will be saved with it. This allows for the creation of
    //     complex Godot.SceneTrees, with instancing and subinstancing.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The owner Godot.Node, or null if there is no owner.
    T GetOwnerOrNull<T>() where T : class;

    //
    // Summary:
    //     Returns the parent node of the current node, or a null instance if the node lacks
    //     a parent.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The parent Godot.Node.
    //
    // Exceptions:
    //   T:System.InvalidCastException:
    //     The fetched node can't be casted to the given type T.
    T GetParent<T>() where T : class;

    //
    // Summary:
    //     Returns the parent node of the current node, or a null instance if the node lacks
    //     a parent.
    //
    // Type parameters:
    //   T:
    //     The type to cast to. Should be a descendant of Godot.Node.
    //
    // Returns:
    //     The parent Godot.Node, or null if the node has no parent.
    T GetParentOrNull<T>() where T : class;

    //
    // Summary:
    //     Adds a child [param node]. Nodes can have any number of children, but every child
    //     must have a unique name. Child nodes are automatically deleted when the parent
    //     node is deleted, so an entire scene can be removed by deleting its topmost node.
    //     If [param force_readable_name] is true, improves the readability of the added
    //     [param node]. If not named, the [param node] is renamed to its type, and if it
    //     shares Godot.Node.Name with a sibling, a number is suffixed more appropriately.
    //     This operation is very slow. As such, it is recommended leaving this to false,
    //     which assigns a dummy name featuring @ in both situations.
    //     If [param internal] is different than Godot.Node.InternalMode.Disabled, the child
    //     will be added as internal node. Such nodes are ignored by methods like Godot.Node.GetChildren(System.Boolean),
    //     unless their parameter include_internal is true.The intended usage is to hide
    //     the internal nodes from the user, so the user won't accidentally delete or modify
    //     them. Used by some GUI nodes, e.g. Godot.ColorPicker. See Godot.Node.InternalMode
    //     for available modes.
    //     Note: If the child node already has a parent, the function will fail. Use Godot.Node.RemoveChild(Godot.Node)
    //     first to remove the node from its current parent. For example:
    //     [codeblocks]
    //     [gdscript]
    //     var child_node = get_child(0)
    //     if child_node.get_parent():
    //     child_node.get_parent().remove_child(child_node)
    //     add_child(child_node)
    //     [/gdscript]
    //     [csharp]
    //     Node childNode = GetChild(0);
    //     if (childNode.GetParent() != null)
    //     {
    //     childNode.GetParent().RemoveChild(childNode);
    //     }
    //     AddChild(childNode);
    //     [/csharp]
    //     [/codeblocks]
    //     If you need the child node to be added below a specific node in the list of children,
    //     use Godot.Node.AddSibling(Godot.Node,System.Boolean) instead of this method.
    //     Note: If you want a child to be persisted to a Godot.PackedScene, you must set
    //     Godot.Node.Owner in addition to calling Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode).
    //     This is typically relevant for tool scripts and editor plugins. If Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)
    //     is called without setting Godot.Node.Owner, the newly added Godot.Node will not
    //     be visible in the scene tree, though it will be visible in the 2D/3D view.
    public void AddChild(Node node, bool forceReadableName = false, Node.InternalMode @internal = Node.InternalMode.Disabled);

    /// <summary>
    /// Removes a child node. The node is NOT deleted and must be deleted manually.
    /// Note: This function may set the Godot.Node.Owner of the removed Node (or its
    /// descendants) to be null, if that Godot.Node.Owner is no longer a parent or ancestor.
    /// </summary>
    /// <param name="node">The <see cref="Node"/> to remove.</param>
    public void RemoveChild(Node node);

    /// <summary>
    /// Returns the number of child nodes.
    /// </summary>
    /// <param name="includeInternal">If false, internal children aren't counted (see AddChild).</param>
    /// <returns>The number of children for this <see cref="Node"/>.</returns>
    int GetChildCount(bool includeInternal = false);

    //
    // Summary:
    //     Returns an array of references to node's children.
    //     If [param include_internal] is false, the returned array won't include internal
    //     children (see internal parameter in Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)).
    Array<Node> GetChildren(bool includeInternal = false);

    //
    // Summary:
    //     Returns a child node by its index (see Godot.Node.GetChildCount(System.Boolean)).
    //     This method is often used for iterating all children of a node.
    //     Negative indices access the children from the last one.
    //     If [param include_internal] is false, internal children are skipped (see internal
    //     parameter in Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)).
    //     To access a child node via its name, use Godot.Node.GetNode(Godot.NodePath).
    Node GetChild(int idx, bool includeInternal = false);

    //
    // Summary:
    //     Returns true if the node that the Godot.NodePath points to exists.
    bool HasNode(NodePath path);

    //
    // Summary:
    //     Fetches a node. The Godot.NodePath can be either a relative path (from the current
    //     node) or an absolute path (in the scene tree) to a node. If the path does not
    //     exist, null is returned and an error is logged. Attempts to access methods on
    //     the return value will result in an "Attempt to call <method> on a null instance."
    //     error.
    //     Note: Fetching absolute paths only works when the node is inside the scene tree
    //     (see Godot.Node.IsInsideTree).
    //     Example: Assume your current node is Character and the following tree:
    //     /root
    //     /root/Character
    //     /root/Character/Sword
    //     /root/Character/Backpack/Dagger
    //     /root/MyGame
    //     /root/Swamp/Alligator
    //     /root/Swamp/Mosquito
    //     /root/Swamp/Goblin
    //     Possible paths are:
    //     [codeblocks]
    //     [gdscript]
    //     get_node("Sword")
    //     get_node("Backpack/Dagger")
    //     get_node("../Swamp/Alligator")
    //     get_node("/root/MyGame")
    //     [/gdscript]
    //     [csharp]
    //     GetNode("Sword");
    //     GetNode("Backpack/Dagger");
    //     GetNode("../Swamp/Alligator");
    //     GetNode("/root/MyGame");
    //     [/csharp]
    //     [/codeblocks]
    Node GetNode(NodePath path);

    //
    // Summary:
    //     Similar to Godot.Node.GetNode(Godot.NodePath), but does not log an error if [param
    //     path] does not point to a valid Godot.Node.
    Node GetNodeOrNull(NodePath path);

    //
    // Summary:
    //     Returns the parent node of the current node, or null if the node lacks a parent.
    Node GetParent();

    //
    // Summary:
    //     Finds the first descendant of this node whose name matches [param pattern] as
    //     in String.match.
    //     [param pattern] does not match against the full path, just against individual
    //     node names. It is case-sensitive, with "*" matching zero or more characters and
    //     "?" matching any single character except ".").
    //     If [param recursive] is true, all child nodes are included, even if deeply nested.
    //     Nodes are checked in tree order, so this node's first direct child is checked
    //     first, then its own direct children, etc., before moving to the second direct
    //     child, and so on. If [param recursive] is false, only this node's direct children
    //     are matched.
    //     If [param owned] is true, this method only finds nodes who have an assigned Godot.Node.Owner.
    //     This is especially important for scenes instantiated through a script, because
    //     those scenes don't have an owner.
    //     Returns null if no matching Godot.Node is found.
    //     Note: As this method walks through all the descendants of the node, it is the
    //     slowest way to get a reference to another node. Whenever possible, consider using
    //     Godot.Node.GetNode(Godot.NodePath) with unique names instead (see Godot.Node.UniqueNameInOwner),
    //     or caching the node references into variable.
    //     Note: To find all descendant nodes matching a pattern or a class type, see Godot.Node.FindChildren(System.String,System.String,System.Boolean,System.Boolean).
    Node FindChild(string pattern, bool recursive = true, bool owned = true);

    //
    // Summary:
    //     Finds descendants of this node whose name matches [param pattern] as in String.match,
    //     and/or type matches [param type] as in Godot.Object.IsClass(System.String).
    //     [param pattern] does not match against the full path, just against individual
    //     node names. It is case-sensitive, with "*" matching zero or more characters and
    //     "?" matching any single character except ".").
    //     [param type] will check equality or inheritance, and is case-sensitive. "Object"
    //     will match a node whose type is "Node" but not the other way around.
    //     If [param recursive] is true, all child nodes are included, even if deeply nested.
    //     Nodes are checked in tree order, so this node's first direct child is checked
    //     first, then its own direct children, etc., before moving to the second direct
    //     child, and so on. If [param recursive] is false, only this node's direct children
    //     are matched.
    //     If [param owned] is true, this method only finds nodes who have an assigned Godot.Node.Owner.
    //     This is especially important for scenes instantiated through a script, because
    //     those scenes don't have an owner.
    //     Returns an empty array if no matching nodes are found.
    //     Note: As this method walks through all the descendants of the node, it is the
    //     slowest way to get references to other nodes. Whenever possible, consider caching
    //     the node references into variables.
    //     Note: If you only want to find the first descendant node that matches a pattern,
    //     see Godot.Node.FindChild(System.String,System.Boolean,System.Boolean).
    Array<Node> FindChildren(string pattern, string type = "", bool recursive = true, bool owned = true);

    //
    // Summary:
    //     Finds the first parent of the current node whose name matches [param pattern]
    //     as in String.match.
    //     [param pattern] does not match against the full path, just against individual
    //     node names. It is case-sensitive, with "*" matching zero or more characters and
    //     "?" matching any single character except ".").
    //     Note: As this method walks upwards in the scene tree, it can be slow in large,
    //     deeply nested scene trees. Whenever possible, consider using Godot.Node.GetNode(Godot.NodePath)
    //     with unique names instead (see Godot.Node.UniqueNameInOwner), or caching the
    //     node references into variable.
    Node FindParent(string pattern);

    //
    // Summary:
    //     Returns true if this node is currently inside a Godot.SceneTree.
    bool IsInsideTree();

    //
    // Summary:
    //     Returns true if the given node is a direct or indirect child of the current node.
    bool IsAncestorOf(Node node);

    //
    // Summary:
    //     Returns true if the given node occurs later in the scene hierarchy than the current
    //     node.
    bool IsGreaterThan(Node node);

    //
    // Summary:
    //     Returns the absolute path of the current node. This only works if the current
    //     node is inside the scene tree (see Godot.Node.IsInsideTree).
    NodePath GetPath();

    //
    // Summary:
    //     Returns the relative Godot.NodePath from this node to the specified [param node].
    //     Both nodes must be in the same scene or the function will fail.
    //     If [param use_unique_path] is true, returns the shortest path considering unique
    //     node.
    //     Note: If you get a relative path which starts from a unique node, the path may
    //     be longer than a normal relative path due to the addition of the unique node's
    //     name.
    NodePath GetPathTo(Node node, bool useUniquePath = false);

    //
    // Summary:
    //     Adds the node to a group. Groups are helpers to name and organize a subset of
    //     nodes, for example "enemies" or "collectables". A node can be in any number of
    //     groups. Nodes can be assigned a group at any time, but will not be added until
    //     they are inside the scene tree (see Godot.Node.IsInsideTree). See notes in the
    //     description, and the group methods in Godot.SceneTree.
    //     The [param persistent] option is used when packing node to Godot.PackedScene
    //     and saving to file. Non-persistent groups aren't stored.
    //     Note: For performance reasons, the order of node groups is not guaranteed. The
    //     order of node groups should not be relied upon as it can vary across project
    //     runs.
    void AddToGroup(StringName group, bool persistent = false);

    //
    // Summary:
    //     Removes a node from the [param group]. Does nothing if the node is not in the
    //     [param group]. See notes in the description, and the group methods in Godot.SceneTree.
    void RemoveFromGroup(StringName group);

    //
    // Summary:
    //     Returns true if this node is in the specified group. See notes in the description,
    //     and the group methods in Godot.SceneTree.
    bool IsInGroup(StringName group);

    //
    // Summary:
    //     Moves a child node to a different index (order) among the other children. Since
    //     calls, signals, etc. are performed by tree order, changing the order of children
    //     nodes may be useful. If [param to_index] is negative, the index will be counted
    //     from the end.
    //     Note: Internal children can only be moved within their expected "internal range"
    //     (see internal parameter in Godot.Node.AddChild(Godot.Node,System.Boolean,Godot.Node.InternalMode)).
    void MoveChild(Node childNode, int toIndex);

    //
    // Summary:
    //     Returns an array listing the groups that the node is a member of.
    //     Note: For performance reasons, the order of node groups is not guaranteed. The
    //     order of node groups should not be relied upon as it can vary across project
    //     runs.
    //     Note: The engine uses some group names internally (all starting with an underscore).
    //     To avoid conflicts with internal groups, do not add custom groups whose name
    //     starts with an underscore. To exclude internal groups while looping over Godot.Node.GetGroups,
    //     use the following snippet:
    //     [codeblocks]
    //     [gdscript]
    //     # Stores the node's non-internal groups only (as an array of Strings).
    //     var non_internal_groups = []
    //     for group in get_groups():
    //     if not group.begins_with("_"):
    //     non_internal_groups.push_back(group)
    //     [/gdscript]
    //     [csharp]
    //     // Stores the node's non-internal groups only (as a List of strings).
    //     List<string> nonInternalGroups = new List<string>();
    //     foreach (string group in GetGroups())
    //     {
    //     if (!group.BeginsWith("_"))
    //     nonInternalGroups.Add(group);
    //     }
    //     [/csharp]
    //     [/codeblocks]
    Array<StringName> GetGroups();

    //
    // Summary:
    //     Queues a node for deletion at the end of the current frame. When deleted, all
    //     of its child nodes will be deleted as well. This method ensures it's safe to
    //     delete the node, contrary to Godot.Object.Free. Use Godot.Object.IsQueuedForDeletion
    //     to check whether a node will be deleted at the end of the frame.
    void QueueFree();

    //
    // Summary:
    //     Deletes the object from memory. Pre-existing references to the object become
    //     invalid, and any attempt to access them will result in a run-time error. Checking
    //     the references with @GlobalScope.is_instance_valid will return false.
    void Free();

    //
    // Summary:
    //     Sets the node's multiplayer authority to the peer with the given peer ID. The
    //     multiplayer authority is the peer that has authority over the node on the network.
    //     Useful in conjunction with Godot.Node.RpcConfig(Godot.StringName,Godot.Variant)
    //     and the Godot.MultiplayerAPI. Inherited from the parent node by default, which
    //     ultimately defaults to peer ID 1 (the server). If [param recursive], the given
    //     peer is recursively set as the authority for all children of this node.
    void SetMultiplayerAuthority(int id, bool recursive = true);

    //
    // Summary:
    //     Returns the peer ID of the multiplayer authority for this node. See Godot.Node.SetMultiplayerAuthority(System.Int32,System.Boolean).
    int GetMultiplayerAuthority();

    //
    // Summary:
    //     Returns true if the local system is the multiplayer authority of this node.
    bool IsMultiplayerAuthority();

    //
    // Summary:
    //     Converts this Godot.Object to a string.
    //
    // Returns:
    //     A string representation of this object.
    string ToString();
}
