namespace PestControl.Api.DataStructures
{
    // A node in the BST holding an int key for ordering and a generic value
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
