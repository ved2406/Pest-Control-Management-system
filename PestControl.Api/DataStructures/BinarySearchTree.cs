using System;
using System.Collections.Generic;

namespace PestControl.Api.DataStructures
{
    /// <summary>
    /// Custom Binary Search Tree implementation for storing data objects.
    /// Uses integer keys (IDs) for ordering.
    ///
    /// Time Complexity Analysis:
    ///   Insert:     O(log n) average, O(n) worst case (degenerate/unbalanced tree)
    ///   Search:     O(log n) average, O(n) worst case
    ///   Delete:     O(log n) average, O(n) worst case
    ///   Traverse:   O(n) always (must visit every node)
    ///   GetAll:     O(n) - in-order traversal returns sorted results
    ///   FindMax:    O(log n) average, O(n) worst case
    ///   Count:      O(n) - must traverse entire tree
    ///
    /// Space Complexity: O(n) for n elements stored
    /// </summary>
    public class BinarySearchTree<T>
    {
        private BstNode<T> _root;

        public BinarySearchTree()
        {
            _root = null;
        }

        /// <summary>
        /// Inserts a new key-value pair into the BST.
        /// If the key already exists, the value is updated.
        /// Average: O(log n), Worst: O(n)
        /// </summary>
        public void Insert(int key, T value)
        {
            _root = InsertRecursive(_root, key, value);
        }

        private BstNode<T> InsertRecursive(BstNode<T> node, int key, T value)
        {
            // Base case: found the correct empty position
            if (node == null)
            {
                return new BstNode<T>(key, value);
            }

            if (key < node.Key)
            {
                // Key is smaller, go left
                node.Left = InsertRecursive(node.Left, key, value);
            }
            else if (key > node.Key)
            {
                // Key is larger, go right
                node.Right = InsertRecursive(node.Right, key, value);
            }
            else
            {
                // Key already exists, update the value
                node.Value = value;
            }

            return node;
        }

        /// <summary>
        /// Searches for a value by its key.
        /// Returns default(T) if not found.
        /// Average: O(log n), Worst: O(n)
        /// </summary>
        public T Search(int key)
        {
            var node = SearchRecursive(_root, key);
            return node != null ? node.Value : default;
        }

        private BstNode<T> SearchRecursive(BstNode<T> node, int key)
        {
            // Base case: not found or key matches
            if (node == null || node.Key == key)
            {
                return node;
            }

            if (key < node.Key)
            {
                return SearchRecursive(node.Left, key);
            }
            else
            {
                return SearchRecursive(node.Right, key);
            }
        }

        /// <summary>
        /// Deletes a node by key from the BST.
        /// Average: O(log n), Worst: O(n)
        /// </summary>
        public void Delete(int key)
        {
            _root = DeleteRecursive(_root, key);
        }

        private BstNode<T> DeleteRecursive(BstNode<T> node, int key)
        {
            if (node == null)
            {
                return null;
            }

            if (key < node.Key)
            {
                node.Left = DeleteRecursive(node.Left, key);
            }
            else if (key > node.Key)
            {
                node.Right = DeleteRecursive(node.Right, key);
            }
            else
            {
                // Node to delete found

                // Case 1: Node has no children (leaf node)
                if (node.Left == null && node.Right == null)
                {
                    return null;
                }

                // Case 2: Node has only one child
                if (node.Left == null)
                {
                    return node.Right;
                }
                if (node.Right == null)
                {
                    return node.Left;
                }

                // Case 3: Node has two children
                // Find the in-order successor (smallest node in right subtree)
                var successor = FindMin(node.Right);
                node.Key = successor.Key;
                node.Value = successor.Value;
                // Delete the in-order successor from the right subtree
                node.Right = DeleteRecursive(node.Right, successor.Key);
            }

            return node;
        }

        /// <summary>
        /// Finds the node with the minimum key in the subtree.
        /// Average: O(log n), Worst: O(n)
        /// </summary>
        private BstNode<T> FindMin(BstNode<T> node)
        {
            while (node.Left != null)
            {
                node = node.Left;
            }
            return node;
        }

        /// <summary>
        /// Returns all values in the tree via in-order traversal (sorted by key).
        /// Always O(n) as every node must be visited.
        /// </summary>
        public List<T> GetAll()
        {
            var result = new List<T>();
            InOrderTraversal(_root, result);
            return result;
        }

        private void InOrderTraversal(BstNode<T> node, List<T> result)
        {
            if (node == null) return;
            InOrderTraversal(node.Left, result);
            result.Add(node.Value);
            InOrderTraversal(node.Right, result);
        }

        /// <summary>
        /// Returns the number of elements in the tree.
        /// O(n) - traverses entire tree.
        /// </summary>
        public int Count()
        {
            return CountRecursive(_root);
        }

        private int CountRecursive(BstNode<T> node)
        {
            if (node == null) return 0;
            return 1 + CountRecursive(node.Left) + CountRecursive(node.Right);
        }

        /// <summary>
        /// Returns the maximum key in the tree.
        /// Average: O(log n), Worst: O(n)
        /// </summary>
        public int MaxKey()
        {
            if (_root == null) return 0;
            var node = _root;
            while (node.Right != null)
            {
                node = node.Right;
            }
            return node.Key;
        }

        /// <summary>
        /// Searches all values using a predicate function.
        /// Always O(n) - must check every node.
        /// </summary>
        public List<T> Filter(Func<T, bool> predicate)
        {
            var result = new List<T>();
            FilterTraversal(_root, predicate, result);
            return result;
        }

        private void FilterTraversal(BstNode<T> node, Func<T, bool> predicate, List<T> result)
        {
            if (node == null) return;
            FilterTraversal(node.Left, predicate, result);
            if (predicate(node.Value))
            {
                result.Add(node.Value);
            }
            FilterTraversal(node.Right, predicate, result);
        }
    }
}
