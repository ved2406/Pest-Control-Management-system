using System;
using System.Collections.Generic;

namespace PestControl.Api.DataStructures
{
    /// <summary>
    /// BinarySearchTree is a custom data structure built from scratch (no built-in collections used).
    /// It stores key-value pairs where the key is an integer (used for ordering)
    /// and the value is a generic type T (could be a Customer, Booking, string, etc.)
    ///
    /// HOW A BST WORKS:
    ///   - Every node has a key, a left child, and a right child
    ///   - Left child always has a SMALLER key than the parent
    ///   - Right child always has a LARGER key than the parent
    ///   - This rule means searching is fast — at each step you halve the remaining nodes
    ///
    /// TIME COMPLEXITY:
    ///   Insert:   O(log n) average — halve the tree at each step
    ///   Search:   O(log n) average — same reason
    ///   Delete:   O(log n) average
    ///   GetAll:   O(n)     — must visit every node
    ///   Count:    O(n)     — must visit every node
    ///   WORST CASE is O(n) if tree becomes a straight line (e.g. insert 1,2,3,4,5 in order)
    ///
    /// SPACE COMPLEXITY: O(n) — one node per item stored
    /// </summary>
    public class BinarySearchTree<T>
    {
        // The root is the top node of the tree. Null if the tree is empty.
        private BstNode<T> _root;

        public BinarySearchTree()
        {
            _root = null; // start with an empty tree
        }

        /// <summary>
        /// Insert adds a new key-value pair to the tree.
        /// It uses recursion — calls itself on left or right subtree until it finds an empty spot.
        /// If the key already exists, the value is updated instead of adding a duplicate.
        /// Average time: O(log n)
        /// </summary>
        public void Insert(int key, T value)
        {
            _root = InsertRecursive(_root, key, value);
        }

        private BstNode<T> InsertRecursive(BstNode<T> node, int key, T value)
        {
            // BASE CASE: we've reached an empty spot — create the new node here
            if (node == null)
            {
                return new BstNode<T>(key, value);
            }

            if (key < node.Key)
            {
                // Key is smaller than current node — go LEFT and recurse
                node.Left = InsertRecursive(node.Left, key, value);
            }
            else if (key > node.Key)
            {
                // Key is larger than current node — go RIGHT and recurse
                node.Right = InsertRecursive(node.Right, key, value);
            }
            else
            {
                // Key already exists — update the value (no duplicates allowed)
                node.Value = value;
            }

            return node; // return the (possibly updated) node back up the call stack
        }

        /// <summary>
        /// Search finds a value by its key.
        /// At each node it compares the target key — goes left if smaller, right if larger.
        /// Returns default(T) (null for reference types) if not found.
        /// Average time: O(log n)
        /// </summary>
        public T Search(int key)
        {
            var node = SearchRecursive(_root, key);
            return node != null ? node.Value : default; // return value or null
        }

        private BstNode<T> SearchRecursive(BstNode<T> node, int key)
        {
            // BASE CASE: hit a null (not found) OR found the exact key
            if (node == null || node.Key == key)
            {
                return node;
            }

            if (key < node.Key)
            {
                return SearchRecursive(node.Left, key);  // go left — key must be smaller
            }
            else
            {
                return SearchRecursive(node.Right, key); // go right — key must be larger
            }
        }

        /// <summary>
        /// Delete removes a node by key. There are THREE cases to handle:
        ///
        /// CASE 1 — Leaf node (no children): simply remove it (return null)
        /// CASE 2 — One child: replace the deleted node with its single child
        /// CASE 3 — Two children: find the IN-ORDER SUCCESSOR (smallest node in right subtree),
        ///          copy its key and value up, then delete the successor from the right subtree.
        ///          The in-order successor is always the leftmost node in the right subtree.
        ///
        /// Average time: O(log n)
        /// </summary>
        public void Delete(int key)
        {
            _root = DeleteRecursive(_root, key);
        }

        private BstNode<T> DeleteRecursive(BstNode<T> node, int key)
        {
            if (node == null)
            {
                return null; // key not found — do nothing
            }

            if (key < node.Key)
            {
                node.Left = DeleteRecursive(node.Left, key);   // go left
            }
            else if (key > node.Key)
            {
                node.Right = DeleteRecursive(node.Right, key); // go right
            }
            else
            {
                // Found the node to delete — handle the 3 cases:

                // CASE 1: Leaf node — no children, just remove it
                if (node.Left == null && node.Right == null)
                {
                    return null;
                }

                // CASE 2a: Only has a right child — replace with right child
                if (node.Left == null)
                {
                    return node.Right;
                }

                // CASE 2b: Only has a left child — replace with left child
                if (node.Right == null)
                {
                    return node.Left;
                }

                // CASE 3: Has two children
                // Find the in-order successor: go right once, then keep going left
                var successor = FindMin(node.Right);

                // Copy the successor's data into this node
                node.Key = successor.Key;
                node.Value = successor.Value;

                // Delete the successor from the right subtree (it's now a duplicate)
                node.Right = DeleteRecursive(node.Right, successor.Key);
            }

            return node;
        }

        /// <summary>
        /// FindMin walks as far left as possible to find the smallest key in a subtree.
        /// Used internally for Delete Case 3 (finding the in-order successor).
        /// </summary>
        private BstNode<T> FindMin(BstNode<T> node)
        {
            while (node.Left != null)
            {
                node = node.Left; // keep going left until no left child
            }
            return node;
        }

        /// <summary>
        /// GetAll returns all values in SORTED ORDER using in-order traversal.
        /// In-order traversal visits nodes in this order: Left → Current → Right
        /// Because of the BST property, this naturally produces ascending key order.
        /// Always O(n) — every node must be visited.
        /// </summary>
        public List<T> GetAll()
        {
            var result = new List<T>();
            InOrderTraversal(_root, result);
            return result;
        }

        private void InOrderTraversal(BstNode<T> node, List<T> result)
        {
            if (node == null) return;          // base case
            InOrderTraversal(node.Left, result);  // 1. visit left subtree first
            result.Add(node.Value);               // 2. add current node's value
            InOrderTraversal(node.Right, result); // 3. visit right subtree last
        }

        /// <summary>
        /// Count returns the total number of nodes in the tree.
        /// Must traverse every node — O(n).
        /// Formula: count = 1 (this node) + count(left subtree) + count(right subtree)
        /// </summary>
        public int Count()
        {
            return CountRecursive(_root);
        }

        private int CountRecursive(BstNode<T> node)
        {
            if (node == null) return 0; // empty subtree contributes 0
            return 1 + CountRecursive(node.Left) + CountRecursive(node.Right);
        }

        /// <summary>
        /// MaxKey returns the largest key in the tree.
        /// In a BST, the largest key is always the rightmost node.
        /// Walk right until there is no more right child.
        /// Average: O(log n)
        /// </summary>
        public int MaxKey()
        {
            if (_root == null) return 0;
            var node = _root;
            while (node.Right != null)
            {
                node = node.Right; // keep going right
            }
            return node.Key;
        }

        /// <summary>
        /// Filter traverses the entire tree and returns all values that match the given condition.
        /// The predicate is a lambda function passed in by the caller.
        /// Example: tree.Filter(c => c.Name.StartsWith("J")) returns all customers with J names.
        /// Always O(n) — every node must be checked.
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
            FilterTraversal(node.Left, predicate, result);   // check left subtree
            if (predicate(node.Value))                        // does this value match?
            {
                result.Add(node.Value);                       // yes — include it
            }
            FilterTraversal(node.Right, predicate, result);  // check right subtree
        }
    }
}