using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels
{
    public class Applicant
    {
        public int ApplicantId { get; set; }
        public string Name { get; set; }      
        public string Email { get; set; }   
        public string Address { get; set; }    
        public string PhoneNo { get; set; }   
    }
}