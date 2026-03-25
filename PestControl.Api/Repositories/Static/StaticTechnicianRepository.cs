using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;

namespace PestControl.Api.Repositories.Static
{
    public class StaticTechnicianRepository : ITechnicianRepository
    {
        private readonly BinarySearchTree<Technician> _tree;

        public StaticTechnicianRepository()
        {
            _tree = new BinarySearchTree<Technician>();
            _tree.Insert(1, new Technician(1, "James Wilson", "Rodents", "07700800001", "j.wilson@pestpro.com", true));
            _tree.Insert(2, new Technician(2, "Rachel Green", "Insects", "07700800002", "r.green@pestpro.com", true));
            _tree.Insert(3, new Technician(3, "Tom Baker", "Wildlife", "07700800003", "t.baker@pestpro.com", true));
            _tree.Insert(4, new Technician(4, "Sophie Clark", "Birds", "07700800004", "s.clark@pestpro.com", false));
        }

        public List<Technician> GetAll()
        {
            return _tree.GetAll();
        }

        public Technician GetById(int id)
        {
            return _tree.Search(id);
        }

        public void Add(Technician technician)
        {
            technician.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(technician.Id, technician);
        }

        public void Update(Technician technician)
        {
            var existing = _tree.Search(technician.Id);
            if (existing != null)
            {
                existing.Name = technician.Name;
                existing.Specialisation = technician.Specialisation;
                existing.Phone = technician.Phone;
                existing.Email = technician.Email;
                existing.Available = technician.Available;
            }
        }

        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        public List<Technician> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(t =>
                t.Name.ToLower().Contains(lower) ||
                t.Specialisation.ToLower().Contains(lower) ||
                t.Email.ToLower().Contains(lower) ||
                t.Phone.Contains(lower)
            );
        }
    }
}
