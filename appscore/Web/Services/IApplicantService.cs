using System.Collections.Generic;
using System.Threading.Tasks;
using Web.ViewModels;

namespace Web.Services
{
    public interface IApplicantService
    {
        Task<IEnumerable<Applicant>> GetApplicants();
    }
}