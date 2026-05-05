using System;

public enum BTStatus { Success, Failure, Running }

public abstract class BTNode
{
    public string Name { get; protected set; }

    public BTNode(string name)
    {
        Name = name;
    }

    public abstract BTStatus Tick();
}

public class ConditionNode : BTNode
{
    private Func<bool> condition;

    public ConditionNode(string name, Func<bool> condition) : base(name)
    {
        this.condition = condition;
    }

    public override BTStatus Tick()
    {
        return condition() ? BTStatus.Success : BTStatus.Failure;
    }
}