using PestControl.Api.DataStructures;
using PestControl.Api.Models;
using PestControl.Api.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PestControl.Api.Repositories.Static
{
    public class StaticCustomerRepository : ICustomerRepository
    {
        private readonly BinarySearchTree<Customer> _tree;

        public StaticCustomerRepository()
        {
            _tree = new BinarySearchTree<Customer>();
            _tree.Insert(1, new Customer(1, "John Smith", "12 Oak Lane, London, E1 6AN", "07700900001", "john.smith@email.com", "Residential"));
            _tree.Insert(2, new Customer(2, "Sarah Johnson", "45 High Street, Manchester, M1 1AD", "07700900002", "sarah.j@email.com", "Commercial"));
            _tree.Insert(3, new Customer(3, "Premier Restaurant Ltd", "88 Victoria Road, Birmingham, B1 1BB", "07700900003", "info@premierrest.com", "Commercial"));
            _tree.Insert(4, new Customer(4, "David Williams", "3 Park Avenue, Leeds, LS1 1UR", "07700900004", "d.williams@email.com", "Residential"));
            _tree.Insert(5, new Customer(5, "Emma Brown", "27 Church Street, Bristol, BS1 1HT", "07700900005", "emma.b@email.com", "Residential"));
            _tree.Insert(6, new Customer(6, "City Hotel Group", "100 Queen Street, Edinburgh, EH2 1JE", "07700900006", "ops@cityhotel.com", "Commercial"));
            _tree.Insert(7, new Customer(7, "Michael Taylor", "9 Mill Lane, Cardiff, CF10 1FL", "07700900007", "m.taylor@email.com", "Residential"));
            _tree.Insert(8, new Customer(8, "Greenfield School", "55 Academy Road, Liverpool, L1 1JQ", "07700900008", "admin@greenfield.edu", "Commercial"));
            _tree.Insert(9, new Customer(9, "Lisa Anderson", "71 Elm Close, Nottingham, NG1 1AB", "07700900009", "lisa.a@email.com", "Residential"));
            _tree.Insert(10, new Customer(10, "Thames Warehouse Co", "200 Dock Road, London, E14 9TS", "07700900010", "contact@thameswarehouse.com", "Industrial"));
        }

        public List<Customer> GetAll()
        {
            return _tree.GetAll();
        }

        public Customer GetById(int id)
        {
            return _tree.Search(id);
        }

        public void Add(Customer customer)
        {
            customer.Id = _tree.Count() > 0 ? _tree.MaxKey() + 1 : 1;
            _tree.Insert(customer.Id, customer);
        }

        public void Update(Customer customer)
        {
            var existing = _tree.Search(customer.Id);
            if (existing != null)
            {
                existing.Name = customer.Name;
                existing.Address = customer.Address;
                existing.Phone = customer.Phone;
                existing.Email = customer.Email;
                existing.PropertyType = customer.PropertyType;
            }
        }

        public void Delete(int id)
        {
            _tree.Delete(id);
        }

        public List<Customer> Search(string query)
        {
            var lower = query.ToLower();
            return _tree.Filter(c =>
                c.Name.ToLower().Contains(lower) ||
                c.Address.ToLower().Contains(lower) ||
                c.Email.ToLower().Contains(lower) ||
                c.Phone.Contains(lower) ||
                c.PropertyType.ToLower().Contains(lower)
            );
        }
    }
}
