using Drug.API.Data;

namespace Drug.API.Services
{
    public class DrugJobService
    {
        private readonly DrugDBContext _context;

        public DrugJobService(DrugDBContext context)
        {
            _context = context;
        }

        public async Task ProcessDrugDataAsync()
        {
            // Example: Fetch all drugs and perform some processing
            Console.WriteLine($"Processing Drugs");
            await Task.Delay(500);
            Console.WriteLine($"Processing Done");
        }
    }
}
