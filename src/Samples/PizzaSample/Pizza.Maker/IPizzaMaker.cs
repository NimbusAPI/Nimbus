namespace Pizza.Maker
{
    public interface IPizzaMaker
    {
        void TakePizzaOrder(int pizzaId);
        void CompletePizza(int pizzaId);
    }
}