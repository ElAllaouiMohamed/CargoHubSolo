using CargohubV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargohubV2.Services
{
    public interface IClientService
    {
        Task<List<Client>> GetClientsAsync(int limit);
        Task<List<Client>> GetAllClientsAsync();
        Task<Client> GetByIdAsync(int id);
        Task<List<ContactPerson>> GetContactPersonsByClientIdAsync(int clientId);
        Task<ContactPerson> AddContactPersonToClientAsync(int clientId, ContactPerson contactPerson);
        Task<ContactPerson> UpdateContactPersonAsync(int contactPersonId, ContactPerson updated);
        Task<bool> DeleteContactPersonAsync(int contactPersonId);
        Task<Client> AddClientAsync(Client client);
        Task<Client> UpdateClientAsync(int id, Client updated);
        Task<bool> SoftDeleteByIdAsync(int id);
    }
}
