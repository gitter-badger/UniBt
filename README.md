# UBT - Behavior Trees for Unity
Created by EjongHyuck([@HansMakart](https://twitter.com/HansMakart))


## Table of Contents
- [What is UBT?](#what-is-ubt)
- [Why UBT?](#why-ubt)
  - [UBT is Event-Driven](#ubt-is-event-driven)
  - [Conditionals are not Leaf Nodes](#conditionals-are-not-leaf-nodes)
  - [Special Handling for Concurrent Behaviors](#special-handling-for-concurrent-behaviors)
     - [Why not use Parallel nodes?](#why-not-use-parallel-nodes)
	 - [Services](#Services)
	 - [Decorator "Observer Aborts" Property](#decorator-observer-aborts-property)
  - [Advantage of UBT's approach to Concurrent Behaviors](#advantage-of-ubts-approach-to-concurrent-behaviors)
     - [Clarity](#clarity)
	 - [Ease of Debugging](#ease-of-debugging)
	 - [Easier Optimizations](#easier-optimizations)
- [Why using UniRx?](#why-using-unirx)
- [Introduction](#introduction)


## What is UBT?
UBT(Unity Behavior Trees) is a reimplementation in Unreal Engine behavior trees style.
The "standard" behavior trees are good but every frames must be a lot of work and It is complex due to the need to create a number of nodes to confirm the condition.
This library is to pursue behavior trees style of the Unreal Engine.
Supported platforms are PC/WebGL/etc and the library is fully supported on Unity 5 and UniRx. *(Further testing is required.)*


## Why UBT?
There are three critical ways in which the UBT implementation of Behavior Trees differs from "standard" behavior trees.
*(This article borrowed from [How Unreal Engine 4 Behavior Trees Differ](https://docs.unrealengine.com/latest/INT/Engine/AI/BehaviorTrees/HowUE4BehaviorTreesDiffer/index.html))*

### UBT is Event-Driven
** Event-Driven ** behavior trees avoid doing lots of work every frame.
Instead of constantly checking wheather are relevant changes has occurred, the behavior trees just passively listen for "events" which can trigger changes in the tree.

Having an event-driven architecture grants improvements to both performance are debugging.
However, to take the most advantage of these improvements, you will need to understand the other difference to our trees and structure your behavior trees appropriately.

Since the code does not have to iterate through the entire tree every tick, performance is much better!
Conceptually, instead of constantly asking "Are we there yet?", we can just rest until we are prodded and told "We are there!"


### Conditionals are not Leaf Nodes
In the standard model for behavior trees, conditions are "Task" leaf nodes, which simply do not do anything other than succeed or fail.
Although nothing prevents you from making traditional conditional tasks, it is highly recommended that you use our Decorator system for conditionals instead.

First, conditional decorators make the behavior tree UI more intuitive and easier to read.
Since conditionals are at the root of the sub-tree they are controlling, you can immediately see what part of the tree is "closed off" if the conditionals are not met.
Also, since all leaves are action tasks, it is easier to see what actual actions are being ordered by the tree.
In a traditional model, conditionals would be among the leaves, so you would have to spend more time figuring out which leaves are conditionals and which leaves are actions.

![](https://bytebucket.org/ejonghyuck/unitybehaviortree/raw/d7386412841e6e802c5003074915cda8607a25c4/Resources/ubt_1.png?token=db40c7d023ebca33c655012932f368785878c3f0)

*In this section of a behavior tree, the Decorator Option Value can prevent the execution of the Sequence node's children.*

Another advantage of conditional decorators is that it is easy to make those decorators act as observers (waiting for events) at critical nodes in the tree.
This feature is critical to gaining full advantage from the event-driven nature of the trees.


### Special Handling for Concurrent Behaviors
Standard behavior trees often use a Parallel composite node to handle concurrent behaviors.
The Parallel node begins execution on all of its children simultaneously.
Special rules determine how to act if one or more of those child trees finish (depending on the desired behavior).

> Parallel nodes are not necessarily mult-threading (executing tasks at "truly" the same time).
> They are just a way to conceptually perform serveral tasks at once.
> Often they still run on the same thread and begin in some sequence.
> That sequence should be irrelevant since they will all happen in the same frame, but it is still sometimes important.

Instead of complex Parallel nodes, UBT use our own special node type wich we call **Services** to accomplish the same sorts of behaviors.

#### Why not use Parallel nodes?
There are two types of nodes which provide the functionality that would normally come from Parallel nodes:

#### Services
Services are special nodes associated with any composite node (Selector, Sequence), which can register for callbacks every X seconds and perform updates of various sorts that need to occur periodically.
For example, a service can be used to determine which enemy is the best choice for the AI game object to pursue while the game object continues to act normally in its behavior tree toward its current enemy.

Services are active only as long as execution remains in the subtree rooted at the composite node with which the service is associated.

#### Decorator "Observer Aborts" Property
One common usage case for standard Parallel nodes is to constantly check conditions so task can abort if the conditions it requires becomes *false*.
For example, if you have a cat that performs a sequence "Shake Rear End", "Pounce", you may want to give up immediately if the mouse escape into its mouse hole.
With Parallel nodes, you would have a child that checks if the mouse can be pounced on, and then another child that's the sequence to perform.
Since our Behavior Trees are event-driven, we instead handle this by having our conditional decorators observer their values and abort when necessary.
(In this example, you would just have the "Mouse can be pounced on?" decorator on the sequence itself, with "Observer Aborts" set to "Self", *This will immediately support.*)


### Advantages of UBT's approach to Concurrent Behaviors

#### Clarity
Using Services and Simple Parallel nodes creates simple trees that are easier to understand.

#### Ease of Debugging
Clearer graphs are easier to debug.
In addition, having fewer simultaneous execution paths is a huge boon to watching what is actually happening in the graph.

#### Easier Optimizations
Event-driven graphs are easier to optimize if they do not have a lot of subtrees "simultaneously" executing.


## Why using UniRx?
Ordinarily, using `Coroutine` is not good practice for event-driven for the following (and other) reasons:

1. Coroutines can't handle exceptions, because yield return statements can't be surrounded with a try-catch construction.
2. Coroutines have limited to the implementation of the code.
3. Coroutines have limited time to operate.

UniRx cures that kind of "asynchronous blues".
Rx is a library for composing asynchronous and event-based programs (like UBT architecture) and LINQ-style query operators.

The task loops are all types of events.
UniRx represents events as reactive sequences which are both easily composable and support time-based operations by using LINQ query operators.
And Unity is generally single threaded but UniRx facilitates multi-threading for joins, cancels, accessing GameObjects, etc.
We were actively apply UniRx by this merit.


## Introduction
Great introduction to Behavior Trees and Rx articles: [UE4 Behavior Trees](https://docs.unrealengine.com/latest/INT/Engine/AI/BehaviorTrees/index.html), [The introduction to Reactive Programming you've been missing](https://gist.github.com/staltz/868e7e9bc2a7b8c1f754).

### Decorators
The following code implements example from the article in UBT.
```
public bool AlwaysTrue()
{
    return true;
}
```

### Services
The following code implements the 'debug message' service example from the article in UBT.
```
public void DebugMessage()
{
    Debug.Log("[Service] Debug Message");
}
```
This example demonstrates the following features:

- Services just as an callback
- Services are perform updates of various sorts that need to occur periodically

### Tasks
Use `System.IDisposable` for asynchronous task loops.
Its Get/Post functions return subscribable IObservables.
```
public System.IDisposable Wait()
{
    var subscription = Observable.Timer(TimeSpan.FromSeconds(2))
        .Subscribe(_ => this.FinishExecute(true));
    return subscription;
}
```
This example demonstrates the following features:

- The task loop as an event stream
- Composable event streams
- Easy handling of time based operations