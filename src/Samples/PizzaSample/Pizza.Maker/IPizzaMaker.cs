using System.Threading.Tasks;

namespace Pizza.Maker
{
    public interface IPizzaMaker
    {
        Task MakePizzaForCustomer(string customerName);
    }
}