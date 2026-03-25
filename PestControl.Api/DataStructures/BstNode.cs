namespace PestControl.Api.DataStructures
{
    /// <summary>
    /// A single node in the Binary Search Tree.
    /// Stores a key (int) used for ordering and a generic value of type T.
    /// </summary>
    public class BstNode<T>
    {
        public int Key { get; set; }
        public T Value { get; set; }
        public BstNode<T> Left { get; set; }
        public BstNode<T> Right { get; set; }

        public BstNode(int key, T value)
        {
            Key = key;
            Value = value;
            Left = null;
            Right = null;
        }
    }
}
