using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Static
{
    public class StaticTreatmentRepository : ITreatmentRepository
    {
        private readonly BinarySearchTree<Treatment> _tree;

        public StaticTreatmentRepository()
        {
            _tree = new BinarySearchTree<Treatment>();
            _tree.Insert(1, new Treatment(1, "RodentBlock Pro", "Bait Station", 1, "Keep away from children and pets. Use tamper-resistant bait stations only. Wash hands after handling."));
            _tree.Insert(2, new Treatment(2, "MouseGuard Snap", "Mechanical Trap", 2, "Set traps along walls away from foot traffic. Check traps daily. Dispose of caught rodents hygienically."));
            _tree.Insert(3, new Treatment(3, "InsectaClear Gel", "Gel Bait Application", 3, "Apply in small dots in cracks and crevices. Avoid food preparation surfaces. Ventilate area after application."));
            _tree.Insert(4, new Treatment(4, "BedBugHeat Treatment", "Thermal Remediation", 4, "Room temperature raised to 56C for sustained period. Remove heat-sensitive items. Area must be vacated during treatment."));
            _tree.Insert(5, new Treatment(5, "WaspNest Powder", "Insecticidal Dust", 5, "Apply at dusk when wasps are less active. Wear protective equipment. Keep people away for 24 hours."));
            _tree.Insert(6, new Treatment(6, "PigeonNet System", "Bird Netting", 6, "Professional installation required. Stainless steel fixings. UV-stabilised polyethylene netting. Annual inspection recommended."));
            _tree.Insert(7, new Treatment(7, "SquirrelCage Trap", "Live Capture Trap", 7, "Check traps twice daily minimum. Bait with peanut butter. Legal requirements apply to grey squirrel release."));
            _tree.Insert(8, new Treatment(8, "FoxDeter Spray", "Scent Deterrent", 8, "Apply around perimeter weekly. Non-toxic formula. Reapply after heavy rain. Safe for garden use."));
            _tree.Insert(9, new Treatment(9, "MothCedar Blocks", "Natural Repellent", 9, "Place in wardrobes and drawers. Replace every 6 months. Sand surface to refresh scent. Non-toxic."));
            _tree.Insert(10, new Treatment(10, "AntBait Station", "Bait Station", 10, "Place near ant trails. Do not disturb stations. Allow 2-3 weeks for colony elimination. Replace monthly."));
        }

        public List<Treatment> GetAll()
        {
            return _tree.GetAll();
        }

        public Treatment GetById(int id)
        {
            return _tree.Search(id);
        }

        public void Add(Treatment treatment)
        {
            treatment.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(treatment.Id, treatment);
        }

        public void Update(Treatment treatment)
        {
            var existing = _tree.Search(treatment.Id);
            if (existing != null)
            {
                existing.ProductName = treatment.ProductName;
                existing.Method = treatment.Method;
                existing.TargetPestTypeId = treatment.TargetPestTypeId;
                existing.SafetyInfo = treatment.SafetyInfo;
            }
        }

        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        public List<Treatment> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(t =>
                t.ProductName.ToLower().Contains(lower) ||
                t.Method.ToLower().Contains(lower) ||
                t.SafetyInfo.ToLower().Contains(lower)
            );
        }
    }
}
