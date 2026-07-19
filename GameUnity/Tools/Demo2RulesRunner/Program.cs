using GameBattle;

try
{
    Demo2RuleTestRunner.RunAll();
    Console.WriteLine("Demo2 rule tests passed.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
