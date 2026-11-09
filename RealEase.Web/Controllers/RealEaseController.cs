using RealEase.Persistence.Context;

namespace RealEase.Web.Controllers
{
    internal class RealEaseController
    {
        private RealEaseDbContext db;

        public RealEaseController(RealEaseDbContext db)
        {
            this.db = db;
        }
    }
}