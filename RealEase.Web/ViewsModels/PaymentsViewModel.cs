using RealEase.Domain.Entities;

namespace RealEase.Web.ViewsModels
{
    public class PaymentsViewModel
    {

        public List<Payment> ActivePayments { get; set; }

        public List<Payment> AllPayments { get; set; }
    }
}

