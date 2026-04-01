using UnityEngine;

public class CoinPickup : PickUp
{
    PlayerCollector collector;
    public int coins = 1;

    // Prepíšeme (override) metódu z PickUp.cs, ktorá sa volá PRESNE vtedy, keï minca dobehne k hráèovi
    protected override void GrantRewardsAndDestroy()
    {
        if (target != null)
        {
            // Získame komponent PlayerCollector z hráèa, ktorý tento objekt zobral a pridáme mu mince
            collector = target.GetComponentInChildren<PlayerCollector>();
            if (collector != null)
            {
                collector.AddCoins(coins);
            }
        }

        // Nakoniec zavoláme pôvodnú funkciu z PickUp.cs.
        // Tá sa postará o odovzdanie XP a Health (ak náhodou minca nejaké má) a bezpeène zavolá Destroy(gameObject).
        base.GrantRewardsAndDestroy();
    }
}