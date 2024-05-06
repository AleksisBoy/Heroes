
public class Sequence : Node 
{
    public Sequence(string n) 
    {
        name = n;
    }

    public override Status Process() 
    {
        Status childstatus = children[currentChild].Process();
        if (childstatus == Status.RUNNING) return Status.RUNNING;
        if (childstatus == Status.FAILURE) 
        {
            Reset();
            return childstatus;
        }

        currentChild++;
        if (currentChild >= children.Count) 
        {
            Reset();
            return Status.SUCCESS;
        }

        return Status.RUNNING;
    }


}
