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