using Web.Bff.Sample.Models;

namespace Web.Bff.Sample.Services;

public interface IPersonService
{
   Task<PersonData> GetPersonByIdAsync(int id); 

   Task<PersonData> UpdatePersonAsync(PersonData currentPerson);
}