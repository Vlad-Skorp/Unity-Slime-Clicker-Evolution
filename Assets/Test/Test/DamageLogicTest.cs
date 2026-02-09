using NUnit.Framework;
using SlimeRpgEvolution2D.Logic;
public class DamageLogicTest
{
    [Test]
    public void CalculateTotal_AddsBaseAndWeapon_Correctly()
    {
        int baseDmg = 10;
        int weaponDmg = 25;
        int expected = 35;

        int result = DamageCalculator.CalculateTotal(baseDmg, weaponDmg);    

        Assert.AreEqual(expected, result, "—ложение базового урона и оружи€ работает неверно!");
    }

    [Test]
    public void CalculateTotal_WithZeroDamage_ReturnsBase()
    {
        int result = DamageCalculator.CalculateTotal(10, 0);
        Assert.AreEqual(10, result);
    }

}
