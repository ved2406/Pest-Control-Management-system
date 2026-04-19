using System;
using System.Collections.Generic;

namespace PestControl.Api.DataStructures
{
    // Generic BST storing key-value pairs
    // Average O(log n) for insert/search/delete and O(n) for GetAll Count Filter
    public class BinarySearchTree<T>
    {
        private BstNode<T> _root;

        public BinarySearchTree()
        {
            _root = null;
        }

        // Inserts a new key-value pair or updates the value if the key already exists
        public void Insert(int key, T value)
        {
            _root = InsertNode(_root, key, value);
        }

        private static BstNode<T> InsertNode(BstNode<T> node, int key, T value)
        {
            if (node == null)
                return new BstNode<T>(key, value);

            if (key < node.Key)
                node.Left = InsertNode(node.Left, key, value);
            else if (key > node.Key)
                node.Right = InsertNode(node.Right, key, value);
            else
                node.Value = value;

            return node;
        }

        // Returns the value for the given key or default(T) if not found
        public T Search(int key)
        {
            var node = FindNode(_root, key);
            return node != null ? node.Value : default;
        }

        private static BstNode<T> FindNode(BstNode<T> node, int key)
        {
            if (node == null || node.Key == key)
                return node;

            return key < node.Key
                ? FindNode(node.Left, key)
                : FindNode(node.Right, key);
        }

        // Removes the node with the given key
        // Handles leaf single-child and two-children cases (replaced by in-order successor)
        public void Delete(int key)
        {
            _root = RemoveNode(_root, key);
        }

        private static BstNode<T> RemoveNode(BstNode<T> node, int key)
        {
            if (node == null) return null;

            if (key < node.Key)
                node.Left = RemoveNode(node.Left, key);
            else if (key > node.Key)
                node.Right = RemoveNode(node.Right, key);
            else
            {
                if (node.Left == null && node.Right == null) return null;
                if (node.Left == null) return node.Right;
                if (node.Right == null) return node.Left;

                // two children - replace with in-order successor (leftmost node in right subtree)
                var successor = LeftMost(node.Right);
                node.Key = successor.Key;
                node.Value = successor.Value;
                node.Right = RemoveNode(node.Right, successor.Key);
            }

            return node;
        }

        private static BstNode<T> LeftMost(BstNode<T> node)
        {
            while (node.Left != null) node = node.Left;
            return node;
        }

        // Returns all values in ascending key order via in-order traversal
        public List<T> GetAll()
        {
            var result = new List<T>();
            InOrder(_root, result);
            return result;
        }

        private static void InOrder(BstNode<T> node, List<T> result)
        {
            if (node == null) return;
            InOrder(node.Left, result);
            result.Add(node.Value);
            InOrder(node.Right, result);
        }

        // Returns the total number of nodes in the tree
        public int Count()
        {
            return CountNodes(_root);
        }

        private static int CountNodes(BstNode<T> node)
        {
            if (node == null) return 0;
            return 1 + CountNodes(node.Left) + CountNodes(node.Right);
        }

        // Returns the largest key by walking to the rightmost node
        public int MaxKey()
        {
            if (_root == null) return 0;
            var node = _root;
            while (node.Right != null) node = node.Right;
            return node.Key;
        }

        // Returns all values that satisfy the given predicate
        public List<T> Filter(Func<T, bool> predicate)
        {
            var result = new List<T>();
            FilterNodes(_root, predicate, result);
            return result;
        }

        private static void FilterNodes(BstNode<T> node, Func<T, bool> predicate, List<T> result)
        {
            if (node == null) return;
            FilterNodes(node.Left, predicate, result);
            if (predicate(node.Value)) result.Add(node.Value);
            FilterNodes(node.Right, predicate, result);
        }
    }
}
