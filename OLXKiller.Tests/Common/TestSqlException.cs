namespace OLXKiller.Tests.Common;

public class TestSqlException(int number, string message) : Exception(message)
{
    public int Number { get; private set; } = number;
}