public interface IDataSender
{
    void SetUdpCommunicator(UdpCommunicator communicator);
    bool IsEnabled { get; set; }
    string SenderName { get; }
}