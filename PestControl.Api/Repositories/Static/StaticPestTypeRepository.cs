using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Static
{
    /// <summary>
    /// A fake pest type repository that stores everything in memory using a binary tree.
    /// Good for testing — no database, just a catalog of pests we know how to treat.
    /// </summary>
    public class StaticPestTypeRepository : IPestTypeRepository
    {
        // Stores pest types in a binary search tree (faster lookups than a plain list)
        private readonly BinarySearchTree<PestType> _tree;

        // Set up with test pest types — our catalog of common pests
        public StaticPestTypeRepository()
        {
            _tree = new BinarySearchTree<PestType>();
            _tree.Insert(1, new PestType(1, "Brown Rat", "Rodents", "Common rat found in urban areas. Can cause structural damage and spread disease.", "High"));
            _tree.Insert(2, new PestType(2, "House Mouse", "Rodents", "Small rodent that contaminates food supplies and causes wiring damage.", "Medium"));
            _tree.Insert(3, new PestType(3, "German Cockroach", "Insects", "Fast-breeding insect found in kitchens and bathrooms. Carries bacteria.", "High"));
            _tree.Insert(4, new PestType(4, "Bed Bug", "Insects", "Parasitic insect that feeds on human blood. Causes itchy bites and allergic reactions.", "Medium"));
            _tree.Insert(5, new PestType(5, "Common Wasp", "Insects", "Aggressive when threatened. Nests in roofs, walls and garden structures.", "Medium"));
            _tree.Insert(6, new PestType(6, "Feral Pigeon", "Birds", "Urban bird that damages buildings with droppings and carries diseases.", "Low"));
            _tree.Insert(7, new PestType(7, "Grey Squirrel", "Wildlife", "Can cause significant damage to roof spaces, wiring and insulation.", "Medium"));
            _tree.Insert(8, new PestType(8, "Fox", "Wildlife", "Urban fox that scavenges bins and can carry mange and other diseases.", "Low"));
            _tree.Insert(9, new PestType(9, "Clothes Moth", "Insects", "Larvae damage natural fabrics, carpets and upholstery.", "Low"));
            _tree.Insert(10, new PestType(10, "Pharaoh Ant", "Insects", "Tropical ant species common in heated buildings. Difficult to eradicate.", "High"));
            _tree.Insert(11, new PestType(11, "Black Garden Ant", "Insects", "Common ant that nests outdoors but enters buildings seeking food.", "Low"));
            _tree.Insert(12, new PestType(12, "Carpet Beetle", "Insects", "Larvae feed on natural fibres causing damage to carpets and clothing.", "Low"));
        }

        // Get all pest types in our catalog
        public List<PestType> GetAll()
        {
            return _tree.GetAll();
        }

        // Look up a specific pest type by ID
        public PestType GetById(int id)
        {
            return _tree.Search(id);
        }

        // Add a new pest type to catalog
        public void Add(PestType pestType)
        {
            pestType.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(pestType.Id, pestType);
        }

        // Update a pest type's info
        public void Update(PestType pestType)
        {
            var existing = _tree.Search(pestType.Id);
            if (existing != null)
            {
                existing.Name = pestType.Name;
                existing.Category = pestType.Category;
                existing.Description = pestType.Description;
                existing.RiskLevel = pestType.RiskLevel;
            }
        }

        // Remove a pest type from catalog
        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        // Search pests by name, category, description, or risk level
        public List<PestType> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(p =>
                p.Name.ToLower().Contains(lower) ||
                p.Category.ToLower().Contains(lower) ||
                p.Description.ToLower().Contains(lower) ||
                p.RiskLevel.ToLower().Contains(lower)
            );
        }
    }
}
