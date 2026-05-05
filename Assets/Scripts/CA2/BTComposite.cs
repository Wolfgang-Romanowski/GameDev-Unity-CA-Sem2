using System.Collections.Generic;

public class BTSequence : BTNode
{
    private List<BTNode> children;

    public BTSequence(string name, List<BTNode> children) : base(name)
    {
        this.children = children;
    }

    public override BTStatus Tick()
    {
        foreach (var child in children)
        {
            BTStatus status = child.Tick();
            if (status != BTStatus.Success)
                return status;
        }
        return BTStatus.Success;
    }
}

public class BTSelector : BTNode
{
    private List<BTNode> children;

    public BTSelector(string name, List<BTNode> children) : base(name)
    {
        this.children = children;
    }

    public override BTStatus Tick()
    {
        foreach (var child in children)
        {
            BTStatus status = child.Tick();
            if (status != BTStatus.Failure)
                return status;
        }
        return BTStatus.Failure;
    }
}