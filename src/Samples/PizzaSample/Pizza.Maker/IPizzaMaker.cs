namespace Pizza.Maker
{
    public interface IPizzaMaker
    {
        void TakePizzaOrder(string customerName);
        void CompletePizza(string customerName);
    }
}