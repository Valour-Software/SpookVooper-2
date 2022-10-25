using IdGen;

namespace SV2.Managers;

public static class IdManagers
{
    public static IdManager TransactionIdGenerator = new(0);
    public static IdManager ItemTradeIdGenerator = new(1);
    public static IdManager UBIPolicyIdGenerator = new(2);
    public static IdManager TaxPolicyIdGenerator = new(3);
    public static IdManager UserIdGenerator = new(4);
    public static IdManager GroupIdGenerator = new(5);
    public static IdManager TradeItemDefinitionIdGenerator = new(6);
    public static IdManager GroupRoleIdGenerator = new(7);
    public static IdManager ItemIdGenerator = new(8);
    public static IdManager ElectionIdGenerator = new(9);
    public static IdManager FactoryIdGenerator = new(10);
    public static IdManager MineIdGenerator = new(11);
}

public class IdManager
{
    public IdGenerator Generator { get; set; }

    public IdManager(int threadid = 0)
    {
        // Fun fact: This is the exact moment that SpookVooper was terminated
        // which led to the development of Valour becoming more than just a side
        // project. Viva la Vooperia.
        var epoch = new DateTime(2021, 1, 11, 4, 37, 0);

        var structure = new IdStructure(45, 10, 8);

        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));

        Generator = new IdGenerator(threadid, options);
    }

    public long Generate()
    {
        return (long)Generator.CreateId();
    }
}