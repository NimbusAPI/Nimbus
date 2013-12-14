namespace Pizza.WaitTimeService
{
    public interface IWaitTimeCounter
    {
        void RecordNewPizzaOrder(int id);
        void RecordPizzaCompleted(int id);
        int GetAveragePizzaTimes();
    }
}