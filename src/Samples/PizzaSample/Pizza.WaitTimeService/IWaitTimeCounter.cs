namespace Pizza.WaitTimeService
{
    public interface IWaitTimeCounter
    {
        void RecordNewPizzaOrder(string customerName);
        void RecordPizzaCompleted(string customerName);
        int GetAveragePizzaTimes();
    }
}